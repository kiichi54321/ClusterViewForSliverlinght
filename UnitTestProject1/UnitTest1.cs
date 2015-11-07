using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using RawlerLib.MyExtend;

namespace UnitTestProject1
{
    public static class TestExtend
    {
        public static IEnumerable<T> ConsoleWriteLine<T>(this IEnumerable<T>  list, Func<T,string> func)
        {
            foreach (var item in list)
            {
                Console.WriteLine(func(item));                
            }
            return list;
        }

        public static IEnumerable<string> ConsoleWriteLine(this IEnumerable<string> list)
        {
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
            return list;
        }
    }

    [TestClass]
    public class UnitTest1
    {
        string file = @"C:\Users\kiichi\Downloads\cookpad_data\cookpad_data\サンプルデータ.txt";
        [TestMethod]
        public void TestMethod1()
        {
    //        string[] filter={ };
            string[] filter = { "マヨネーズ" };
            List<string> itemList = new List<string>();
            string tmpId = string.Empty;
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            foreach (var item in System.IO.File.ReadLines(file))
            {
                var d = item.Split('\t');
                if (d.Length > 0)
                {
                    if (tmpId != d[0])
                    {
                        if (filter.Length == 0 || (filter.Length > 0 && itemList.ContainsAny(filter)))
                        {
                            foreach (var item2 in itemList)
                            {
                                dic.AddList(item2, tmpId);
                            }
                        }
                        itemList.Clear();
                        tmpId = d[0];
                    }
                    itemList.Add(d[1]);
                }
            }
            if (filter.Length == 0 || (filter.Length > 0 && itemList.ContainsAny(filter)))
            {
                foreach (var item2 in itemList)
                {
                    dic.AddList(item2, tmpId);
                }
                itemList.Clear();
            }

            var dic2 = dic.ToDictionary(n => n.Key, n => n.Value.Count);
            dic2.OrderByDescending(n => n.Value).Take(100).ConsoleWriteLine(n => n.Key + "\t" + n.Value);
        }
    }

}
