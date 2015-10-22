//using System;
//using System.Collections;
//using System.Collections.Generic;
//using Links.Core.Structures;

//namespace Links.Core
//{
//    unsafe partial class Links : IList<ulong>, IList<Pair>, IList<Link>
//    {
//        #region IEnumerable implementation

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return ((IEnumerable<ulong>)this).GetEnumerator();
//        }

//        #endregion

//        #region IEnumerable<ulong> implementation

//        /// <summary>Unsafe code may not appear in iterators.</summary>
//        private ulong GetEnumerator_AllocatedLinks
//        {
//            get
//            {
//                return _header->AllocatedLinks;
//            }
//        }

//        public IEnumerator<ulong> GetEnumerator()
//        {
//            try
//            {
//                _rwLock.EnterReadLock();

//                // Часть этого кода скопирована из Each и должена соответствовать блоку из этой функции
//                for (ulong link = 1; link <= GetEnumerator_AllocatedLinks; link++)
//                    if (Exists(link))
//                        yield return link;
//            }
//            finally
//            {
//                _rwLock.ExitReadLock();
//            }
//        }

//        #endregion

//        #region ICollection<ulong> implementation

//        public void Add(ulong item)
//        {
//            Create(item, item);
//        }

//        public void CopyTo(ulong[] array, int arrayIndex)
//        {
//            ExecuteReadOperation(() =>
//            {
//                for (var link = (ulong) arrayIndex + 1; link <= _header->AllocatedLinks; link++)
//                    if (ExistsCore(link))
//                        array[link] = link;
//                    else
//                        array[link] = 0;
//            });
//        }

//        public void Clear()
//        {
//            ExecuteWriteOperation(() =>
//            {
//                for (var link = _header->AllocatedLinks; link >= 1; link--)
//                    DeleteCore(link);
//            });
//        }

//        public bool Contains(ulong item)
//        {
//            return Exists(item);
//        }

//        int ICollection<ulong>.Total
//        {
//            get { return (int)Total; }
//        }

//        public bool IsReadOnly
//        {
//            get { return false; }
//        }

//        public bool Remove(ulong item)
//        {
//            try
//            {
//                Delete(item);
//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        #endregion

//        #region IList<ulong> implementation

//        int IList<ulong>.IndexOf(ulong item)
//        {
//            return (int)(Search(item, item) - 1);
//        }

//        void IList<ulong>.Insert(int index, ulong item)
//        {
//            Create(item, item);
//        }

//        void IList<ulong>.RemoveAt(int index)
//        {
//            Delete((ulong)index + 1);
//        }

//        ulong IList<ulong>.this[int index]
//        {
//            get
//            {
//                var link = (ulong)(index + 1);
//                return Exists(link) ? link : 0;
//            }
//            set
//            {
//                var link = (ulong)(index + 1);

//                if (value == 0)
//                    Delete(link);
//                else
//                    throw new NotImplementedException();
//            }
//        }

//        #endregion

//        #region IEnumerable<Pair> implementation

//        IEnumerator<Pair> IEnumerable<Pair>.GetEnumerator()
//        {
//            try
//            {
//                _rwLock.EnterReadLock();

//                // Часть этого кода скопирована из Each и должена соответствовать блоку из этой функции
//                for (ulong link = 1; link <= GetEnumerator_AllocatedLinks; link++)
//                    if (ExistsCore(link))
//                        yield return new Pair(GetSourceCore(link), GetTargetCore(link));
//            }
//            finally
//            {
//                _rwLock.ExitReadLock();
//            }
//        }

//        #endregion

//        #region ICollection<Pair> implementation

//        public void Add(Pair item)
//        {
//            Create(item.Source, item.Target);
//        }

//        void ICollection<Pair>.Clear()
//        {
//            Clear();
//        }

//        public bool Contains(Pair item)
//        {
//            return Search(item.Source, item.Target) != 0;
//        }

//        public void CopyTo(Pair[] array, int arrayIndex)
//        {
//            try
//            {
//                _rwLock.EnterReadLock();

