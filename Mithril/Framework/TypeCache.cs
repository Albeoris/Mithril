using System;

namespace Mithril
{
    public static class TypeCache<T>
    {
        public static readonly Type Type = typeof(T);
    }
}