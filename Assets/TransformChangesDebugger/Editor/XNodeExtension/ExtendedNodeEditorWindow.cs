using System;
using System.Collections.Generic;
using System.Linq;
using ImmersiveVRTools.Runtime.Common.Extensions;
using TransformChangesDebugger.API;
using TransformChangesDebugger.API.Patches;
using TransformChangesDebugger.Editor.XNode;
using TransformChangesDebugger.Editor.XNodeExtension.EditorWindowAddons;
using TransformChangesDebugger.Runtime.GUI;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace TransformChangesDebugger.Editor.XNodeExtension {
    internal class ExtendedNodeEditorWindow: NodeEditorWindow
    {
        private static bool IsInitialized;
        
        private int GapBetweenRows { get; set; } = 30 ;
        private int GapBetweenColumns { get; set;  } = 30;
        private int NodeHeight { get; set; } = 130;
        
        private static Dictionary<Node, Vector2> NodeSizesAtLastAutoAlignment = new Dictionary<Node, Vector2>();
        private static readonly TrackedObjectsSidebar TrackedObjectsSidebar = new TrackedObjectsSidebar(NodeEditorWindowAddonAttachment.Left, 210, 0);
        private static readonly ModifiersSidebar ModifiersSidebar = new ModifiersSidebar(NodeEditorWindowAddonAttachment.Right, 0);
        private static readonly PatchedAssembliesBar PatchedAssembliesBar = new PatchedAssembliesBar(NodeEditorWindowAddonAttachment.Bottom, 0, 200);
        private static readonly HeaderBar HeaderBar = new HeaderBar(NodeEditorWindowAddonAttachment.Top, 0, 50);
        private static readonly TopConsoleBar TopConsoleBar = new TopConsoleBar(NodeEditorWindowAddonAttachment.Top, 0, true);

        private static Vector2 OpenAsWindowSize = new Vector2(1000, 600);
        
        private static readonly List<NodeEditorWindowAddon> EditorWindowAddons = new List<NodeEditorWindowAddon>()
        {
            TrackedObjectsSidebar,
            ModifiersSidebar,
            PatchedAssembliesBar,
            HeaderBar,
            TopConsoleBar
        };
        
        private static readonly Dictionary<ChangeType, Func<bool>> ChangeReasonToIsSelectedToShowByUserMap = new Dictionary<ChangeType, Func<bool>>()
        {
            [ChangeType.Unknown] = () => true,
            [ChangeType.Position] = () => TransformChangesDebuggerGuiManager.ShowChangesForPosition,
            [ChangeType.Rotation] = () => TransformChangesDebuggerGuiManager.ShowChangesForRotation,
            [ChangeType.Scale] = () => TransformChangesDebuggerGuiManager.ShowChangesForScale,
        }.AsAllElementsRequiredMap();
        
        protected override void OnGUI()
        {
            if(!IsInitialized) Initialize();
            
            TransformChangesDebuggerGuiManager.UpdateDataOnFrameStart(Time.frameCount);

            TemporalityChangeEventTypeToPreventMainGraphHandlingIfOriginatedInSideWindow(out var eventBeforeTemporaryTypeChange);

            base.OnGUI();
            
            Event.current.type = eventBeforeTemporaryTypeChange;
            RenderAddonWindows();

            if (Event.current.type == EventType.Layout)
            {
                AutoAlignNodesIfAnyRelevantLayoutChange();
            }
        }

        private void AutoAlignNodesIfAnyRelevantLayoutChange()
        {
            if(graph.nodes.Count == 0) return;
            
            if (!NodeSizesAtLastAutoAlignment.Any() || NodeSizesAtLastAutoAlignment.Count != nodeSizes.Count
                || NodeSizesAtLastAutoAlignment.Any(nodeAtLastRepositionKv => nodeSizes.TryGetValue(nodeAtLastRepositionKv.Key, out var size) 
                    ? size != nodeAtLastRepositionKv.Value 
                    : true))
            {
                var allAutoAligningNodes = graph.nodes.Cast<AutoAligningNodeBase>().ToList();
                foreach (var columnIndex in allAutoAligningNodes.Select(n => n.ColumnIndex).Distinct().OrderBy(c => c))
                {
                    var nodesForColumn = allAutoAligningNodes.Where(c => c.ColumnIndex == columnIndex).ToList();
                    foreach (var rowIndex in nodesForColumn.Select(n => n.RowIndex).Distinct().OrderBy(r => r))
                    {
                        var currentNode = nodesForColumn.Single(n => n.RowIndex == rowIndex);
                        var allPreviousNodes = nodesForColumn.Where(n => n.RowIndex < rowIndex).ToList();

                        currentNode.position.x = columnIndex * (AutoAligningNodeBase.NodeWidth + GapBetweenColumns);
                        currentNode.position.y = (allPreviousNodes.Sum(n => nodeSizes.TryGetValue(n, out var size) ? size.y : rowIndex * NodeHeight))
                                                 + allPreviousNodes.Count * GapBetweenRows;
                    }
                }
                
                NodeSizesAtLastAutoAlignment = nodeSizes.ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        private static void TemporalityChangeEventTypeToPreventMainGraphHandlingIfOriginatedInSideWindow(out EventType eventBeforeTemporaryTypeChange)
        {
            var renderableWindowAddons = GetCurrentFrameRenderableWindowAddons();
            var currentEvent = Event.current;
            eventBeforeTemporaryTypeChange = currentEvent.type;
            if ((currentEvent.type == EventType.ScrollWheel || currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) &&
                renderableWindowAddons.Any(r => r.LastOriginalScreenPosition.Contains(currentEvent.mousePosition)))
            {
                currentEvent.type = EventType.Ignore;
            }
        }

        private void OnSelectionChange()
        {
            var changedTransformNode = Selection.objects.Length > 0 ? Selection.objects[0] as TransformChangesDebuggerNodeBase : null;
            if (changedTransformNode)
            {
                TransformChangesDebuggerGuiManager.HandleSelectionChanged(changedTransformNode);
            }
        }

       
        internal void Initialize()
        {
            graph?.Clear();
            EnsureInitialized(this);
            
            TransformChangesDebuggerGuiManager.GraphDrawingDependentSettingChanged += (sender, args) =>
            {
                GenerateNodeGraphForSelection(args);
            };

            IsInitialized = true;
        }

        internal void MoveNodeToView(Node node) => MoveNodeToView(node, Vector2.zero);
        internal void MoveNodeToView(Node node, Vector2 panAdjustment)
        {
            SelectNode(node, false);

            var nodeDimension = nodeSizes.ContainsKey(node) ? nodeSizes[node] / 2 : Vector2.zero;
            panOffset = -node.position - nodeDimension + panAdjustment;
        }
        

        private void GenerateNodeGraphForSelection(GraphDrawingDependedSettingChangedEventArgs args)
        {
            EnsureInitialized(this);
            
            graph.nodes.Clear();

            var nodesToAdd = new List<Node>();
            var columnIndex = 0;

            var isFullMismatchWarningAlreadyRendered = false; //warning label should only be rendered once to save space
            foreach (var frameIndexToChangesForSelectedObjectKv in args.FrameIndexToChangesForSelectedObjectMap)
            {
                var rowIndex = 0;
                
                var frameIndicatorNode = ScriptableObject.CreateInstance<FrameIndicatorNode>();
                var frameIndex = frameIndexToChangesForSelectedObjectKv.Key;
                frameIndicatorNode.name = $"Frame: {frameIndex}";
                frameIndicatorNode.RowIndex = rowIndex++;
                frameIndicatorNode.ColumnIndex = columnIndex;
                frameIndicatorNode.graph = graph;
                graph.nodes.Add(frameIndicatorNode);

                foreach (var transformChange in frameIndexToChangesForSelectedObjectKv.Value)
                {
                    if (ChangeReasonToIsSelectedToShowByUserMap[transformChange.ChangeType]())
                    {
                        if (transformChange.IsMismatchWithPreviousChange())
                        {
                            var changeMismatchNode = ScriptableObject.CreateInstance<ChangeMismatchNode>();
                            changeMismatchNode.name = $"Value Mismatch - {transformChange.ChangeType}!";
                            changeMismatchNode.ExpectedValue = transformChange.PreviousSameTypeChange.NewValue;
                            changeMismatchNode.ActualValue = transformChange.ValueBeforeChange;
                            changeMismatchNode.ChangeType = transformChange.ChangeType;
                            if (!isFullMismatchWarningAlreadyRendered)
                            {
                                changeMismatchNode.RenderWarningAsLabel = true;
                                isFullMismatchWarningAlreadyRendered = true;
                            }

                            changeMismatchNode.RowIndex = rowIndex++;
                            changeMismatchNode.ColumnIndex = columnIndex;

                            graph.nodes.Add(changeMismatchNode);
                        }
                        
                        var changeNode = CreateChangeNode(transformChange, rowIndex++, columnIndex);
                        graph.nodes.Add(changeNode);
                    }
                }

                columnIndex++;
            }
        }

        private TransformChangeNode CreateChangeNode(TransformChange change, int rowIndex, int columnIndex)
        {
            TransformChangeNode changeNode = null;

            switch (change.ChangeType)
            {
                case ChangeType.Unknown:
                    changeNode = ScriptableObject.CreateInstance<TransformChangeNode>();
                    break;
                
                case ChangeType.Position:
                    var positionChangeNode = ScriptableObject.CreateInstance<PositionTransformChangeNode>();
                    changeNode = positionChangeNode;
                    positionChangeNode.Position = (Vector3) change.NewValue;
                    break;
                
                case ChangeType.Rotation:
                    var rotationChangeNode = ScriptableObject.CreateInstance<RotationTransformChangeNode>();
                    changeNode = rotationChangeNode;
                    rotationChangeNode.Rotation = (Quaternion) change.NewValue;
                    break;
                
                case ChangeType.Scale:
                    var scaleChangeNode = ScriptableObject.CreateInstance<ScaleTransformChangeNode>();
                    changeNode = scaleChangeNode;
                    scaleChangeNode.Scale = (Vector3) change.NewValue;
                    break;
                    
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            changeNode.name = $"{change.ChangeType}{(change.WasChangeSkipped ? " (SKIPPED)" : "")}";
            changeNode.RowIndex = rowIndex;
            changeNode.ColumnIndex = columnIndex;
            changeNode.MethodName = TransformChange.SimplifyMethodName(change.CallingFromMethodName, change.CallingObject.GetType());
            changeNode.ComparisonOnlyTransformModifier = new ComparisonOnlyTransformModifier(change.CallingObject, change.CallingFromMethodName);
            changeNode.OriginalMethodCall = new OriginalMethodCall(
                TransformChange.SimplifyMethodName(change.TrackedDueToInterceptedMethodCallFullName, typeof(Transform)),
                change.TrackedDueToInterceptedMethodArguments
            );
            changeNode.Originator = change.CallingObject;
            changeNode.ModifiedObject = change.ModifiedObject;
            changeNode.graph = graph;
            
            return changeNode;
        }

        public new static ExtendedNodeEditorWindow Open() {
            var w = GetWindow(typeof(ExtendedNodeEditorWindow), false, "Transform Changes Debugger", true) as ExtendedNodeEditorWindow;
            w.wantsMouseMove = true;
            w.minSize = OpenAsWindowSize;
            EnsureInitialized(w);
            return w;
        }

        //this is when EditorPrefs can be safely accessed
        private static void EnsureInitialized(ExtendedNodeEditorWindow w)
        {
            if (w.graph == null)
                w.graph = ScriptableObject.CreateInstance<TransformChangesGraph>();
            
            TransformChangesDebuggerGuiManager.Initialize(w);
            
        }

        private void RenderAddonWindows()
        {
            var renderableWindowAddons = GetCurrentFrameRenderableWindowAddons();
            var leftAttachment = renderableWindowAddons.SingleOrDefault(a => a.NodeEditorWindowAddonAttachment == NodeEditorWindowAddonAttachment.Left);
            var rightAttachment = renderableWindowAddons.SingleOrDefault(a => a.NodeEditorWindowAddonAttachment == NodeEditorWindowAddonAttachment.Right);
            var topAttachments = renderableWindowAddons.Where(a => a.NodeEditorWindowAddonAttachment == NodeEditorWindowAddonAttachment.Top).ToList();
            foreach (var editorWindowAddon in renderableWindowAddons)
            {
                var attachment = editorWindowAddon.NodeEditorWindowAddonAttachment;
                var xPosition = attachment == NodeEditorWindowAddonAttachment.Right ? position.width - editorWindowAddon.Width 
                    : attachment == NodeEditorWindowAddonAttachment.Top || attachment == NodeEditorWindowAddonAttachment.Bottom ? (leftAttachment?.Width ?? 0) 
                    : 0;
                
                var yPosition = attachment == NodeEditorWindowAddonAttachment.Bottom ? position.height - editorWindowAddon.Height
                    : attachment == NodeEditorWindowAddonAttachment.Top && topAttachments.Count > 1 
                        //if more top attachments add correct offset
                        ? GetYPositionForStackedAttachment(topAttachments, editorWindowAddon) 
                    : 0;

                var width = attachment == NodeEditorWindowAddonAttachment.Top || attachment == NodeEditorWindowAddonAttachment.Bottom 
                    ? position.width - (leftAttachment?.Width ?? 0) - (rightAttachment?.Width ?? 0)
                    : editorWindowAddon.Width != 0 ? editorWindowAddon.Width 
                        : position.width;

                if (attachment == NodeEditorWindowAddonAttachment.Top || attachment == NodeEditorWindowAddonAttachment.Bottom)
                {
                    editorWindowAddon.AdjustWidthToActualSize((int)width);
                }

                var contentHeight = editorWindowAddon.LastFinalLayoutRenderArea.y + editorWindowAddon.LastFinalLayoutRenderArea.height;
                var height = editorWindowAddon.FitHeightToContent
                    ?  contentHeight
                    : editorWindowAddon.Height != 0 ? editorWindowAddon.Height : position.height;
                var originalScreenPosition = new Rect(xPosition, yPosition, width, height);
                var windowPositionZoomAdjusted = CalculateSideWindowPositionZoomAdjusted(xPosition, yPosition, width, height, zoom);
                
                GUILayout.BeginArea(windowPositionZoomAdjusted, editorWindowAddon.WindowStyle);
                {
                    var editorWindowAsScrollableSidebar = editorWindowAddon as ScrollableBarEditorWindowAddon;
                    if (editorWindowAsScrollableSidebar != null)
                    {
                        editorWindowAsScrollableSidebar.BeginVerticalScrollView(height, contentHeight);
                    }

                    editorWindowAddon.Draw(originalScreenPosition);

                    if (editorWindowAsScrollableSidebar != null)
                        UnityEngine.GUI.EndScrollView();
                }
                GUILayout.EndArea();
            }
        }

        private static List<NodeEditorWindowAddon> GetCurrentFrameRenderableWindowAddons()
        {
            return EditorWindowAddons.Where(r => r.ShouldRender()).ToList();
        }

        private static int GetYPositionForStackedAttachment(List<NodeEditorWindowAddon> attachmentsInSamePosition, NodeEditorWindowAddon currentAttachment)
        {
            var currentAttachmentIndex = attachmentsInSamePosition.IndexOf(currentAttachment);
            if (currentAttachmentIndex == 0) return 0;
            
            return attachmentsInSamePosition.Take(currentAttachmentIndex).Sum(a => a.Height);
        }

        //TODO: this still does not adjust 100% correct when docked (at the bottom - large size)
        //Adjusting side window position as zoom seems to be messing with coordinates and they are rendered in wrong places
        private Rect CalculateSideWindowPositionZoomAdjusted(float xPosition, float yPosition, float width, float height, float zoomAdjustment)
        {
            //HACK: not 100% sure why this is needed, arrived at by trial and error, it seems that even with normal zoom adjustment y value will slowly grow,
            //22* zoom change seems to either counteract whatever that is or value difference is so small that it's no longer visible
            var magicAdjustment = ((zoomAdjustment - 1) * 22);
            
            //getting zoom difference from 1 and multiplying that by width / height allows to correctly place corner-windows
            return new Rect(
                xPosition + ((zoomAdjustment - 1) * position.width / 2), 
                yPosition + ((zoomAdjustment - 1) * position.height / 2) - magicAdjustment, 
                width, height);
        }
    }
}