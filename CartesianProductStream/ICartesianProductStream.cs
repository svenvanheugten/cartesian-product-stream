using System;

namespace CartesianProduct
{
    public interface ICartesianProductStream
    {
        void AddX(int x);
        void AddY(int y);
        void Subscribe(Action<(int, int)> action);
    }
}