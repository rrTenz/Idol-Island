using UnityEngine;

namespace TransformChangesDebugger.Editor
{
    internal class TransformChangesDebuggerEditorPrefs
    {
        public static readonly string RootGlobalKey = nameof(TransformChangesDebugger);
        public static readonly string RootProjectKey = $"{RootGlobalKey}.{Application.productName}";
        
        public static readonly string GeneralFlagSettings = $"{RootProjectKey}.GeneralFlagSettings";
        public static readonly string AssemblyMethodsToPatchCacheEditorPrefKey = $"{RootProjectKey}.AssemblyMethodsToPatchCache";
        public static readonly string AssemblyInfoToPatchEntries = $"{RootProjectKey}.AssemblyInfoToPatchEntries";
        public static readonly string AssemblyUserFriendlyGroupNameToIsOpenedMap = $"{RootProjectKey}.AssemblyUserFriendlyGroupNameToIsOpenedMap";
        public static readonly string UserChosenShowFrames = $"{RootProjectKey}.UserChosenShowFrames";
        public static readonly string UserChosenShowTransformChangesFor = $"{RootProjectKey}.UserChosenShowTransformChangesFor";
        public static readonly string TransformModifierToUseColorMap = $"{RootProjectKey}.TransformModifierToUseColorMap";
    }
}