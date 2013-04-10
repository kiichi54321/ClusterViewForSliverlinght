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

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "TSV Files (.tsv)|*.tsv|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == true)
            {
                clusterTable = ClusterTable.Create(openFileDialog1.File.OpenText());
                ClusterTableView.DataContext = clusterTable;
            }
            
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

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

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "clusterTable Files (.clusterTable)|*.clusterTable|All Files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog()==true)
            {
                clusterTable.Save(saveFileDialog.OpenFile());
                MessageBox.Show("完了");
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
              clusterTable =  ClusterTable.Load(openFileDialog1.File.OpenRead());
              ClusterTableView.DataContext = clusterTable;
              FilePanel.Visibility = System.Windows.Visibility.Collapsed;
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
                clusterTable.AddRelationData(openFileDialog1.File.OpenText());
                MessageBox.Show("完了");
            }
        }


        bool itemDragging = false;
        Comunity dragItem = null;

        private void ItemsControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (itemDragging)
            {
                var control = sender as Border;
                control.BorderBrush = new SolidColorBrush(Colors.Red);
                control.BorderThickness = new Thickness(1);
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

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement tb = sender as FrameworkElement;
            Comunity c = tb.Tag as Comunity;
            if (((ComboBoxItem)comboBox1.SelectedItem).Content.ToString() == "確信度")
            {
                clusterTable.ViewRelation(c, (int)RelationCountSlider.Value, RelationIndexType.確信度);
            }
            else
            {
                clusterTable.ViewRelation(c, (int)RelationCountSlider.Value, RelationIndexType.補正確信度);
            }
            dragItem = c;
            itemDragging = true; 
            mouseOverComunity = c;
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (FilePanel.Visibility == System.Windows.Visibility.Visible)
            {
                FilePanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                FilePanel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ViewSettingPanel.Visibility == System.Windows.Visibility.Visible)
            {
                ViewSettingPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ViewSettingPanel.Visibility = System.Windows.Visibility.Visible;
            }
        }

    }



   
}
