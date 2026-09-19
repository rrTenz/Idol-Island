using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TransformChangesDebugger.API;
using TransformChangesDebugger.API.Patches;
using TransformChangesDebugger.Runtime;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TransformChangesDebugger.Editor
{
    [InitializeOnLoad]
    internal class TransformChangesDebuggerInitializer
    { 
        static TransformChangesDebuggerInitializer()
        {
            EnsureAPICompatibilitySetTo46(out var shouldStopInitialization);
            if(shouldStopInitialization) return;

            //Fired just before app starts (at that time static values are still present from previous session)
            AppDomain.CurrentDomain.DomainUnload += (sender, args) =>
            {
                bool isApplicationPlaying;
                try
                {
                    //calling isPlaying will throw exception when application is closing, otherwise going further will cause FatalError and Unity will terminate
                    isApplicationPlaying = !Application.isPlaying;
                }
                catch (Exception e)
                {
                    Debug.Log("Unable to get Application.isPlaying in DomainUnload - this error is handling issue where on editor closing accessing perf settings will cause Fatal Exception, " +
                              "if you're seeing it outside of closing application / log file scenario then something else might be wrong, please contact support.");
                    return;
                }
                
                if(isApplicationPlaying)
                    PersistDataBetweenSessions();

                TransformChangesTracker.RemoveAllTrackedData();
            };

            SceneManager.sceneLoaded += (arg0, mode) =>
            {
                if(EditorApplication.isPlayingOrWillChangePlaymode) return;

                var isAnyObjectTracked = !!GameObject.FindObjectOfType<TrackTransformChanges>();
                TransformChangesDebuggerGuiManager.ToggleMessageToUser(!isAnyObjectTracked, NothingTrackedMessage);
            };

            TransformPatches.InterceptMethodsToEnableChangeTrackingStarted += (sender, args) =>
            {     
                //TODO: patch method could have some callback / event that'd signal how much is done, this could be used to give better indication for progress bar
                EditorUtility.DisplayProgressBar("Please wait...", "Patching assemblies to enable change tracking.", 0.3f);
            };

            TransformPatches.InterceptMethodsToEnableChangeTrackingCompleted += (sender, result) =>
            {
                EditorUtility.ClearProgressBar();
            };
            
            InitializeDataPersistedBetweenSessionsSafe();
            
            var rootApplicationFolder = new DirectoryInfo(Application.dataPath + "/.."); 
            var allAvailableAssemblyPaths = CompilationPipeline.GetAssemblies()
                .SelectMany(a => new List<string> { a.outputPath  }.Concat(a.allReferences))
                .Distinct()
                .Select(path => new FileInfo(ResolveToFullAssemblyPath(rootApplicationFolder, path)))
                .OrderBy(assyFile => assyFile.Name)
                .ToList();
            
            var userChosenAssembliesToPatch = new List<FileInfo>();
            var userChosenAssembliesToPatchWrapper = JsonUtility.FromJson<AssemblyPatchInfoWrapper>(EditorPrefs.GetString(TransformChangesDebuggerEditorPrefs.AssemblyInfoToPatchEntries));
            if (userChosenAssembliesToPatchWrapper == null || !userChosenAssembliesToPatchWrapper.AssemblyPatchInfoEntries.Any())
            {
                userChosenAssembliesToPatch.AddRange(GetDefaultAssembliesToPatch(allAvailableAssemblyPaths));
            }
            else
            {
                userChosenAssembliesToPatch.AddRange(userChosenAssembliesToPatchWrapper.AssemblyPatchInfoEntries
                    .Where(a => a.IsPatchingEnabled)
                    .Select(a => new FileInfo(a.AssemblyFilePath)
                ));
            }

            TransformPatches.InterceptMethodsToEnableChangeTrackingCompleted += (sender, result) =>
            {
                TransformChangesDebuggerGuiManager.UpdateAssemblyToPatchGuiData(result.AssemblyResults);
            };

            TransformChangesDebuggerManager.IsTrackingEnabled = TransformChangesDebuggerGuiManager.IsTrackingEnabled;
            
            TransformChangesDebuggerManager.Initialize(allAvailableAssemblyPaths, userChosenAssembliesToPatch);
            
            TransformChangesDebuggerGuiManager.UpdateAssemblyToPatchGuiData(new List<RedirectSetterMethodsFromCallingCodeForAssyResult>());
        }

        private static void EnsureAPICompatibilitySetTo46(out bool shouldStopInitialization)
        {
            var compatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(
                    EditorUserBuildSettings.selectedBuildTargetGroup);
            if (compatibilityLevel != ApiCompatibilityLevel.NET_4_6)
            {
                var response = EditorUtility.DisplayDialog("Transform Changes Debugger - not supported",
                    $"Your API compatibility level ({compatibilityLevel}) is not supported." +
                    $"\n\nTo use the tool you need to change to API Compatibility to .NET 4.x",
                    "Ok, change that for me",
                    "No, I'm not going to change"
                );

                if (response)
                {
                    PlayerSettings.SetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup, ApiCompatibilityLevel.NET_4_6);
                }

                shouldStopInitialization = true;
            }

            shouldStopInitialization = false;
        }

        private static void PersistDataBetweenSessions()
        {
            EditorPrefs.SetString(
                TransformChangesDebuggerEditorPrefs.AssemblyMethodsToPatchCacheEditorPrefKey,
                JsonUtility.ToJson(new SerializableAssemblyMethodsToPatchCacheWrapper(TransformPatches
                        .AssemblyMethodsToPatchCache
                        .Select(kv => new SerializableAssemblyMethodsToPatchCacheEntry(kv.Key, kv.Value))
                        .ToList()
                    )
                )
            );

            EditorPrefs.SetString(
                TransformChangesDebuggerEditorPrefs.AssemblyInfoToPatchEntries,
                JsonUtility.ToJson(
                    new AssemblyPatchInfoWrapper(TransformChangesDebuggerGuiManager.AssemblyPatchInfoEntries) 
                )
            );
            
            TransformChangesDebuggerGuiManager.UpdateAssemblyToPatchGuiData(new List<RedirectSetterMethodsFromCallingCodeForAssyResult>());
        }

        public static IReadOnlyList<string> DefaultAssemblyNamesToPatch = new List<string>()
        {
            "Assembly-CSharp.dll"
        };

        public static readonly string NothingTrackedMessage = "No objects tracked. Assemblies will not be processed to save time. Add 'TrackTransformChanges' to game object that you wish to track and restart play mode.";

        private static List<FileInfo> GetDefaultAssembliesToPatch(List<FileInfo> allAvailableAssemblyPaths)
        {
            return allAvailableAssemblyPaths.Where(a => DefaultAssemblyNamesToPatch.Contains(a.Name)).ToList();
        }

        private static void InitializeDataPersistedBetweenSessionsSafe()
        {
            //TODO: if more methods are going to be initialized, come up with some common way to do that, same for persisting
            InitializeAssemblyMethodsToPatchCacheSafe();
            InitializeAssemblyPatchInfoEntriesSafe();
        }

        private static void InitializeAssemblyPatchInfoEntriesSafe()
        {
            try
            {
                var assemblyInfoToPatchEntries = EditorPrefs.GetString(TransformChangesDebuggerEditorPrefs.AssemblyInfoToPatchEntries);
                var cachedValues = JsonUtility.FromJson<AssemblyPatchInfoWrapper>(assemblyInfoToPatchEntries);
                if (cachedValues != null)
                {
                    TransformChangesDebuggerGuiManager.AssemblyPatchInfoEntries = cachedValues.AssemblyPatchInfoEntries;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Unable to load {nameof(TransformChangesDebuggerGuiManager.AssemblyPatchInfoEntries)}");
            }
        }

        private static void InitializeAssemblyMethodsToPatchCacheSafe()
        {
            try
            {
                var assemblyMethodsToPatchCachedJsonString = EditorPrefs.GetString(TransformChangesDebuggerEditorPrefs.AssemblyMethodsToPatchCacheEditorPrefKey);
                var cachedValues = JsonUtility.FromJson<SerializableAssemblyMethodsToPatchCacheWrapper>(assemblyMethodsToPatchCachedJsonString);
                if (cachedValues != null)
                {
                    TransformPatches.AssemblyMethodsToPatchCache =
                        cachedValues.SerializableAssemblyMethodsToPatchCacheEntries.ToDictionary(
                            e => e.MethodToPathCacheKey,
                            e => e.CachedAssemblyWithMethodsToPatchInfo
                    );
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Unable to load {nameof(TransformPatches.AssemblyMethodsToPatchCache)}");
            }
        }

        private static string ResolveToFullAssemblyPath(DirectoryInfo rootApplicationFolder, string assyPath)
        {
            if (!File.Exists(assyPath))
            {
                var fullAssyPath = Path.Combine(rootApplicationFolder.FullName, assyPath);
                if (File.Exists(fullAssyPath))
                {
                    return fullAssyPath;
                }
                else
                {
                    Debug.LogWarning($"Unable to find assembly: '{assyPath}'");
                }
            }

            return assyPath;
        }
    }

    [Serializable]
    public class SerializableAssemblyMethodsToPatchCacheWrapper
    {
        public List<SerializableAssemblyMethodsToPatchCacheEntry> SerializableAssemblyMethodsToPatchCacheEntries;

        public SerializableAssemblyMethodsToPatchCacheWrapper(List<SerializableAssemblyMethodsToPatchCacheEntry> serializableAssemblyMethodsToPatchCacheEntries)
        {
            SerializableAssemblyMethodsToPatchCacheEntries = serializableAssemblyMethodsToPatchCacheEntries;
        }

        public SerializableAssemblyMethodsToPatchCacheWrapper()
        {
        }
    }
    
    [Serializable]
    public class SerializableAssemblyMethodsToPatchCacheEntry
    {
        public MethodToPathCacheKey MethodToPathCacheKey;
        public CachedAssemblyWithMethodsToPatchInfo CachedAssemblyWithMethodsToPatchInfo;

        public SerializableAssemblyMethodsToPatchCacheEntry(MethodToPathCacheKey methodToPathCacheKey, CachedAssemblyWithMethodsToPatchInfo cachedAssemblyWithMethodsToPatchInfo)
        {
            MethodToPathCacheKey = methodToPathCacheKey;
            CachedAssemblyWithMethodsToPatchInfo = cachedAssemblyWithMethodsToPatchInfo;
        }
    }

    [Serializable]
    public class AssemblyPatchInfoWrapper
    {
        public List<AssemblyPatchInfo> AssemblyPatchInfoEntries;

        public AssemblyPatchInfoWrapper(List<AssemblyPatchInfo> assemblyPatchInfoEntries)
        {
            AssemblyPatchInfoEntries = assemblyPatchInfoEntries;
        }
    }
}