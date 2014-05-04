using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastleRenderer.Structures
{
    /// <summary>
    /// Represents a list that is always sorted using the given comparer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrderedList<T> : IEnumerable<T>
    {
        private List<T> internal_list;
        private IComparer<T> comparer;

        /// <summary>
        /// Initialises a new instance of the OrderedList<T> class that is empty and has the default initial capacity.
        /// </summary>
        /// <param name="comparer"></param>
        public OrderedList(IComparer<T> comparer)
        {
            internal_list = new List<T>();
            this.comparer = comparer;
        }

        /// <summary>
        /// Initialises a new instance of the OrderedList<T> class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="comparer"></param>
        public OrderedList(IComparer<T> comparer, int capacity)
        {
            internal_list = new List<T>(capacity);
            this.comparer = comparer;
        }

        /// <summary>
        /// Gets the index of the specified item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            return internal_list.IndexOf(item);
        }

        /// <summary>
        /// Returns the item at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                return internal_list[index];
            }
        }

        /// <summary>
        /// Adds an item into this ordered list. The item will be sorted into the correct place
        /// </summary>
        /// <param name="item"></param>
        public bool Add(T item)
        {
            if (comparer != null)
                for (int i = 0; i < internal_list.Count; i++)
                {
                    int cmp = comparer.Compare(item, internal_list[i]);
                    if (cmp < 0)
                    {
                        internal_list.Insert(i, item);
                        return true;
                    }
                }
            internal_list.Add(item);
            return true;
        }

        /// <summary>
        /// Adds a range of items into this ordered list. The items will be sorted into the correct place
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
                internal_list.Add(item);
            if (comparer != null) internal_list.Sort(comparer);
        }

        /// <summary>
        /// Clears this list of all items
        /// </summary>
        public void Clear()
        {
            internal_list.Clear();
        }

        /// <summary>
        /// Returns if this list contains the specified item or not
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return internal_list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            internal_list.CopyTo(array, arrayIndex);
        }

        public void CopyTo(T[] array)
        {
            internal_list.CopyTo(array);
        }

        public int Count
        {
            get { return internal_list.Count; }
        }

        public bool Remove(T item)
        {
            return internal_list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new OrderedListEnum<T>(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class OrderedListEnum<T> : IEnumerator<T>
    {
        private int index;
        private OrderedList<T> list;

        public OrderedListEnum(OrderedList<T> list)
        {
            this.list = list;
            index = -1;
            Current = default(T);
        }

        public T Current { get; private set; }

        public void Dispose()
        {
            index = -1;
            list = null;
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            index++;
            if (index >= list.Count)
            {
                Current = default(T);
                return false;
            }
            Current = list[index];
            return true;
        }

        public void Reset()
        {
            index = -1;
            Current = default(T);
        }
    }
}
