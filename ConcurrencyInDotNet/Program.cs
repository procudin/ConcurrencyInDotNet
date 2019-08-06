using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConcurrencyInDotNetFSharp;

namespace ConcurrencyInDotNet
{
    class Program
    {
        static DictionaryAgent<int, string> agent = new DictionaryAgent<int, string>();

        static async Task Main(string[] args)
        {
            await Task.Run(() =>
            {
                Enumerable.Repeat(0, 10000).ToList().AsParallel().WithDegreeOfParallelism(3).Select((v, i) => i % 10000).ForAll(v => agent.AddIfNotExists(v, v.ToString()));

                agent.AddIfNotExists(10, 10.ToString());
                agent.AddIfNotExists(20, 10.ToString());
                Thread.Sleep(5000);
            });
        }
    }


    public static class EXT
        {
        public static void ForEach<T>(this IEnumerable<T> col, Action<T> act)
        {
            foreach (var t in col)
                act(t);
        }
            

        }
}
