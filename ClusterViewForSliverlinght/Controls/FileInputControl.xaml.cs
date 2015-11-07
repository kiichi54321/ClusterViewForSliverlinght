using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RawlerLib.MyExtend;
using System.Text;

namespace ClusterViewForSliverlinght.Controls
{
    public enum FileType
    {
        トランザクション,バスケット,自然言語
    }

    public partial class FileInputControl : UserControl,System.ComponentModel.INotifyPropertyChanged
    {
        public FileInputControl()
        {
            InitializeComponent();
        }

        private FileType fileType = FileType.トランザクション;

        string layerText = string.Empty;
        public string LayerText
        {
            get
            {
                return textbox.Text;
            }
            set
            {
                layerText = value;
                textbox.Text = layerText;
                OnPropertyChanged("LayerText");
            }
        }

        public int ClusterCount
        {
            get
            {
                return (int)clusterSlider.Value;
            }
        }

        public Action<ClusterViewForSliverlinght.Models.ClusterTable> Update { get; set; }
        public Action<ClusterViewForSliverlinght.Models.ClusterTable> UpdateClustering { get; set; }
        public Func<List<string>> GetMargeText { get; set; }
        public Func<ClusterViewForSliverlinght.Models.ClusterTable> GetClutsterTable { get; set; }

        System.IO.FileInfo file;
        Dictionary<string, int> countDic = new Dictionary<string, int>();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            if (openFileDialog1.ShowDialog() == true)
            {
                fileType = FileType.トランザクション;
                file = openFileDialog1.File;
                CreateCountDic();
            }
        }

        string[] GetFilters()
        {
            return filterText.Text.Split(' ').Where(n=>n.Length>0).ToArray();
        }

        void CreateCountDic()
        {
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            if (fileType == FileType.トランザクション)
            {
                using (var stream =  file.OpenText())
                {
                    List<string> itemList = new List<string>();
                    string tmpId = string.Empty;
                    var filter = GetFilters();
                    foreach (var item in stream.ReadLines())
                    {
                        var d = item.Split('\t');
                        if (d.Length > 1)
                        {
                            if (tmpId != d[0])
                            {
                                var d2 = d.ToArray(3);
                                if (string.IsNullOrEmpty(d2[2]) == true)
                                {
                                    if (filter.Length == 0 || (filter.Length > 0 && itemList.ContainsAny(filter)))
                                    {
                                        foreach (var item2 in itemList)
                                        {
                                            dic.AddList(item2, tmpId);
                                        }
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

                }
                countDic = dic.ToDictionary(n => n.Key, n => n.Value.Distinct().Count());

                CreateLayerText(countDic);
            }
        }

        Dictionary<string, string> CreateNayoseDic()
        {
            StringBuilder err = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var item in nayoseText.ReadLines())
            {
                var d = item.Split(' ');
                var first = d.Where(n=>n.Length>0).First();
                if (item.First() == '#') { continue; }
                if (item.First() == '-') { first = string.Empty; d[0] = d[0].Replace("-",""); }
                foreach (var item2 in d.Where(n=>n.Length>0))
                {
                    if(dic.ContainsKey(item2)==false)
                    {
                        dic.Add(item2, first);
                    }
                    else
                    {
                        err.AppendLine(item2+"は既にあります。");
                    }
                }
            }

            return dic;
        }

        void CreateLayerText(Dictionary<string,int> dic)
        {
            Dictionary<string, string> lineDic = new Dictionary<string, string>();            

            if( LayerText.ReadLines().Where(n=>n.Length>0).Count()>0)
            {
                var useList = new HashSet<string>( LayerText.ReadLines().Where(n=>n.Contains("--layer")==false).Select(n => n.Split('\t').First()).Where(n => n.Length > 0));
                lineDic = useList.ToDictionary(n => n.Split(',').First(), n => n);
                var d = useList.ToDictionary(n => n.Split(',').First(), n => n.Split(','));

                Dictionary<string, string> tmpDic = CreateNayoseDic();
                dic = dic.GroupBy(n => tmpDic.GetValueOrDefault(n.Key, n.Key)).Where(n => n.Key.IsNullOrEmpty()==false).Select(n => new { n.Key, count = n.Sum(m => m.Value) }).ToDictionary(n => n.Key, n => n.count);
            }
            var rank = dic.OrderByDescending(n => n.Value).Take((int)slider1.Value).ToList();

            var sum = rank.Sum(n => Math.Pow( Math.Log(n.Value+1),2));

            StringBuilder sb = new StringBuilder();

            Dictionary<string, int> layerCount = new Dictionary<string, int>();
            double diff = sum / slider2.Value;
            double current = 0;
            int l = 1;
            string layerName = "layer" + l;
            sb.AppendLine("--"+layerName);
            foreach (var item in rank)
            {
                current = Math.Pow( Math.Log(item.Value + 1),2) + current;
                if (current-diff > 0)
                {
                    l++;
                    layerName = "layer" + l;
                    sb.AppendLine();
                    sb.AppendLine("--"+layerName );
                    current = current - diff;
                }
                layerCount.AddCount(layerName);
                sb.AppendLine(lineDic.GetValueOrDefault( item.Key,item.Key) + "\t" +  item.Value);
            }
            LayerText = sb.ToString();
            LayerCountList.ItemsSource = layerCount;
        }

        Dictionary<string,List<string>> GetLayerDic(string layerText)
        {
            string tmp = string.Empty;
            List<string> list = new List<string>();
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            int count = 1;
            foreach (var item in layerText.ReadLines())
            {
                if (item.Length>1 && item.Substring(0, 2) == "--")
                {
                    list = new List<string>();
                    dic.Add("layer"+count, list);
                    count++;
                }
                else
                {
                    list.Add(item.Split('\t').First());
                }
            }
            return dic;
        }


        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if(PropertyChanged !=null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CreateLayerText(countDic);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CreateFile((n)=>Update(n));
        }

        string nayoseText = string.Empty;
        void CreateFile(Action<Models.ClusterTable> EndAction)
        {
            var ui = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();
            var filter = filterText.Text.Split(' ').Where(n => n.Length > 0).ToArray();
            var clusterCount = ClusterCount;
            var layerText = LayerText;
            nayoseText = nayoseTextBox.Text;
            if (fileType == FileType.トランザクション)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() => {
                    IEnumerable<string> errList;
                   
                    Models.ClusterTable cluster = Models.ClusterTable.CreateDataForTransaction(file.OpenText(), clusterCount, GetLayerDic(layerText), filter, CreateNayoseDic(), out errList);
                    ui.TaskStart(() => {
                        if(EndAction !=null)  EndAction(cluster);
                    });
                });
            }
        }
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            CreateFile(n=> UpdateClustering(n));
        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            LayerText = string.Empty;
            CreateLayerText(countDic);
        }

        private void textbox_TextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            LayerText = LayerText.ReadLines(false).Select(n => n.Split('\t').First()).Lines2String();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(LayerText);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            LayerText = Clipboard.GetText();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            CreateCountDic();
        }

