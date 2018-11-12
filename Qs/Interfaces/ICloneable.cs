using System;

namespace Qs.Interfaces
{
    public interface ICloneable<out T> : ICloneable
    {
        new T Clone();
    }
}