//                for (var link = (ulong)arrayIndex + 1; link <= _header->AllocatedLinks; link++)
//                    if (ExistsCore(link))
//                        array[link] = new Pair(GetSourceCore(link), GetTargetCore(link));
//                    else
//                        array[link] = new Pair(0, 0);
//            }
//            finally
//            {
//                _rwLock.ExitReadLock();
//            }
//        }

//        int ICollection<Pair>.Total
//        {
//            get { return (int)Total; }
//        }

//        bool ICollection<Pair>.IsReadOnly
//        {
//            get { return false; }
//        }

//        public bool Remove(Pair item)
//        {
//            try
//            {
//                Delete(item.Source, item.Target);
//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        #endregion

//        #region IList<Pair> implementation

//        int IList<Pair>.IndexOf(Pair item)
//        {
//            return (int)Search(item.Source, item.Target);
//        }

//        void IList<Pair>.Insert(int index, Pair item)
//        {
//            Create(item.Source, item.Target);
//        }

//        void IList<Pair>.RemoveAt(int index)
//        {
//            var link = (ulong)(index + 1);
//            Delete(link);
//        }

//        Pair IList<Pair>.this[int index]
//        {
//            get
//            {
//                try
//                {
//                    _rwLock.EnterReadLock();

//                    var link = (ulong)(index + 1);
//                    return ExistsCore(link) ? new Pair(GetSourceCore(link), GetTargetCore(link)) : new Pair(0, 0);
//                }
//                finally
//                {
//                    _rwLock.ExitReadLock();
//                }
//            }
//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        #endregion

//        #region IEnumerable<Link> implementation

//        IEnumerator<Structures.Link> IEnumerable<Structures.Link>.GetEnumerator()
//        {
//            try
//            {
//                _rwLock.EnterReadLock();

//                // Часть этого кода скопирована из Each и должена соответствовать блоку из этой функции
//                for (ulong link = 1; link <= GetEnumerator_AllocatedLinks; link++)
//                    if (ExistsCore(link))
//                        yield return new Structures.Link(link, GetSourceCore(link), GetTargetCore(link));
//            }
//            finally
//            {
//                _rwLock.ExitReadLock();
//            }
//        }

//        #endregion

//        #region ICollection<Link> implementation

//        void ICollection<Structures.Link>.Add(Structures.Link item)
//        {
//            throw new NotImplementedException();
//        }

//        void ICollection<Structures.Link>.Clear()
//        {
//            Clear();
//        }

//        bool ICollection<Structures.Link>.Contains(Structures.Link item)
//        {
//            throw new NotImplementedException();
//        }

//        void ICollection<Structures.Link>.CopyTo(Structures.Link[] array, int arrayIndex)
//        {
//            throw new NotImplementedException();
//        }

//        int ICollection<Structures.Link>.Total
//        {
//            get { return (int)Total; }
//        }

//        bool ICollection<Structures.Link>.IsReadOnly
//        {
//            get { return false; }
//        }

//        bool ICollection<Structures.Link>.Remove(Structures.Link item)
//        {
//            throw new NotImplementedException();
//        }

//        #endregion

//        #region IList<Link> implementation

//        int IList<Structures.Link>.IndexOf(Structures.Link item)
//        {
//            throw new NotImplementedException();
//        }

//        void IList<Structures.Link>.Insert(int index, Structures.Link item)
//        {
//            throw new NotImplementedException();
//        }

//        void IList<Structures.Link>.RemoveAt(int index)
//        {
//            throw new NotImplementedException();
//        }

//        Structures.Link IList<Structures.Link>.this[int index]
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }
//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        #endregion

//        #region Utility methods

//        private ulong[] ToArray()
//        {
//            var array = new ulong[_header->AllocatedLinks];
//            CopyTo(array, 0);
//            return array;
//        }

//        #endregion
//    }
//}

namespace Platform.Links.DataBase.CoreUnsafe.Pairs
{
}