        private void nayoseLoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == true)
            {
                nayoseTextBox.Text = openFileDialog1.File.OpenText().ReadToEnd();
            }

        }

        private void nayoseSaveButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new SaveFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == true)
            {
                try
                {
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(openFileDialog1.OpenFile());
                    sw.Write(nayoseTextBox.Text);
                    MessageBox.Show("完了");
                }
                catch(Exception ex)
                {
                    MessageBox.Show("失敗："+ex.Message);
                }
            }

        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            var nayoseDic = nayoseTextBox.Text.ReadLines().Where(n => n.Length > 0).Select(n=>new Tuple<string,string[]>( n.Split(' ').First(),n.Split(' ').Skip(1).ToArray()));
           
            var clusterTable = GetClutsterTable();
            var dic2 = clusterTable.LayerGroup.SelectMany(n => n.Items).SelectMany(n => n.Comunities).Select(n => new { n, List = n.GetList().ToArray() }).Where(n => n.List.Count() > 0).Select(n => new Tuple<string, string[]>(n.n.Name, n.List));

            StringBuilder sb = new StringBuilder();
            nayoseDic.ToList().Adds(dic2).GroupBy(n => n.Item1).Run(n => sb.AppendLine(n.Key + " " + n.SelectMany(m => m.Item2).Distinct().JoinText(" ")));
            nayoseTextBox.Text = sb.ToString();
        }


    }

    public static class Extend
    {
        public static IEnumerable<string> GetList(this Models.Comunity comunity )
        {
            foreach (var item in comunity.Children)
            {
                yield return item.Name;
                foreach (var item2 in item.GetList())
                {
                    yield return item2;
                }
            }
            
        }

        public static void Run<T>(this IEnumerable<T> list ,Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
        }

    }
}
