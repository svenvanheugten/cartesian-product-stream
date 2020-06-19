using System;
using System.Collections.Concurrent;

namespace CartesianProduct
{
    public class UnsafeCartesianProductStream : ICartesianProductStream
    {
        private readonly ConcurrentBag<int> _xs;
        private readonly ConcurrentBag<int> _ys;
        private Action<(int, int)> _callback;

        public UnsafeCartesianProductStream()
        {
            _xs = new ConcurrentBag<int>();
            _ys = new ConcurrentBag<int>();
        }

        public void AddX(int x)
        {
            _xs.Add(x);

            foreach (var y in _ys)
            {
                _callback((x, y));
            }
        }

        public void AddY(int y)
        {
            _ys.Add(y);

            foreach (var x in _xs)
            {
                _callback((x, y));
            }
        }

        public void Subscribe(Action<(int, int)> action)
        {
            _callback = action;
        }
    }
}