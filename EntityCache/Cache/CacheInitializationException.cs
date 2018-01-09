using System;

namespace EntityCache.Cache
{
    internal class CacheInitializationException : Exception
    {
        public CacheInitializationException() : base("Cache was set to Eager but Init() wasn't called.")
        {
        }
    }
}