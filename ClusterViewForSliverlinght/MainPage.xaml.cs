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
using ClusterViewForSliverlinght.Models;
using System.Windows.Printing;
using System.Reflection;
using System.Windows.Browser;

namespace ClusterViewForSliverlinght
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            VersionBlock.DataContext = this;
            graphView = new View.Graph(canvas1);
            pd = new PrintDocument();
            pd.PrintPage += new EventHandler<PrintPageEventArgs>(pd_PrintPage);
            graphView.ChangeDataGrid += new EventHandler<MyLib.Event.Args<List<View.Graph.RelationData>>>(graphView_ChangeDataGrid);
            MyLib.Task.Utility.UISyncContext = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();
            ClusteringSettingPanel.DataContext = ClusteringViewModel;

        }
        Analyze.ClusteringViewModel clusteringViewModel = new Analyze.ClusteringViewModel();
        public Analyze.ClusteringViewModel ClusteringViewModel
        {
            get
            {
                if (clusteringViewModel == null) clusteringViewModel = new Analyze.ClusteringViewModel();
                return clusteringViewModel;
            }
        }
        public void ChangeClusterTable()
        {
            ClusteringViewModel.ClusterTable = clusterTable;
            ClusterTableView.DataContext = clusterTable;
        }

        void graphView_ChangeDataGrid(object sender, MyLib.Event.Args<List<View.Graph.RelationData>> e)
        {
            relationViewDataGrid.ItemsSource = e.Value;
        }

        void pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            Viewbox vb = new Viewbox();
            vb.Width = e.PrintableArea.Width;
            vb.Height = e.PrintableArea.Height;
            vb.Child = canvas1;
            e.PageVisual = vb;

        }

        ClusterTable clusterTable;
        View.Graph graphView;
        PrintDocument pd;



        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        #region 列の横移動処理
        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            clusterTable.LeftShift((Category)b.Tag);
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            clusterTable.RightShift((Category)b.Tag);
        }
        #endregion

        #region ファイル関係処理

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "TSV Files (.tsv)|*.tsv|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == true)
            {
                IEnumerable<string> errList = null;
                var stream = openFileDialog1.File.OpenText();
                try
                {
                    clusterTable = ClusterTable.Create(stream, out errList);
                    ClusterTableView.DataContext = clusterTable;
                    ChangeClusterTable();
                    if (errList.Any())
                    {
                        MessageBox.Show("エラーが存在します。\n" + errList.Aggregate((n, m) => n + "\n" + m));
                    }
                }
                catch
                {
                    MessageBox.Show("ファイル読み込みに失敗しました。形式が違うかもしれません。\n" + errList.Aggregate((n, m) => n + "\n" + m));
                }
                finally
                {
                    stream.Close();
                }

            }

        }
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "clusterTable Files (.clusterTable)|*.clusterTable|All Files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    clusterTable.Save(saveFileDialog.OpenFile());
                    MessageBox.Show("完了");
                }
                catch
                {
                    MessageBox.Show("ファイルの書き込みに失敗");
                }
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "clusterTable Files (.clusterTable)|*.clusterTable|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == true)
            {
                try
                {
                    clusterTable = ClusterTable.Load(openFileDialog1.File.OpenRead());
                    ChangeClusterTable();

                    FilePanel.Visibility = System.Windows.Visibility.Collapsed;
                }
                catch
                {
                    MessageBox.Show("ファイル読み込みに失敗しました。形式が違うと思われます。");
                }
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "TSV Files (.tsv)|*.tsv|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == true)
            {
                IEnumerable<string> errList = null;
                try
                {
                    clusterTable.AddRelationData(openFileDialog1.File.OpenText(), out errList);

                    if (errList.Any())
                    {
                        MessageBox.Show("完了しましたが、エラーが存在します。" + errList.Aggregate((n, m) => n + "\n" + m));
                    }
                    else
                    {
                        MessageBox.Show("完了");
                    }
                }
                catch
                {
                    MessageBox.Show("ファイル読み込みに失敗しました。形式が違うと思われます。" + errList.Aggregate((n, m) => n + "\n" + m));
                }
                if (errList.Any())
                {
                    MessageBox.Show("エラーが存在します。" + errList.Aggregate((n, m) => n + "\n" + m));
                }
            }
        }
        private void button6_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "TSV Files (.tsv)|*.tsv|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == true)
            {
                try
                {
                    clusterTable.AddUserData(openFileDialog1.File.OpenText());
                    MessageBox.Show("完了");
                }
                catch
                {
                    MessageBox.Show("ファイル読み込みに失敗しました。形式が違うと思われます。");
                }
            }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "TSV Files (.tsv)|*.tsv|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == true)
            {
                IEnumerable<string> errList = null;
                try
                {
                    string message = "完了";
                    var stream = openFileDialog1.File.OpenText();
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            clusterTable.AddComunityUserData(stream, out errList);
                            if (errList.Any())
                            {
                                message = "完了しましたが、エラーが存在します。 \n" + errList.Aggregate((n, m) => n + "\n" + m);
                            }
                            stream.Close();
                        }).ContinueWith((n) => { MessageBoxShow(message); });
                }
                catch
                {
                    MessageBox.Show("ファイル読み込みに失敗しました。形式が違うと思われます。\n" + errList.Aggregate((n, m) => n + "\n" + m));
                }
            }
        }

        System.Threading.Tasks.TaskScheduler UIthread = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();
        System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
        void MessageBoxShow(string message)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    MessageBox.Show(message);
                }, cancellationToken, System.Threading.Tasks.TaskCreationOptions.None, UIthread);
        }

        #endregion

        bool itemDragging = false;
        Comunity dragItem = null;



        private void ItemsControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (mouseMode == MouseMode.移動)
            {
                if (itemDragging)
                {
                    var control = sender as Border;
                    control.BorderBrush = new SolidColorBrush(Colors.Red);
                    control.BorderThickness = new Thickness(1);
                }
            }
        }

        private void ItemsControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (itemDragging)
            {
                var control = sender as Border;
                control.BorderBrush = new SolidColorBrush(Colors.Black);
                control.BorderThickness = new Thickness(1);
            }
        }

        private void ItemsControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseMode == MouseMode.移動)
            {
                if (mouseOverComunity != dragItem)
                {
                    if (itemDragging)
                    {

                        var control = sender as Border;
                        control.BorderBrush = new SolidColorBrush(Colors.Black);
                        control.BorderThickness = new Thickness(1);
                        var layer = control.Tag as Layer;

                        clusterTable.MoveComunity(dragItem, mouseOverComunity, layer);
                        itemDragging = false;
                        dragItem = null;
                        clusterTable.SearchHub();
                    }
                }
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement tb = sender as FrameworkElement;
            Comunity c = tb.Tag as Comunity;
            List<ItemRelationViewData> list = new List<ItemRelationViewData>();
            if (Mode != MouseMode.複数選択)
            {
                clusterTable.ViewRelation(c, (int)RelationCountSlider.Value, GetRelationIndexType(), out list);
            }
            else
            {
                c.ChangeSelect();
                clusterTable.ViewRelation((int)RelationCountSlider.Value, GetRelationIndexType(), out list);

            }

            if (Mode == MouseMode.削除)
            {
                c.ChangeDelete();
            }


            dragItem = c;
            itemDragging = true;
            mouseOverComunity = c;
            CreateUserAttributeData();
            RelationDataGrid.ItemsSource = list;
        }


        Comunity mouseOverComunity = null;
        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement tb = sender as FrameworkElement;
            Comunity c = tb.Tag as Comunity;
            mouseOverComunity = c;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseOverComunity = null;
        }

        private RelationIndexType GetRelationIndexType()
        {
            if (((ComboBoxItem)comboBox1.SelectedItem).Content.ToString() == "確信度")
            {
                return RelationIndexType.確信度;
            }
            else
            {
                return RelationIndexType.補正確信度;
            }
        }

        private void modeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (modeComboBox == null) return;
            if (((ComboBoxItem)modeComboBox.SelectedItem).Content.ToString() == "移動")
            {
                mouseMode = MouseMode.移動;
            }
            else if (((ComboBoxItem)modeComboBox.SelectedItem).Content.ToString() == "選択")
            {
                mouseMode = MouseMode.選択;
            }
            else if (((ComboBoxItem)modeComboBox.SelectedItem).Content.ToString() == "削除")
            {
                mouseMode = MouseMode.削除;
            }
            else if (((ComboBoxItem)modeComboBox.SelectedItem).Content.ToString() == "複数選択")
            {
                mouseMode = MouseMode.複数選択;
            }
            if (clusterTable != null)
            {
                clusterTable.AllUnSeleted();
                clusterTable.ChangeVisibility();
            }
            dragItem = null;
            itemDragging = false;
            mouseOverComunity = null;
        }

        static MouseMode mouseMode = MouseMode.選択;

        public static MouseMode Mode
        {
            get { return MainPage.mouseMode; }
            set { MainPage.mouseMode = value; }
        }
        public enum MouseMode
        {
            移動, 選択, 削除, 複数選択
        }


        #region 表示ON/OFF
        void ChangeVisibility(FrameworkElement fe)
        {
            if (fe.Visibility == System.Windows.Visibility.Visible)
            {
                fe.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                fe.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ChangeVisibility(FilePanel);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ChangeVisibility(ViewSettingPanel);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            ChangeVisibility(AttributeContentControl);
            RelationContentControl.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void button8_Click(object sender, RoutedEventArgs e)
        {
            //ChangeVisibility(HelpPanel);
            Uri linkUri = new Uri("help.html", UriKind.Relative);
            HtmlPage.Window.Navigate(linkUri, "_new");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ChangeVisibility(RelationContentControl);
            AttributeContentControl.Visibility = System.Windows.Visibility.Collapsed;
        }

        #endregion

        #region ユーザ属性関係
        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CreateUserAttributeData();
        }

        void CreateUserAttributeData()
        {
            if (comboBox2 != null && dragItem != null)
            {
                if (AttributeContentControl.Visibility == System.Windows.Visibility.Visible)
                {
                    var list = clusterTable.GetUserIdList(dragItem, ((ComboBoxItem)comboBox2.SelectedItem).Content.ToString());
                    var g = clusterTable.CreateUserAttributeGroup(list);
                    AttributeContentControl.Visibility = System.Windows.Visibility.Collapsed;
                    AttributeContentControl.DataContext = g;
                    if (g.Count() > 0)
                    {
                        UserCount.Text = g.First().Count.ToString();
                    }
                    AttributeContentControl.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
        #endregion

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            graphView.SetData(clusterTable);
            graphView.ChangeLinkNum((int)RelationCountSlider2.Value);
            relationViewDataGrid.ItemsSource = graphView.GetRelationData();
            graphView.Spring();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            graphView.Spring();
        }

        private void RelationCountSlider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (graphView != null)
            {
                graphView.ChangeLinkNum((int)RelationCountSlider2.Value);
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            RelationCountSlider2.Value = RelationCountSlider2.Value - 1;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            RelationCountSlider2.Value = RelationCountSlider2.Value + 1;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (graphView.AutoRun(RelationCountSlider2, TimeSlider, UIthread))
            {
                autoButton.Content = "停止";
            }
            else
            {
                autoButton.Content = "開始";
            }
        }

        private void button4_Click_1(object sender, RoutedEventArgs e)
        {
            graphView.ChangeLinkTextVisibility();
            graphView.ChangeNodeCountTextVisibility();
        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            pd.Print("関係グラフ");
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            relationViewDataGrid.ItemsSource = graphView.GetRelationData();
        }

        private void minLeftNumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (minLeftNumericUpDown != null && graphView != null)
            {
                graphView.Min信頼度比 = minLeftNumericUpDown.Value;
            }
        }

        public string Version
        {
            get
            {

                string name = Assembly.GetExecutingAssembly().FullName;
                AssemblyName asmName = new AssemblyName(name);

                return asmName.Version.ToString(3);
            }
        }

        private void ClusteringButton_Click(object sender, RoutedEventArgs e)
        {
            Analyze.Clustering clustering = new Analyze.Clustering() { ClusterTable = clusterTable };
            clustering.Run();

        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            ChangeVisibility(ClusteringPanel);
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "TSV Files (.tsv)|*.tsv|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == true)
            {
                IEnumerable<string> errList = null;
                var stream = openFileDialog1.File.OpenText();
                try
                {
                    clusterTable = new ClusterTable();
                    clusterTable.ReadCominityLayerData(stream, (int)ClusterNumSlider.Value, out errList);
                    ClusterTableView.DataContext = clusterTable;
                    ChangeClusterTable();
                    if (errList.Any())
                    {
                        MessageBox.Show("エラーが存在します。\n" + errList.Aggregate((n, m) => n + "\n" + m));
                    }
                }
                catch
                {
                    MessageBox.Show("ファイル読み込みに失敗しました。形式が違うかもしれません。\n" + errList.Aggregate((n, m) => n + "\n" + m));
                }
                finally
                {
                    stream.Close();
                }

            }
        }



        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            button7_Click(sender, e);
        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            Analyze.Clustering clustering = new Analyze.Clustering() { ClusterTable = clusterTable };
            clustering.RandomLayout();
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            Analyze.Clustering clustering = new Analyze.Clustering() { ClusterTable = clusterTable, TryCount = 100 };
            clustering.Run();
        }

        private void Button_Click_14(object sender, RoutedEventArgs e)
        {
            Analyze.Clustering clustering = new Analyze.Clustering() { ClusterTable = clusterTable, TryCount = 1 };
            clustering.Run();
        }

        private void AttributeViewButton_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            var t = b.Tag as UserAttributeData;
            clusterTable.UserAttributeUnSelected();
            clusterTable.ChangeColorByAttribute(t.Parent.GroupName, t.AttributeName);
            t.Selected = true;
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (clusterTable != null)
            {
                clusterTable.AddCategroy();
            }
        }

        private void CategoryDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as FrameworkElement;
            var c = b.Tag as Category;
            clusterTable.RemoveCategory(c);
        }

        private void Button_Click_15(object sender, RoutedEventArgs e)
        {
            Analyze.Clustering2 clustering = new Analyze.Clustering2();
            clustering.ClusterTable = clusterTable;
            clustering.Report = (text) =>
            {
                MyLib.Task.Utility.UITask(() =>
                {
                    ClusteringRepotTextBox.Text += text + "\n";
                }, UIThread);

            };
            System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    clustering.Run();
                }).ContinueWith((n) => clustering.Update());
        }

        System.Threading.Tasks.TaskScheduler UIThread = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();
       

        private void sample1Buntton_Click(object sender, RoutedEventArgs e)
        {
            WebClient webClient = new WebClient(); 
            Uri docUri = HtmlPage.Document.DocumentUri;
           // if (webClient.IsBusy == false)
            {
              //  var u = new Uri(docUri, "Data/hanzawa.zip");
                var u = new Uri("http://web.sfc.keio.ac.jp/~kiichi/ClusterViewForSliverlinght/Data/hanzawa.zip");
                webClient.OpenReadAsync(u);
                webClient.OpenReadCompleted += (o, e1) =>
                {
                    try
                    {
                        using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(e1.Result))
                        using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                        {
                            zip.First().Extract(mStream);
                            clusterTable = ClusterTable.Load(mStream);
                            ChangeClusterTable();

                            FilePanel.Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("ダウンロードに失敗");
                    }
                };
            }
          
        }

        private void sample2Buntton_Click(object sender, RoutedEventArgs e)
        {
            //Uri docUri = HtmlPage.Document.DocumentUri;
            //if (webClient.IsBusy == false)
            //{
            //    webClient.OpenReadAsync(new Uri(docUri, "Data/burasan.zip"));
            //}
         
        }


    }




}
