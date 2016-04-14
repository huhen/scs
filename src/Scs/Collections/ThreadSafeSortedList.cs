using System.Collections.Generic;
using System.Threading;

namespace Hik.Collections
{
    /// <summary>
    /// This class is used to store key-value based items in a thread safe manner.
    /// It uses System.Collections.Generic.SortedList internally.
    /// </summary>
    /// <typeparam name="TK">Key type</typeparam>
    /// <typeparam name="TV">Value type</typeparam>
    public class ThreadSafeSortedList<TK, TV>
    {
        /// <summary>
        /// Gets/adds/replaces an item by key.
        /// </summary>
        /// <param name="key">Key to get/set value</param>
        /// <returns>Item associated with this key</returns>
        public TV this[TK key]
        {
            get
            {
                LockSlim.EnterReadLock();
                try
                {
                    return Items.ContainsKey(key) ? Items[key] : default(TV);
                }
                finally
                {
                    LockSlim.ExitReadLock();
                }
            }

            set
            {
                LockSlim.EnterWriteLock();
                try
                {
                    Items[key] = value;
                }
                finally
                {
                    LockSlim.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets count of items in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                LockSlim.EnterReadLock();
                try
                {
                    return Items.Count;
                }
                finally
                {
                    LockSlim.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Internal collection to store items.
        /// </summary>
        protected readonly SortedList<TK, TV> Items;

        /// <summary>
        /// Used to synchronize access to _items list.
        /// </summary>
        protected readonly ReaderWriterLockSlim LockSlim;

        /// <summary>
        /// Creates a new ThreadSafeSortedList object.
        /// </summary>
        public ThreadSafeSortedList()
        {
            Items = new SortedList<TK, TV>();
            LockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Checks if collection contains spesified key.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True; if collection contains given key</returns>
        public bool ContainsKey(TK key)
        {
            LockSlim.EnterReadLock();
            try
            {
                return Items.ContainsKey(key);
            }
            finally
            {
                LockSlim.ExitReadLock();
            }
        }

        /// <summary>
        /// Checks if collection contains spesified item.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>True; if collection contains given item</returns>
        public bool ContainsValue(TV item)
        {
            LockSlim.EnterReadLock();
            try
            {
                return Items.ContainsValue(item);
            }
            finally
            {
                LockSlim.ExitReadLock();
            }
        }

        /// <summary>
        /// Removes an item from collection.
        /// </summary>
        /// <param name="key">Key of item to remove</param>
        public bool Remove(TK key)
        {
            LockSlim.EnterWriteLock();
            try
            {
                if (!Items.ContainsKey(key))
                {
                    return false;
                }

                Items.Remove(key);
                return true;
            }
            finally
            {
                LockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets all items in collection.
        /// </summary>
        /// <returns>Item list</returns>
        public List<TV> GetAllItems()
        {
            LockSlim.EnterReadLock();
            try
            {
                return new List<TV>(Items.Values);
            }
            finally
            {
                LockSlim.ExitReadLock();
            }
        }

        /// <summary>
        /// Removes all items from list.
        /// </summary>
        public void ClearAll()
        {
            LockSlim.EnterWriteLock();
            try
            {
                Items.Clear();
            }
            finally
            {
                LockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets then removes all items in collection.
        /// </summary>
        /// <returns>Item list</returns>
        public List<TV> GetAndClearAllItems()
        {
            LockSlim.EnterWriteLock();
            try
            {
                var list = new List<TV>(Items.Values);
                Items.Clear();
                return list;
            }
            finally
            {
                LockSlim.ExitWriteLock();
            }
        }
    }
}
