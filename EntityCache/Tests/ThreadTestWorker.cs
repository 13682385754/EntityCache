using System.Collections.Generic;
using System.Diagnostics;
using EntityCache.Cache;
using EntityCache.Entity;

namespace EntityCache.Tests
{
    internal class ThreadTestWorker
    {
        private readonly Cache<Person> _cache;
        private readonly List<int> _ids;

        public ThreadTestWorker(Cache<Person> cache, List<int> ids)
        {
            _cache = cache;
            _ids = ids;
        }

        public void Work()
        {
            const string name = "Thread";
            
            Person p = new Person(_ids[0], name, 0);
            _cache.Update(p);

            _cache.Get(_ids[1]);
            p = new Person(_ids[1], name, 0);
            _cache.Update(p);

            Debug.Assert(_cache.Get(_ids[0]).Name.Equals(name));
            Debug.Assert(_cache.Get(_ids[1]).Name.Equals(name));

            _cache.Remove(_ids[2]);
        }
    }
}