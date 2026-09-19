using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.Utilities
{
    public static class CodeEditorManager
    {
        public static void GotoScript(Component component, string methodName)
        {
            var monoScript = MonoScript.FromMonoBehaviour((MonoBehaviour)component);
            var scriptFile = new FileInfo(Application.dataPath + "\\..\\" + AssetDatabase.GetAssetPath(monoScript));
            
            var scriptFileTextLines = monoScript.text.Split(new[] {"\r\n"}, StringSplitOptions.None);
            var openAtLine = 0;
            for (var i = 0; i < scriptFileTextLines.Length; i++)
            {
                //TODO: quite naive to find a method in this way, will not work say for properties, but good enough for now
                if (Regex.IsMatch(scriptFileTextLines[i], $"^.*{methodName}\\(.*\\)[^;]*$"))
                {
                    openAtLine = i;
                    break;
                }
            }

            if (openAtLine == 0)
            {                    
                UnityEngine.Debug.LogWarning($"Unable to find method {methodName} in script. Opening at beginning.");
            }

            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(scriptFile.FullName, openAtLine + 1);
        }
    }
}