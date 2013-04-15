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

namespace ClusterViewForSliverlinght
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }
       
        ClusterTable clusterTable;





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
                try
                {
                    clusterTable = ClusterTable.Create(openFileDialog1.File.OpenText());
                    ClusterTableView.DataContext = clusterTable;
                }
                catch
                {
                    MessageBox.Show("ファイル読み込みに失敗しました。形式が違うかもしれません");
                }
            }

        }
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "clusterTable Files (.clusterTable)|*.clusterTable|All Files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog()==true)
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
                    ClusterTableView.DataContext = clusterTable;
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
                try
                {
                    clusterTable.AddRelationData(openFileDialog1.File.OpenText());
                    MessageBox.Show("完了");
                }
                catch
                {
                    MessageBox.Show("ファイル読み込みに失敗しました。形式が違うと思われます。");
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
                try
                {
                    clusterTable.AddComunityUserData(openFileDialog1.File.OpenText());
                    MessageBox.Show("完了");
                }
                catch
                {
                    MessageBox.Show("ファイル読み込みに失敗しました。形式が違うと思われます。");
                }
            }
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
                    }
                }
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement tb = sender as FrameworkElement;
            Comunity c = tb.Tag as Comunity;
            if (Mode != MouseMode.選択)
            {
                clusterTable.ViewRelation(c, (int)RelationCountSlider.Value, GetRelationIndexType());
            }
            else
            {
                c.ChangeSelect();
                clusterTable.ViewRelation((int)RelationCountSlider.Value, GetRelationIndexType());

            }

            if (Mode == MouseMode.削除)
            {
                c.ChangeDelete();
            }


            dragItem = c;
            itemDragging = true; 
            mouseOverComunity = c;
            CreateUserAttributeData();
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
            if (clusterTable != null)
            {
                clusterTable.AllUnSeleted();
                clusterTable.ChangeVisibility();
            }
            dragItem = null;
            itemDragging = false;
            mouseOverComunity = null;
        }

        static MouseMode mouseMode = MouseMode.移動;

        public static MouseMode Mode
        {
            get { return MainPage.mouseMode; }
            set { MainPage.mouseMode = value; }
        }
        public enum MouseMode
        {
            移動, 選択, 削除
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
        }
        #endregion

        #region ユーザ属性関係
        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CreateUserAttributeData();
        }

        void CreateUserAttributeData()
        {
            if (comboBox2 != null && dragItem !=null)
            {
                var list = clusterTable.GetUserIdList(dragItem, ((ComboBoxItem)comboBox2.SelectedItem).Content.ToString());
                var g= clusterTable.CreateUserAttributeGroup(list);
                AttributeContentControl.DataContext = g;
                if (g.Count() > 0)
                {
                    UserCount.Text = g.First().Count.ToString();
                }
            }
        }
        #endregion


    }



   
}
