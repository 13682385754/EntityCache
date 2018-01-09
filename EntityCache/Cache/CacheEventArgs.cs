using System;

namespace EntityCache.Cache
{
    public class CacheEventArgs : EventArgs
    {
        public enum CacheOperation
        {
            Add,
            Remove,
            Update
        }

        public int EntityId { get; }
        public CacheOperation Operation{ get; }

        public CacheEventArgs(CacheOperation cacheOperation, int entityId)
        {
            EntityId = entityId;
            Operation = cacheOperation;
        }
    }
}