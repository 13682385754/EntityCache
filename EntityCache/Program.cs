using System;
using EntityCache.Tests;

namespace EntityCache
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            bool isShowNotifications = false;
            CacheOperationsTests.Start(isShowNotifications);
            Console.WriteLine("====================");
            ThreadsTests.Start();
            Console.ReadKey();
        }
    }
}
