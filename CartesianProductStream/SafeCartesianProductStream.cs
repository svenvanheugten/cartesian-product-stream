using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CartesianProduct;

namespace SmartCartesianProduct
{
    public class SafeCartesianProductStream : ICartesianProductStream
    {
        private readonly FreezableBag _xs;
        private readonly FreezableBag _ys;
        private Action<(int, int)> _callback;

        public SafeCartesianProductStream()
        {
            _xs = new FreezableBag();
            _ys = new FreezableBag();
        }
        
        public void AddX(int x)
        {
            _ys.Freeze();

            _xs.Add(x);

            var snapshot = _ys.GetSnapshot();
            
            _ys.Unfreeze();

            foreach (var y in snapshot)
            {
                _callback((x, y));
            }
        }

        public void AddY(int y)
        {
            _xs.Freeze();

            _ys.Add(y);

            var snapshot = _xs.GetSnapshot();
            
            _xs.Unfreeze();

            foreach (var x in snapshot)
            {
                _callback((x, y));
            }
        }

        public void Subscribe(Action<(int, int)> action)
        {
            _callback = action;
        }
        
        private class FreezableBag
        {
            private readonly ReaderWriterLock _readerWriterLock;
            private readonly IList<ConcurrentBag<int>> _concurrentBags;

            public FreezableBag()
            {
                _readerWriterLock = new ReaderWriterLock();
                _concurrentBags = new List<ConcurrentBag<int>>
                {
                    new ConcurrentBag<int>()
                };
            }

            public void Add(int value)
            {
                _readerWriterLock.AcquireReaderLock(TimeSpan.FromDays(1));
                _concurrentBags[^1].Add(value);
                _readerWriterLock.ReleaseReaderLock();
            }

            public IEnumerable<int> GetSnapshot()
            {
                if (!_readerWriterLock.IsWriterLockHeld)
                {
                    throw new InvalidOperationException();
                }

                var concurrentBagsLength = _concurrentBags.Count;

                if (_concurrentBags[^1].IsEmpty)
                {
                    concurrentBagsLength--;
                }

                _concurrentBags.Add(new ConcurrentBag<int>());

                return GetSnapshotEnumerable(concurrentBagsLength);
            }

            private IEnumerable<int> GetSnapshotEnumerable(int length)
            {
                for (var i = 0; i < length; i++)
                {
                    foreach (var item in _concurrentBags[i])
                    {
                        yield return item;
                    }
                }
            }

            public void Freeze()
            {
                _readerWriterLock.AcquireWriterLock(TimeSpan.FromDays(1));
            }

            public void Unfreeze()
            {
                _readerWriterLock.ReleaseWriterLock();
            }
        }
    }
}