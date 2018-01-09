using System;
using System.Collections.Generic;
using System.Threading;
using EntityCache.Entity;
using EntityCache.EntityTranslator;
using EntityCache.Repository;

namespace EntityCache.Cache
{
    internal class Cache <T> where T : class, IEntity
    {
        public enum InitType
        {
            Eager,
            Lazy
        }

        public event EventHandler<CacheEventArgs> Notify;

        private bool _isInitialized;
        private readonly InitType _initType;
        private readonly IRepositoryProvider _repositoryProvider;
        private readonly IEntityTranslator<T> _entityTranslator;
        private readonly Dictionary<int, T> _entityCache;
        private readonly ReaderWriterLockSlim _sync; // I chose this over a simple lock because the popular use case for this
                                                     // class is Get() so using this will result in even better retrieval times
        
        public Cache(IRepositoryProvider fileReadDataProvider, IEntityTranslator<T> personEntityTranslator, InitType initType = InitType.Eager)
        {
            _initType = initType;
            _repositoryProvider = fileReadDataProvider;
            _entityTranslator = personEntityTranslator;
            _entityCache = new Dictionary<int, T>();
            _sync = new ReaderWriterLockSlim();
        }
        
        public void Init()
        {
            _sync.EnterWriteLock();
            try
            {
                _Init();
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        private void _Init()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            if (_initType == InitType.Eager)
            {
                List<Dictionary<string, string>> entities = _repositoryProvider.GetAllEntries();
                foreach (var item in entities)
                {
                    T entity = _entityTranslator.ParseEntity(item); 
                    _entityCache.Add(entity.Id, entity);
                }
            }
            
        }

        // returns null if id doesn't exist
        public T Get(int id)
        {
            _sync.EnterReadLock();
            try
            {
                return _Get(id);
            }
            finally
            {
                _sync.ExitReadLock();
            }
        }

        private T _Get(int id)
        {
            CheckInitialization();
            if (_entityCache.ContainsKey(id))
            {
                return _entityCache[id];
            }

            // it won't exist only if it was set to lazy init, otherwise cache should update with any operation
            if (_initType == InitType.Lazy)
            {
                // try to get that entry
                Dictionary<string, string> entry = _repositoryProvider.Get(id);
                if (entry != null)
                {
                    // add that entity to the cache 
                    T entity = (T)_entityTranslator.ParseEntity(entry);
                    _entityCache.Add(entity.Id, entity);
                    return entity;
                }
            }

            return null;
        }

        public void Remove(int id)
        {
            _sync.EnterWriteLock();
            try
            {
                _Remove(id);
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        private void _Remove(int id)
        {
            CheckInitialization();
           
            if (!CheckEntityExist(id))
            {               
                throw new ArgumentException("Failed to remove entity, entity doesn't exist");
            }

            // try to remove first from the repo
            if (_repositoryProvider.Remove(id))
            {
                _entityCache.Remove(id);
                OnRemove(id); // call only when successful
            }
            else
            {
                throw new ArgumentException("Failed to remove entity");
            }
        }

        public void Update(T entity)
        {
            _sync.EnterWriteLock();
            try
            {
                _Update(entity);       
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        private void _Update(T entity)
        {
            CheckInitialization();
            if (!CheckEntityExist(entity))
            {
                throw new ArgumentException("Entity doesn't exist, add it first");
            }

            // we get here only if the entry exists 
            var entry = _entityTranslator.MapEntity(entity);
            if (_repositoryProvider.Update(entry))
            {
                // replace only if successfully updated repo
                _entityCache[entity.Id] = entity;
                OnUpdate(entity.Id);
            }
        }

        public void Add(T entity)
        {
            _sync.EnterWriteLock();
            try
            {
                _Add(entity);
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        private void _Add(T entity)
        {
            CheckInitialization();

            if (CheckEntityExist(entity))
            {
                throw new ArgumentException("Entity already exists, use update");
            }

            var entry = _entityTranslator.MapEntity(entity);
            if (_repositoryProvider.Add(entry))
            {
                _entityCache.Add(entity.Id, entity);
                OnAdd(entity.Id);
            }
        }

        private bool CheckEntityExist(T entity)
        {
            return CheckEntityExist(entity.Id);
        }

        private bool CheckEntityExist(int id)
        {
            bool entityExistsInCache = _entityCache.ContainsKey(id);
            return entityExistsInCache ||
                   _initType != InitType.Eager && _repositoryProvider.Get(id) != null;
        }


        private void OnAdd(int id)
        {
            Notify?.Invoke(this, new CacheEventArgs(CacheEventArgs.CacheOperation.Add, id));
        }

        private void OnUpdate(int id)
        {
            Notify?.Invoke(this, new CacheEventArgs(CacheEventArgs.CacheOperation.Update, id));
        }

        private void OnRemove(int id)
        {
            Notify?.Invoke(this, new CacheEventArgs(CacheEventArgs.CacheOperation.Remove, id));
        }

        private void CheckInitialization()
        {
            if(!_isInitialized && _initType == InitType.Eager)
            {
                throw new CacheInitializationException();
            }
        }
    }
}
