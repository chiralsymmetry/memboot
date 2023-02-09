﻿using System;
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

        }

        public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count => _count;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item)
        {
            _firstItem++;
            _array[_firstItem] = item;
            _count++;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
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
