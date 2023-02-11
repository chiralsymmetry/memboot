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

        private bool IsFull => _count == _array.Length;
        private int FirstItemPredecessor => (_array.Length + _firstItem - 1) % _array.Length;
        private int LastItemSuccessor => (_firstItem + Count) % _array.Length;

        private void IncrementFirstItemIndex() => _firstItem = (_firstItem + 1) % _array.Length;
        private void DecrementFirstItemIndex() => _firstItem = (_array.Length + _firstItem - 1) % _array.Length;
        private void IncrementCount() => _count = Math.Min(_array.Length, _count + 1);
        private void DecrementCount() => _count = Math.Max(0, _count - 1);
        private int InternalIndex(int externalIndex) => (_firstItem + externalIndex) % _array.Length;

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

        private void CopyInArray(T[] array, int src, int dst, int length)
        {
            if (length > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            else if (dst < src)
            {
                // Shift sub-array to the left.
                Array.Copy(_array, src, _array, dst, length);
            }
            else if (src < dst)
            {
                // Shift sub-array to the right.
                T[] tmpArray = new T[length];
                Array.Copy(_array, src, tmpArray, 0, length);
                Array.Copy(tmpArray, 0, _array, dst, length);
            }
            // Else elements are in the right place.
        }

        public T this[int index] { get => throw new ArgumentOutOfRangeException(); set => throw new NotImplementedException(); }

        public int Count => _count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            _array[LastItemSuccessor] = item;
            if (_count < _array.Length)
            {
                _count++;
            }
            else
            {
                IncrementFirstItemIndex();
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
            var contents = GetContents();
            Array.Copy(contents, array, Count);
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
            if (index < 0 || index > Count || index == _array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (index == Count)
            {
                Add(item);
            }
            else if (index == 0)
            {
                _array[FirstItemPredecessor] = item;
                DecrementFirstItemIndex();
                IncrementCount();
            }
            else if (IsFull)
            {
                int i = InternalIndex(index);
                if (FirstItemPredecessor < i)
                {
                    int count = i - _firstItem;
                    CopyInArray(_array, _firstItem, _firstItem - 1, count);
                    DecrementFirstItemIndex();
                    i--;
                }
                else
                {
                    int count = FirstItemPredecessor - i;
                    CopyInArray(_array, i, i + 1, count);
                }
                _array[i] = item;
            }
            else
            {
                int i = InternalIndex(index);
                if (i < LastItemSuccessor)
                {
                    int count = LastItemSuccessor - i;
                    CopyInArray(_array, i, i + 1, count);
                }
                else
                {
                    int count = i - _firstItem;
                    CopyInArray(_array, _firstItem, _firstItem - 1, count);
                    DecrementFirstItemIndex();
                }
                _array[i] = item;
                IncrementCount();
            }
        }

        private bool RemoveAtInternalIndex(int i)
        {
            bool result = false;
            if (i >= 0)
            {
                if (IsFull)
                {
                    if (i < _firstItem)
                    {
                        int count = LastItemSuccessor - i;
                        CopyInArray(_array, i + 1, i, count);
                    }
                    else
                    {
                        int count = i - _firstItem;
                        CopyInArray(_array, _firstItem, _firstItem + 1, count);
                        IncrementFirstItemIndex();
                    }
                }
                else
                {
                    if (i < LastItemSuccessor)
                    {
                        int count = LastItemSuccessor - 1;
                        CopyInArray(_array, i + 1, i, count);
                    }
                    else
                    {
                        int count = i - _firstItem;
                        CopyInArray(_array, _firstItem, _firstItem + 1, count);
                        IncrementFirstItemIndex();
                    }
                }
                DecrementCount();
                result = true;
            }
            return result;
        }

        public bool Remove(T item)
        {
            var i = InternalIndex(IndexOf(item));
            return RemoveAtInternalIndex(i);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            var i = InternalIndex(index);
            RemoveAtInternalIndex(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
