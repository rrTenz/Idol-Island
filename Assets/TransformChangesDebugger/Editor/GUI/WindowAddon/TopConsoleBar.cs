using System;
using System.Linq;
using ImmersiveVRTools.Runtime.Common.Utilities;
using TransformChangesDebugger.Editor;
using TransformChangesDebugger.Editor.XNodeExtension.EditorWindowAddons;
using UnityEngine;

namespace XNodeEditor
{
    internal class TopConsoleBar : BarEditorWindowAddon
    {
        private GUIStyle _windowStyle;
        public override GUIStyle WindowStyle
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

        private GUIStyle InitStyles()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                normal = {background = TextureHelper.CreateGuiBackgroundColor(new Color( 181f, 181f, 0f, 0.15f ))}
            };

            return style;
        }
        
        private GUIStyle _labelStyle;

        private GUIStyle LabelStyle
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
                }

                return _labelStyle;
            }
        }

        public TopConsoleBar(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int width, int height) : base(nodeEditorWindowAddonAttachment, width, height)
        {
        }

        public TopConsoleBar(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int width, bool fitHeightToContent) : base(nodeEditorWindowAddonAttachment, width, fitHeightToContent)
        {
        }

        public override bool ShouldRender()
        {
            return TransformChangesDebuggerGuiManager.UserMessages.Any();
        }

        protected override void DrawInternal()
        {
            if (TransformChangesDebuggerGuiManager.UserMessages.Any())
            {
                foreach (var message in TransformChangesDebuggerGuiManager.UserMessages)
                {
                    try
                    {
                        GUILayout.Label(message, LabelStyle);
                        GUILayout.Space(5);
                    }
                    catch (Exception e)
                    {
                        //if collection changes between event calls this will cause error, in that case it'll be swallowed silently
                    }
                }
            }
            else
            {
                //render anything - otherwise general GetRect call will fail
                GUILayout.Space(1);
            }
        }
    }
}