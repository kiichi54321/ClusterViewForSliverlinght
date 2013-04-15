using System;
using System.IO;
using System.Collections.Generic;

namespace ClusterViewForSliverlinght
{
    public interface IFile
    {
        int GetIndex(string column);
    }

    public class TSVFile:IFile
    {
        StreamReader steram;
        public TSVFile(StreamReader stream)
        {
            this.steram = stream;
        }

        public TSVFile(string fileName)
        {
            this.steram = new StreamReader(fileName);
        }

        Dictionary<string, int> headerDic = new Dictionary<string, int>();

        public int GetIndex(string column)
        {
            if (headerDic.ContainsKey(column))
            {
                return headerDic[column];
            }
            else
            {
                return -1;
            }
        }

        public static IEnumerable<TSVLine> ReadLines(StreamReader stream)
        {
            TSVFile r = new TSVFile(stream);
            return r.Lines;
        }



        public IEnumerable<TSVLine> Lines
        {
            get
            {
                StreamReader sr = steram;
                string first = sr.ReadLine();
                int i =0;
                headerDic.Clear();
                foreach (var item in first.Split('\t'))
                {
                    headerDic.Add(item, i);
                    i++;
                }

                while (sr.Peek()>-1)
                {
                    yield return new TSVLine(sr.ReadLine(), this);
                }
            }
        }
    }

    public class TSVFile<T>:IFile
        where T:TSVLine,new()
    {
        StreamReader steram;
        public TSVFile(StreamReader stream)
        {
            this.steram = stream;
        }

        public TSVFile(string fileName)
        {
            this.steram = new StreamReader(fileName);
        }

        Dictionary<string, int> headerDic = new Dictionary<string, int>();

        public int GetIndex(string column)
        {
            if (headerDic.ContainsKey(column))
            {
                return headerDic[column];
            }
            else
            {
                return -1;
            }
        }

        public static IEnumerable<T> ReadLines(StreamReader stream)
        {
            TSVFile<T> r = new TSVFile<T>(stream);
            return r.Lines;
        }



        public IEnumerable<T> Lines
        {
            get
            {
                StreamReader sr = steram;
                string first = sr.ReadLine();
                int i = 0;
                headerDic.Clear();
                foreach (var item in first.Split('\t'))
                {
                    headerDic.Add(item, i);
                    i++;
                }

                while (sr.Peek() > -1)
                {
                    T t = new T();
                    t.Line = sr.ReadLine();
                    t.SetCsvRead(this);
                    yield return t;
                }
            }
        }
    }

    public class TSVLine
    {
        string line;

        public string Line
        {
            get { return line; }
            set { line = value;
            this.data = line.Split('\t');
            }
        }
        string[] data;
        IFile csvRead;


        public TSVLine(string line,IFile read)
        {
            this.line = line;
            this.csvRead = read;
            this.data = line.Split('\t');
        }
        public TSVLine()
        {
        }

        public void SetCsvRead(IFile file)
        {
            csvRead = file;
        }

        public string GetValue(int index)
        {
            if (index > -1)
            {
                if (data.Length > index)
                {
                    return data[index];
                }
                else
                {
                    throw new Exception("列をはみ出しました");
                }
            }
            throw new Exception("0以上の数値を入れてください");
        }


        public string GetValue(string column)
        {
            var i = csvRead.GetIndex(column);
            if (i > -1)
            {
                if (data.Length > i)
                {
                    return data[i];
                }
                else
                {
                    throw new Exception("列をはみ出しました");
                }
            }
            throw new Exception("存在しない列名です");

        }

        public string GetValue(string column,string defaultValue)
        {
            var i = csvRead.GetIndex(column);
            if (i > -1)
            {
                if (data.Length > i)
                {
                    return data[i];
                }
                else
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
        

        public int GetIntValue(string column)
        {
            return int.Parse(GetValue(column));
        }
        public double GetDoubleValue(string column)
        {
            return double.Parse(GetValue(column));
        }
        public int GetIntValue(string column, int defaultValue)
        {
            int i = defaultValue;
            if (int.TryParse(GetValue(column), out i))
            {
                return i;
            }
            return defaultValue;
        }
        public double GetDoubleValue(string column,double defaultValue)
        {
            double i = defaultValue;
            if (double.TryParse(GetValue(column), out i))
            {
                return i;
            }
            return defaultValue;
        }

        public int GetIntValue(int index)
        {
            return int.Parse(GetValue(index));
        }
        public double GetDoubleValue(int index)
        {
            return double.Parse(GetValue(index));
        }
    }
}
