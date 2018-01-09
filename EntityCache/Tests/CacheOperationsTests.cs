using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EntityCache.Cache;
using EntityCache.Entity;
using EntityCache.EntityTranslator;
using EntityCache.Repository;

namespace EntityCache.Tests
{
    class CacheOperationsTests
    {
        private static readonly Random Random = new Random(1);

        public static void Start(bool isPrintNotifications)
        {
            PersonEntityTranslator personEntityTranslator = new PersonEntityTranslator();
            FileReadDataProvider fileReadDataProvider = new FileReadDataProvider();
            Cache<Person> cache = new Cache<Person>(fileReadDataProvider, personEntityTranslator);
            cache.Init();
            if (isPrintNotifications)
            {
                cache.Notify += (sender, args) => Console.WriteLine($"op: {args.Operation} id: {args.EntityId}");
            }

            Console.WriteLine("Test eager init cache");
            TestSingleOps(cache);
            Console.WriteLine("====================");
            TestManyOps(cache);

            // test lazy init
            Console.WriteLine("====================");
            Console.WriteLine("Test lazy init cache");
            fileReadDataProvider.InsertMockDataIntoRepo();
            Cache<Person> cacheLazy = new Cache<Person>(fileReadDataProvider, personEntityTranslator, Cache<Person>.InitType.Lazy);
            TestLazySingleOps(cacheLazy);
        }

        private static void TestSingleOps(Cache<Person> cache)
        {
            // assume cache is empty in the beginning 
            Console.Write("Get fail ");
            Person p = cache.Get(0);
            Debug.Assert(p == null);
            WriteSuccess();

            Console.Write("Update fail ");
            p = new Person(0, "bob", 111);
            try
            {
                cache.Update(p);
            }
            catch (ArgumentException e)
            {
                Debug.Assert(e.Message.Equals("Entity doesn't exist, add it first"));
                WriteSuccess();
            }

            Console.Write("Add fail ");
            cache.Add(p);
            try
            {
                cache.Add(p);
            }
            catch (ArgumentException e)
            {
                Debug.Assert(e.Message.Equals("Entity already exists, use update"));
                WriteSuccess();
            }

            Console.Write("Remove fail ");
            try
            {
                cache.Remove(9);
            }
            catch (ArgumentException e)
            {
                Debug.Assert(e.Message.Equals("Failed to remove entity, entity doesn't exist"));
                WriteSuccess();
            }

            // Add success
            Console.Write("Add ");
            p = new Person(1, "bab", 333);
            cache.Add(p);
            Person personFromCache = cache.Get(1);
            Debug.Assert(p.Equals(personFromCache));
            WriteSuccess();

            // update success
            Console.Write("Update ");
            p = new Person(1, "bab", 444);
            cache.Update(p);
            personFromCache = cache.Get(1);
            Debug.Assert(p.Age == personFromCache.Age);
            WriteSuccess();

            // remove success
            Console.Write("Remove ");
            cache.Remove(1);
            Debug.Assert(cache.Get(1) == null);
            WriteSuccess();

            // get implicitly succeeds from all the above 

            cache.Remove(0);
        }

        private static void TestManyOps(Cache<Person> cache)
        {
            // assumes caches is empty
            // add many entities to the cache, then update and remove some of them
            const int entitiesAmount = 10;
            Console.Write($"Running tests on {entitiesAmount} entities ");

            List<Person> entityList = new List<Person>();
            char a = 'a';
            for (int i = 0; i < entitiesAmount; i++)
            {
                entityList.Add(new Person(i, new string((char)(a + i), 3), i + 10));
            }

            foreach (var person in entityList)
            {
                cache.Add(person);
            }

            // check add           
            foreach (var person in entityList.AsEnumerable().Reverse())
            {
                Person personFromCache = cache.Get(person.Id);
                Debug.Assert(personFromCache.Equals(person));                             
            }

            // check update
            const int updatesAmount = 3;
            for (int i = 0; i < updatesAmount; i++)
            {
                Person randomPerson = entityList[Random.Next(0, entitiesAmount)];
                randomPerson = new Person(randomPerson.Id, randomPerson.Name, randomPerson.Age + 10);
                cache.Update(randomPerson);
                Debug.Assert(randomPerson.Equals(cache.Get(randomPerson.Id)));
            }

            // check remove 
            const int removeAmount = 5;
            for (int i = 0; i < removeAmount; i++)
            {
                int idToRemove = Random.Next(0, entitiesAmount);
                Person beforeRemove = cache.Get(idToRemove);
                if (beforeRemove == null)
                {
                    i--;    // this id was already removed, so try again
                    continue;
                }
                cache.Remove(idToRemove);
                Person afterRemove = cache.Get(idToRemove);
                Debug.Assert(!beforeRemove.Equals(afterRemove) && afterRemove == null);
            }

            WriteSuccess();
        }

        private static void TestLazySingleOps(Cache<Person> cache)
        {
            // assume cache is empty in the beginning 
            Console.Write("Get fail ");
            Person p = cache.Get(99);
            Debug.Assert(p == null);
            WriteSuccess();

            Console.Write("Get success ");
            p = new Person(0, "aaa", 10);
            Debug.Assert(p.Equals(cache.Get(0)));
            WriteSuccess();

            Console.Write("Update fail ");
            Person p2 = new Person(10, "bob", 111);
            try
            {
                cache.Update(p2);
            }
            catch (ArgumentException e)
            {
                Debug.Assert(e.Message.Equals("Entity doesn't exist, add it first"));
                WriteSuccess();
            }

            Console.Write("Add fail ");
            try
            {
                cache.Add(p);
            }
            catch (ArgumentException e)
            {
                Debug.Assert(e.Message.Equals("Entity already exists, use update"));
                WriteSuccess();
            }

            Console.Write("Remove fail ");
            try
            {
                cache.Remove(90);
            }
            catch (ArgumentException e)
            {
                Debug.Assert(e.Message.Equals("Failed to remove entity, entity doesn't exist"));
                WriteSuccess();
            }

            // remove success
            Console.Write("Remove ");
            cache.Remove(1);
            Debug.Assert(cache.Get(1) == null);
            WriteSuccess();

            cache.Get(3);
            cache.Get(4);

            // cache has 3 entities at this point (can't assert it from here, saw in debugger)
        }

        private static void WriteSuccess()
        {
            Console.WriteLine("success");
        }

    }
}
