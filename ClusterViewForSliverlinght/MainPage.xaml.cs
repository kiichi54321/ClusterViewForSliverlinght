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

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            Comunity c = tb.Tag as Comunity;
            if (((ComboBoxItem)comboBox1.SelectedItem).Content.ToString() == "確信度")
            {
                clusterTable.ViewRelation(c, 10, RelationIndexType.確信度);
            }
            else
            {
                clusterTable.ViewRelation(c, 10, RelationIndexType.補正確信度);
            }
//            c.Brush = new SolidColorBrush(Colors.Orange);
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


    }



   
}
