using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Anagram_Tree.Models;

namespace Anagram_Tree
{
    public static class DatabaseScraper
    {
        public static async Task<(List<List<Data>> datas, string stats)> Search(string word, string connectionString)
        {
            #region Setup
            var alphabet = "абвгдежзийклмнопрстуфхцчшщъьюяАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЮЯ-".ToList();
            string stats = $"Alpabet letter count: {alphabet.Count}<br>";
            #endregion

            #region Query generation
            var builder = new DbContextOptionsBuilder<ApplicationContext>();
            builder.UseNpgsql(connectionString);
            var _applicationContext = new ApplicationContext(builder.Options);
            var data = new List<List<Data>>();
            alphabet = alphabet.Where(c => !word.Contains(c)).ToList();
            stats += $"Alphabet: {string.Join("", alphabet)}<br><br>";
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

                stats += $"[results-{i}] {results.Count}<br>";
                sum += results.Count(r => r.SanityCheck(word));
                data.Add(results.Where(r => r.SanityCheck(word)).Select(r => r.ToData()).ToList());
            }

            stats += $"TORAL WORD COUNT: {sum}<br>";
            return (data, stats);
            #endregion
        }

        public static string PrintAllWords(List<List<Data>> data)
        {
            string result = "";
            for (int level = 2; level < data.Count; level++)
            {
                result += $"------------------------------------------level {level} ------------------------------------------<br>";
                foreach (var d in data[level])
                {
                    result += $"(power: {d.Power}) {d.Value} => ({string.Join(", ", d.LinksForward.Select(n => n.Value))})<br>";
                }
            }
            return result;
        }

        public static int Setup(ref List<List<Data>> data)
        {
            int connections = 0;
            SetConnection(ref data, ref connections);
            Order(ref data);
            return connections;
        }

        public static string PrintConnectionsJson(List<List<Data>> data, int connections)
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
                            result += "";
                        else
                            result += ",";
                    }
                }
            }
            return result;
        }

        public static string PrintDataJson(List<List<Data>> data)
        {
            string result = "";
            for (int i = 0; i < data.Count; i++)
            {
                var list = data[i];

                if(list.Count > 0)
                    result += "[";

                for (int j = 0; j < list.Count; j++)
                {
                    result += $"    {{ value: '{list[j].Power}-{list[j].Value}', xFrom: 0, yFrom: 0, xTo: 0, yTo: 0 }}";
                    if(j == list.Count - 1)
                        result += "";
                    else
                        result += ",";
                }

                if(i != data.Count - 1 && list.Count > 0)
                    result += "],";
                else if(list.Count > 0)
                    result += "]";
            }
            return result;
        }

        public static string PrintStatistics(List<List<Data>> data, int connections)
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
            result += $"TOTAL CONNECTIONS COUNT: {connections}<br>";
            result += $"MAX CONNECTIONS: {maxConn}<br>";
            result += $"MIN CONNECTIONS: {minConn}<br>";
            result += $"AVERAGE CONNECTIONS: {averageConn}<br>";
            result += $"HIGHEST POWER: {highestPower}<br>";
            result += $"AVERAGE POWER: {averagePower}<br>";
            bool minConnHasPassed = false;
            foreach (var list in data)
            {
                foreach (var d in list)
                {
                    if(d.LinksForward.Count == maxConn)
                        result += $"Max connections word: {d.Value} => ({string.Join(", ", d.LinksForward.Select(n => n.Value))})<br>";

                    if(d.LinksForward.Count == minConn && !minConnHasPassed)
                    {
                        result += $"Min connections word: {d.Value}<br>";
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
                    .OrderBy(d => d.LinksForward.Count)
                    .ThenBy (d => d.LinksBackward.Count)
                    .ThenBy (d => d.Power)
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
