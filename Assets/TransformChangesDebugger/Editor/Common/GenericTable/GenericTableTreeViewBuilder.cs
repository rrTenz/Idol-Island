using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.GenericTable
{
    public abstract class GenericTableTreeViewBuilder<TColumnData, TRowData>
    {
        private readonly GenericTableTreeViewState _state;
        private readonly List<TRowData> _allRowData;

        public GenericTableTreeViewBuilder(GenericTableTreeViewState state, List<TRowData> allRowData)
        {
            _state = state;
            _allRowData = allRowData;
        }
	    
        public GenericTableTreeView<TColumnData, TRowData> Create()
        {
            var firstInit = _state.MultiColumnHeaderState == null;
		    
            var columns = GenerateColumns(out var columnIdToColumnDataMap);
            var headerState =  new MultiColumnHeaderState(columns.ToArray());
                
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(_state.MultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(_state.MultiColumnHeaderState, headerState);
            // _state.MultiColumnHeaderState = headerState; //TODO: get it back to work, if needed that want's to be saved with editor-window
				            
            var multiColumnHeader = new MultiColumnHeader(headerState);
            if (firstInit)
                multiColumnHeader.ResizeToFit();

            var treeView = new GenericTableTreeView<TColumnData, TRowData>(_state.TreeViewState, multiColumnHeader, columnIdToColumnDataMap, _allRowData, RenderCell);
            treeView.Reload();
                
            // m_SearchField = new SearchField();
            // m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

            return treeView;
        }
	    
        private List<MultiColumnHeaderState.Column> GenerateColumns(out Dictionary<int, GenericTableTreeViewColumnData<TColumnData>> columnIdToControllerMappingsMap)
        {
            columnIdToControllerMappingsMap = new Dictionary<int, GenericTableTreeViewColumnData<TColumnData>>();
            var headers = new List<MultiColumnHeaderState.Column>();

            var columnDataEntries = GetColumnData();
		    
            for (var index = 0; index < columnDataEntries.Count; index++)
            {
                var columnData = columnDataEntries[index];

                headers.Add(CreateColumn(columnData));

                columnIdToControllerMappingsMap.Add(index, columnData);
            }

            return headers;
        }

        protected abstract List<GenericTableTreeViewColumnData<TColumnData>> GetColumnData();

        protected abstract MultiColumnHeaderState.Column CreateColumn(GenericTableTreeViewColumnData<TColumnData> columnData);
        protected abstract void RenderCell(GenericTableItem<TRowData> rowEntry, GenericTableTreeViewColumnData<TColumnData> columnData, Rect cellRect);
    }
}