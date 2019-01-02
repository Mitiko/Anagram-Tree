using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Anagram_Tree.Models;

namespace Words
{
    public static class DatabaseScraper
    {
        static async Task<List<List<Data>>> Search(string word)
        {
            #region Setup
            var alphabet = "абвгдежзийклмнопрстуфхцчшщъьюяАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЮЯ-".ToList();
            Console.WriteLine($"Alpabet letter count: {alphabet.Count}");
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
                sum += results.Count(r => r.SanityCheck(word));
                data.Add(results.Where(r => r.SanityCheck(word)).Select(r => r.ToData()).ToList());
            }

            Console.WriteLine($"TORAL WORD COUNT: {sum}");
            return data;
            #endregion
        }

        public static string PrintAllWords(ref List<List<Data>> data)
        {
            string result = "";
            for (int level = 2; level < data.Count; level++)
            {
                result += $"------------------------------------------level {level} ------------------------------------------\n";
                foreach (var d in data[level])
                {
                    result += $"(power: {d.Power}) {d.Value} => ({string.Join(", ", d.LinksForward.Select(n => n.Value))})\n";
                }
            }
            return result;
        }

        public static int SetupPrinting(ref List<List<Data>> data)
        {
            int connections = 0;
            SetConnection(ref data, ref connections);
            Order(ref data);
            return connections;
        }

        public static string PrintConnectionsJson(ref List<List<Data>> data, ref int connections)
        {
            string result = "";
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

                        result += $"{{ iFrom: {d.Level - 2}, jFrom: {j}, iTo: {next.Level - 2}, jTo: {jn} }}";
                        connCount++;
                        if(connCount == connections)
                            result += "\n";
                        else
                            result += ",\n";
                    }
                }
            }
            return result;
        }

        public static string PrintDataJson(ref List<List<Data>> data)
        {
            string result = "";
            for (int i = 0; i < data.Count; i++)
            {
                var list = data[i];

                if(list.Count > 0)
                    result += "[\n";

                for (int j = 0; j < list.Count; j++)
                {
                    result += $"    {{ value: '{list[j].Power}-{list[j].Value}', xFrom: 0, yFrom: 0, xTo: 0, yTo: 0 }}";
                    if(j == list.Count - 1)
                        result += "\n";
                    else
                        result += ",\n";
                }

                if(i != data.Count - 1 && list.Count > 0)
                    result += "],\n";
                else if(list.Count > 0)
                    result += "]\n";
            }
            return result;
        }

        public static string PrintStatistics(ref List<List<Data>> data, ref int connections)
        {
            string result = "";
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
            result += $"TOTAL CONNECTIONS COUNT: {connections}\n";
            result += $"MAX CONNECTIONS: {maxConn}\n";
            result += $"MIN CONNECTIONS: {minConn}\n";
            result += $"AVERAGE CONNECTIONS: {averageConn}\n";
            result += $"HIGHEST POWER: {highestPower}\n";
            result += $"AVERAGE POWER: {averagePower}\n";
            bool minConnHasPassed = false;
            foreach (var list in data)
            {
                foreach (var d in list)
                {
                    if(d.LinksForward.Count == maxConn)
                        result += $"Max connections word: {d.Value} => ({string.Join(", ", d.LinksForward.Select(n => n.Value))})\n";

                    if(d.LinksForward.Count == minConn && !minConnHasPassed)
                    {
                        result += $"Min connections word: {d.Value}\n";
                        minConnHasPassed = true;
                    }
                }
            }
            return result;
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
