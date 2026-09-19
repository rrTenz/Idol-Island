using System;
using System.Collections;
using System.Collections.Generic;

namespace ImmersiveVRTools.Runtime.Common.DataStructure
{
    public class RingBufferList<T> : IList<T>
    {
        private readonly List<T> _list;
        private int _currentStartOfList;
        private readonly int _maxSize;

        public RingBufferList(int maxSize)
        {
            _currentStartOfList = 0;
            _maxSize = maxSize;
            _list = new List<T>();
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _list.Count)
                    throw new IndexOutOfRangeException();

                return _list[(_currentStartOfList + index) % _maxSize];
            }
            set => throw new NotSupportedException();
        }

        public void Add(T item)
        {
            if (_list.Count < _maxSize)
            {
                _list.Add(item);
            }
            else
            {
                // Override the last item before the new first item of the list
                _list[_currentStartOfList] = item;
                _currentStartOfList = (_currentStartOfList + 1) % _maxSize;
            }
        }

        public void Clear()
        {
            _currentStartOfList = 0;
            _list.Clear();
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (var i = 0; i < _list.Count; i++)
            {
                var realIndex = (_currentStartOfList + i) % _maxSize;
                array[arrayIndex + i] = _list[realIndex];
            }
        }


        public int IndexOf(T item)
        {
            var baseIndex = _list.IndexOf(item);
            if (baseIndex == -1) // not found?
                return -1;

            if (baseIndex >= _currentStartOfList)
                return baseIndex - _currentStartOfList;
            return _list.Count - _currentStartOfList + baseIndex;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _list.Count; i++)
            {
                var realIndex = (_currentStartOfList + i) % _maxSize;
                yield return _list[realIndex];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region "Unsupported Operations"

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}