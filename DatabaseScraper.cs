using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Anagram_Tree.Models;

namespace Words
{
    class DatabaseScraper
    {
        static async Task Search(string[] args)
        {
            #region Setup
            var word = "";
            var alphabet = "абвгдежзийклмнопрстуфхцчшщъьюяАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЮЯ-".ToList();
            Console.WriteLine($"Alpabet letter count: {alphabet.Count}");
            #endregion

            #region Argument sanity check
            if(args.Length < 1)
            {
                Console.WriteLine("Provide a base word to search for: ");
                word = Console.ReadLine();
            }
            else if(args.Length > 1)
            {
                Console.WriteLine("[WARNING] Only first argument will be used!");
                word = args[0];
            }
            else
            {
                word = args[0];
            }
            #endregion

            #region Query generation
            var _applicationContext = new ApplicationContext();
            var data = new List<List<Data>>();
            alphabet = alphabet.Where(c => !word.Contains(c)).ToList();
            Console.WriteLine($"Alphabet: {string.Join("", alphabet)}");
            Console.WriteLine();
            var sum = 0;
            #endregion

            #region Query execution
            data.Add(new List<Data>());
            data.Add(new List<Data>());
            for(int i = 2; i < 20; i++)
            {
                var query = _applicationContext.Word.Where(w => w.Name.Length == i);
                alphabet.ForEach(l => query = query.Where(w => !w.Name.Contains(l)));
                var results = await query
                    .OrderBy(w => w.Id)
                    .GroupBy(w => w.Name)
                    .Select(w => w.FirstOrDefault())
                    .ToListAsync();

                Console.WriteLine($"[results-{i}] {results.Count}");
                foreach (var result in results)
                {
                    if(result.SanityCheck(word))
                    {
                        sum++;
                    }
                }
                sum += results.Count(r => r.SanityCheck(word));
                data.Add(results.Where(r => r.SanityCheck(word)).Select(r => r.ToData()).ToList());
            }

            Console.WriteLine($"TORAL WORD COUNT: {sum}");
            #endregion

            #region Connections statistics
            int connections = 0;
            SetConnection(ref data, ref connections);

            Order(ref data);

            PrintStatistics(ref data, ref connections);

            Console.WriteLine("-------------------------------------------------------------------------------");

            // PrintDataJson(ref data);
            PrintAllWords(ref data);
            // Console.WriteLine("-------------------------------------------------------------------------------");

            // PrintConnectionsJson(ref data, ref connections);
            #endregion
        }

        private static void PrintAllWords(ref List<List<Data>> data)
        {
            for (int level = 2; level < data.Count; level++)
            {
                Console.WriteLine($"------------------------------------------level {level} ------------------------------------------");
                foreach (var d in data[level])
                {
                    Console.WriteLine($"(power: {d.Power}) {d.Value} => ({string.Join(", ", d.LinksForward.Select(n => n.Value))})");
                }
            }
        }

        private static void PrintConnectionsJson(ref List<List<Data>> data, ref int connections)
        {
            var connCount = 0;
            for (int i = 0; i < data.Count; i++)
            {
                var list = data[i];

                for (int j = 0; j < list.Count; j++)
                {
                    var d = list[j];
                    for (int k = 0; k < d.LinksForward.Count; k++)
                    {
                        var next = d.LinksForward[k];
                        var jn = data[i + 1].IndexOf(next);

                        Console.Write($"{{ iFrom: {d.Level - 2}, jFrom: {j}, iTo: {next.Level - 2}, jTo: {jn} }}");
                        connCount++;
                        if(connCount == connections)
                            Console.WriteLine("");
                        else
                            Console.WriteLine(",");
                    }
                }
            }
        }

        private static void PrintDataJson(ref List<List<Data>> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                var list = data[i];

                if(list.Count > 0)
                    Console.WriteLine("[");

                for (int j = 0; j < list.Count; j++)
                {
                    Console.Write($"    {{ value: '{list[j].Power}-{list[j].Value}', xFrom: 0, yFrom: 0, xTo: 0, yTo: 0 }}");
                    if(j == list.Count - 1)
                        Console.WriteLine("");
                    else
                        Console.WriteLine(",");
                }

                if(i != data.Count - 1 && list.Count > 0)
                    Console.WriteLine("],");
                else if(list.Count > 0)
                    Console.WriteLine("]");
            }
        }

        private static void PrintStatistics(ref List<List<Data>> data, ref int connections)
        {
            var maxConn = 0;
            var minConn = int.MaxValue;
            var highestPower = 1;
            var averagePower = 1f;
            var averageConn = 0f;
            var count = data.Count;
            foreach (var list in data)
            {
                foreach (var d in list)
                {
                    averageConn += d.LinksForward.Count;
                    averagePower += d.Power;

                    if(d.LinksForward.Count > maxConn)
                        maxConn = d.LinksForward.Count;

                    if(d.LinksForward.Count < minConn)
                        minConn = d.LinksForward.Count;

                    if(d.Power > highestPower)
                        highestPower = d.Power;
                }
            }
            averageConn /= count;
            averagePower /= count;
            Console.WriteLine($"TOTAL CONNECTIONS COUNT: {connections}");
            Console.WriteLine($"MAX CONNECTIONS: {maxConn}");
            Console.WriteLine($"MIN CONNECTIONS: {minConn}");
            Console.WriteLine($"AVERAGE CONNECTIONS: {averageConn}");
            Console.WriteLine($"HIGHEST POWER: {highestPower}");
            Console.WriteLine($"AVERAGE POWER: {averagePower}");
            bool minConnHasPassed = false;
            foreach (var list in data)
            {
                foreach (var d in list)
                {
                    if(d.LinksForward.Count == maxConn)
                        Console.WriteLine($"Max connections word: {d.Value} => ({string.Join(", ", d.LinksForward.Select(n => n.Value))})");

                    if(d.LinksForward.Count == minConn && !minConnHasPassed)
                    {
                        Console.WriteLine($"Min connections word: {d.Value}");
                        minConnHasPassed = true;
                    }
                }
            }
        }

        private static void Order(ref List<List<Data>> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                data[i] = data[i]
                    .OrderBy(d => d.Power)
                    .ThenBy (d => d.LinksForward.Count)
                    .ThenBy (d => d.LinksBackward.Count)
                    .ToList();
            }
        }

        private static void SetConnection(ref List<List<Data>> data, ref int connections)
        {
            for (int i = 2; i < 20; i++)
            {
                foreach (var d1 in data[i])
                {
                    if(d1.Level == 2)
                        d1.Power = 1;

                    foreach (var d2 in data[i + 1])
                    {
                        // d2 is bigger
                        if(d2.Contains(d1))
                        {
                            d2.LinksBackward.Add(d1);
                            d1.LinksForward.Add(d2);
                            connections++;

                            d2.Power += d1.Power;
                        }
                    }
                }
            }
        }
    }
}
