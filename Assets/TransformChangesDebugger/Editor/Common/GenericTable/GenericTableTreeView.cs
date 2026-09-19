using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.GenericTable
{
    public delegate void RenderCellFn<TRowData, TColumnData> (GenericTableItem<TRowData> rowEntry, TColumnData column, Rect cellRect);


    public class GenericTableTreeView<TColumnData, TRowData> : TreeView
    {
        private readonly Dictionary<int, GenericTableTreeViewColumnData<TColumnData>> _columnIdToControllerMappingsMap;
        private readonly List<TRowData> _allRowEntries;
        private readonly RenderCellFn<TRowData, GenericTableTreeViewColumnData<TColumnData>> _renderCellFn;

        public GenericTableTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader,
            Dictionary<int, GenericTableTreeViewColumnData<TColumnData>> columnIdToControllerMappingsMap,
            List<TRowData> allRowEntries,
            RenderCellFn<TRowData, GenericTableTreeViewColumnData<TColumnData>> renderCellFn
        )
            : base(state, multiColumnHeader)
        {
            _columnIdToControllerMappingsMap = columnIdToControllerMappingsMap;
            _allRowEntries = allRowEntries;
            _renderCellFn = renderCellFn;
            rowHeight = 18;
            // columnIndexForTreeFoldouts = 2;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            // customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            // extraSpaceBeforeIconAndLabel = kToggleWidth;
            // multicolumnHeader.sortingChanged += OnSortingChanged;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};

            for (var index = 0; index < _allRowEntries.Count; index++)
            {
                var rowEntry = _allRowEntries[index];
                root.AddChild(new GenericTableItem<TRowData>(index + 1, 0, string.Empty, rowEntry));
            }

            return root;
        }
        
        protected override void RowGUI (RowGUIArgs args)
        {
            for (var i = 0; i < args.GetNumVisibleColumns (); ++i)
            {
                var rowData = args.item as GenericTableItem<TRowData>;
                var cellRect = args.GetCellRect(i);
                var col = args.GetColumn(i);

                var columnData = _columnIdToControllerMappingsMap[col];
                _renderCellFn(rowData, columnData, cellRect);
            }
        }
    }
}