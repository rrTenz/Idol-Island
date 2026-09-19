using UnityEngine;
using ImmersiveVRTools.Runtime.Common.Utilities;

namespace ImmersiveVRTools.Editor.Common.Utilities
{
    public class CommonEditorUI
    {
        private static GUIStyle _windowStyle;
        public static GUIStyle WindowStyle
        {
            get
            {
                //background texture sometimes is null after domain reload, this will prevent that
                if (_windowStyle == null || _windowStyle.normal.background == null)
                {
                    _windowStyle = InitStyles();
                }
                return _windowStyle;
            }
        }
        
        private static GUIStyle _labelStyle;

        private static GUIStyle InfoMessageLabelStyle
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle(GUI.skin.label)
                    {
                        wordWrap = true,
                        fontStyle = FontStyle.Bold,
                        fontSize = 14
                    };
                    
                    _labelStyle.normal.background = TextureHelper.CreateGuiBackgroundColor(new Color( 181f, 181f, 0f, 0.15f ));
                }

                return _labelStyle;
            }
        }
        
        private static GUIStyle InitStyles()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                normal = { background = TextureHelper.CreateGuiBackgroundColor(new Color( 181f, 181f, 0f, 0.15f )) }
            };

            return style;
        }
        
        public static void InfoMessage(string message)
        {
            GUILayout.Label(message, InfoMessageLabelStyle);
        }
    }
}