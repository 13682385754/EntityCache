using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EntityCache.Entity;
using EntityCache.EntityTranslator;
using EntityCache.Repository;
using EntityCache.Cache;

namespace EntityCache.Tests
{
    public class ThreadsTests
    {
        public static void Start()
        {
            Console.Write("Starting threads tests ");
            PersonEntityTranslator personEntityTranslator = new PersonEntityTranslator();
            FileReadDataProvider fileReadDataProvider = new FileReadDataProvider();
            fileReadDataProvider.InsertMockDataIntoRepo();
            Cache<Person> cache = new Cache<Person>(fileReadDataProvider, personEntityTranslator, Cache<Person>.InitType.Eager);
            cache.Init();
            TestThreads(cache);

            Debug.Assert(fileReadDataProvider.GetFileContents().Trim().Equals(ThreadsOpsResult));
            Console.WriteLine("success");
        }

        private const string ThreadsOpsResult =
@"Name,Thread,Age,0
Name,Thread,Age,0
Name,Thread,Age,0
Name,Thread,Age,0
Name,Thread,Age,0
Name,Thread,Age,0";

        private static void TestThreads(Cache<Person> cache)
        {
            var lists = new List<List<int>>
            {
                new List<int> {0, 1, 9},
                new List<int> {2, 1, 8},
                new List<int> {3, 2, 7},
                new List<int> {5, 4, 6},
            };

            var workerList = lists.Select(list => new ThreadTestWorker(cache, list)).ToList();
            var threads = workerList.Select(threadTestWorker => new Thread(threadTestWorker.Work)).ToList();

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
    }
}