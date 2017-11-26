using System;

namespace Pulse.Core
{
    public static class TypeCache<T>
    {
        public static readonly Type Type = typeof(T);
    }
}