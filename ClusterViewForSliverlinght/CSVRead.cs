using System;
using System.IO;
using System.Collections.Generic;

namespace ClusterViewForSliverlinght
{
    public interface IFile
    {
        int GetIndex(string column);
        IEnumerable<string> ErrMessageList { get; }
        void AddErrMessage(string message);
    }

    public class TSVFile:IFile,IDisposable
    {
        StreamReader stream;
        public TSVFile(StreamReader stream)
        {
            this.stream = stream;
        }

        public TSVFile(string fileName)
        {
            this.stream = new StreamReader(fileName);
        }

        Dictionary<string, int> headerDic = new Dictionary<string, int>();

        public Dictionary<string, int> HeaderDic
        {
            get { return headerDic; }
            set { headerDic = value; }
        }

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

        //public static IEnumerable<TSVLine> ReadLines(StreamReader stream)
        //{
        //    TSVFile r = new TSVFile(stream);
        //    return r.Lines;
        //}

        public static IEnumerable<TSVLine> ReadLines(StreamReader stream, out IEnumerable<string> errList)
        {
            TSVFile r = new TSVFile(stream);
            var list = r.Lines;
            errList = r.ErrMessageList;
            return list;
        }


        public bool CheckHeader(IEnumerable<string> list)
        {
            bool flag = true;
            foreach (var item in list)
            {
                if (headerDic.ContainsKey(item) == false)
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        List<TSVLine> lines = new List<TSVLine>();
        public IEnumerable<TSVLine> StockLines
        {
            get {return lines; }
        }
        public void Create()
        {
            StreamReader sr = stream;
            string first = sr.ReadLine();
            int i = 0;
            headerDic.Clear();
            foreach (var item in first.Split('\t'))
            {
                headerDic.Add(item, i);
                i++;
            }
            int c = 2;
            lines = new List<TSVLine>();
            while (sr.Peek() > -1)
            {
                lines.Add(new TSVLine(sr.ReadLine(), this, c));
                c++;
            }
        }


        public IEnumerable<TSVLine> Lines
        {
            get
            {
                StreamReader sr = stream;
                string first = sr.ReadLine();
                int i =0;
                headerDic.Clear();
                foreach (var item in first.Split('\t'))
                {
                    headerDic.Add(item, i);
                    i++;
                }
                int c = 2;

                while (sr.Peek()>-1)
                {
                    yield return new TSVLine(sr.ReadLine(), this,c);
                    c++;
                }
             
            }
        }

        #region IDisposable メンバー

        public void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        #endregion

        #region IFile メンバー


        List<string> errMessage = new List<string>();
        public IEnumerable<string> ErrMessageList
        {
            get
            {
                return errMessage;
            }
        }


        public void AddErrMessage(string message)
        {
            errMessage.Add(message);           
        }

        #endregion
    }

    public class TSVFile<T>:IFile,IDisposable
        where T:TSVLine,new()
    {
        StreamReader stream;
        public TSVFile(StreamReader stream)
        {
            this.stream = stream;
        }

        public TSVFile(string fileName)
        {
            this.stream = new StreamReader(fileName);
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

        public static IEnumerable<T> ReadLines(StreamReader stream,out IEnumerable<string> errList)
        {
            TSVFile<T> r = new TSVFile<T>(stream);
            var list = r.Lines;
            errList = r.ErrMessageList;
            return list;
        }



        public IEnumerable<T> Lines
        {
            get
            {
                StreamReader sr = stream;
                string first = sr.ReadLine();
                int i = 0;
                headerDic.Clear();
                foreach (var item in first.Split('\t'))
                {
                    headerDic.Add(item, i);
                    i++;
                }
                int c = 2;
                while (sr.Peek() > -1)
                {
                    T t = new T();
                    t.Count = c;
                    t.Line = sr.ReadLine();
                    t.SetCsvRead(this);
                    c++;
                    yield return t;
                }
                stream.Close();
            }
        }

        #region IDisposable メンバー

        public void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        #endregion


        #region IFile メンバー


        List<string> errMessage = new List<string>();
        public IEnumerable<string> ErrMessageList
        {
            get
            {
                return errMessage;
            }
        }


        public void AddErrMessage(string message)
        {
            errMessage.Add(message);
        }

        #endregion
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

        public int Count { get; set; }

        public TSVLine(string line,IFile read,int count)
        {
            this.line = line;
            this.csvRead = read;
            this.data = line.Split('\t');
            this.Count = count;
        }
        public TSVLine()
        {
        }

        public void SetCsvRead(IFile file)
        {
            csvRead = file;
        }

        private void SendErrMessage(string err)
        {
            this.csvRead.AddErrMessage(Count + "行目 エラー内容："+err+" 「" + this.line + "」" );
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
                    SendErrMessage("列をはみ出しました");
                    throw new Exception("列をはみ出しました");
                }
            }
            SendErrMessage("0以上の数値を入れてください");
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
                    SendErrMessage("「"+column+"」の列が足りません");
                    throw new Exception("列をはみ出しました");
                }
            }
            else
            {
                SendErrMessage("「"+column+"」は存在しない列名です");
                throw new Exception("存在しない列名です");
            }
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
            try
            {
                if (int.TryParse(GetValue(column), out i))
                {
                    return i;
                }
            }
            catch
            {
                SendErrMessage("列名「" + column + "」の取得に失敗しました。規定値を使います。");
            }
            return defaultValue;
        }
        public double GetDoubleValue(string column,double defaultValue)
        {
            double i = defaultValue;
            try
            {
                if (double.TryParse(GetValue(column), out i))
                {
                    return i;
                }
            }
            catch
            {
                SendErrMessage("列名「" + column + "」の取得に失敗しました。規定値を使います。");
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
