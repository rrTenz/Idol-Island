using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TransformChangesDebugger.Editor.GUI
{
    internal static class TransformChangesDebuggerStyles
    {
        public static readonly GUIContent PrevFrame = EditorGUIUtility.TrIconContent("Animation.PrevKey", "Previous frame");
        public static readonly GUIContent NextFrame = EditorGUIUtility.TrIconContent("Animation.NextKey", "Next frame");
        public static readonly GUIContent TrackChangesOff = EditorGUIUtility.TrIconContent("Record Off", "Track Changes");
        public static readonly GUIContent TrackChangesOn = EditorGUIUtility.TrIconContent("Record On", "Stop Tracking Changes");
        
        public static readonly GUIContent Frame = EditorGUIUtility.TrTextContent("Frame: ", "Selected newest frame with changes / Total number of frames with changes [actual frameCount value]");
        public static readonly GUIStyle ToolbarLabel = GetStyle("ToolbarLabel");
        public static readonly GUIStyle SidebarHeader = new GUIStyle(UnityEngine.GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 20
        };
        
        public static readonly GUIStyle EmphasisedText = new GUIStyle(UnityEngine.GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        
        public static readonly GUIStyle WrapText = new GUIStyle(UnityEngine.GUI.skin.label)
        {
            wordWrap = true
        };

        public static readonly GUIStyle ModifierToggle = new GUIStyle((GUIStyle) "OL Toggle");
        public static readonly GUIStyle AssyToPatchToggle = new GUIStyle((GUIStyle) "OL Toggle");

        private static readonly Texture HelpTexture = EditorGUIUtility.TrIconContent("_Help").image;

        private static readonly GUIStyle HelpTooltipStyle = new GUIStyle("IconButton");
        private static Dictionary<Vector4, GUIStyle> _rectOffsetToCachedHelpTooltipStyle = new Dictionary<Vector4, GUIStyle>(); 

        public static readonly int HelperTooltipWidth = 40;
        //using Vector4 for easy dict lookup
        public static void AddHelperTooltip(string content) => AddHelperTooltip(content, Vector4.zero);
        public static void AddHelperTooltip(string content, Vector4 margin)
        {
            GUIStyle tooltipStyleWithPadding;
            if (margin == Vector4.zero) tooltipStyleWithPadding = HelpTooltipStyle;
            else
            {
                if (!_rectOffsetToCachedHelpTooltipStyle.ContainsKey(margin))
                    _rectOffsetToCachedHelpTooltipStyle[margin] = new GUIStyle(HelpTooltipStyle)
                    {
                        margin = new RectOffset((int) margin.x, (int) margin.z, (int) margin.y, (int) margin.w)
                    };

                tooltipStyleWithPadding = _rectOffsetToCachedHelpTooltipStyle[margin];
            }
            
            var documentationIcon = new GUIContent(HelpTexture, content);
            GUILayout.Label(documentationIcon, tooltipStyleWithPadding, GUILayout.Width(HelperTooltipWidth));
        }
        
        internal static GUIStyle GetStyle(string styleName)
        {
            GUIStyle guiStyle = UnityEngine.GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (guiStyle == null)
            {
                Debug.LogError((object) ("Missing built-in guistyle " + styleName));
                guiStyle = new GUIStyle();
            }
            return guiStyle;
        }
        
        public static readonly Color OrangeColor = new Color(1, 0.65f, 0, 0.3f);
    }
}