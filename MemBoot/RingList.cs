using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot
{
    public class RingList<T> : IList<T>
    {
        private readonly T[] _array;
        private int _count;
        private int _firstItem;

        public RingList(int capacity)
        {
            _array = new T[capacity];
            _count = 0;
            _firstItem = 0;
        }

        private int LastItemSuccessor => (_firstItem + Count) % _array.Length;

        private T[] GetContents(int start, int count)
        {
            // Handle all cases:
            // []
            // [0, 1, 2, 3, _, _, _, _]
            // [_, _, 0, 1, 2, 3, _, _]
            // [_, _, _, _, 0, 1, 2, 3]
            // [2, 3, _, _, _, _, 0, 1]
            T[] contents;
            if (count == 0)
            {
                contents = Array.Empty<T>();
            }
            else if (LastItemSuccessor > start)
            {
                contents = _array.Skip(start).Take(count).ToArray();
            }
            else
            {
                int firstBatchCount = _array.Length - start;
                int secondBatchCount = count - firstBatchCount;
                contents = _array.Skip(start).Take(firstBatchCount)
                    .Concat(_array.Take(secondBatchCount)).ToArray();
            }
            return contents;
        }

        private T[] GetContents()
        {
            return GetContents(_firstItem, _count);
        }

        public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count => _count;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item)
        {
            _array[LastItemSuccessor] = item;
            if (_count < _array.Length)
            {
                _count++;
            }
            else
            {
                _firstItem = (_firstItem + 1) % _array.Length;
            }
        }

        public void Clear()
        {
            Array.Clear(_array);
            _count = 0;
            _firstItem = 0;
        }

        public bool Contains(T item)
        {
            return GetContents().Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            var list = new List<T>(GetContents());
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            return false;
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
