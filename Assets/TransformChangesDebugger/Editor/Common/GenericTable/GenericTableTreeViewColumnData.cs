namespace ImmersiveVRTools.Editor.Common.GenericTable
{
    public class GenericTableTreeViewColumnData<TData>
    {
        public object NonGenericColumnData { get; }
        public TData Data { get; }

        public GenericTableTreeViewColumnData(TData data)
        {
            Data = data;
        }

        public GenericTableTreeViewColumnData(object nonGenericColumnData)
        {
            NonGenericColumnData = nonGenericColumnData;
        }
    }
}