namespace Clifton.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public sealed class CircularList<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IDisposable, IEnumerator
    {
        private T[] items;
        private int idx;
        private bool loaded;
        private int enumIdx;

        public CircularList(int numItems)
        {
            if (numItems <= 0)
            {
                throw new ArgumentOutOfRangeException("numItems can't be negative or 0.");
            }
            this.items = new T[numItems];
            this.idx = 0;
            this.loaded = false;
            this.enumIdx = -1;
        }

        public void Clear()
        {
            this.idx = 0;
            this.items.Initialize();
            this.loaded = false;
        }

        public void Dispose()
        {
        }

        public IEnumerator<T> GetEnumerator() => 
            this;

        public bool MoveNext()
        {
            this.enumIdx++;
            bool flag = this.enumIdx < this.Count;
            if (!flag)
            {
                this.Reset();
            }
            return flag;
        }

        public void Next()
        {
            int num = this.idx + 1;
            this.idx = num;
            if (num == this.items.Length)
            {
                this.idx = 0;
                this.loaded = true;
            }
        }

        private void RangeCheck(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Indexer cannot be less than 0.");
            }
            if (index >= this.items.Length)
            {
                throw new ArgumentOutOfRangeException("Indexer cannot be greater than or equal to the number of items in the collection.");
            }
        }

        public void Reset()
        {
            this.enumIdx = -1;
        }

        public void SetAll(T val)
        {
            this.idx = 0;
            this.loaded = true;
            for (int i = 0; i < this.items.Length; i++)
            {
                this.items[i] = val;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this;

        public int Index =>
            this.idx;

        public T Value
        {
            get
            {
              return  this.items[this.idx];
            }
            set
            {
                this.items[this.idx] = value;
            }
        }

        public int Count =>
            (this.loaded ? this.items.Length : this.idx);

        public int Length =>
            this.items.Length;

        public T this[int index]
        {
            get
            {
                this.RangeCheck(index);
                return this.items[index];
            }
            set
            {
                this.RangeCheck(index);
                this.items[index] = value;
            }
        }

        public T Current =>
            this[this.enumIdx];

        object IEnumerator.Current =>
            this[this.enumIdx];
    }
}

