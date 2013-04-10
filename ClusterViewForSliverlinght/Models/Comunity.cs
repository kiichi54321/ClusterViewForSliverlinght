using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Collections.Generic;

namespace ClusterViewForSliverlinght.Models
{
    [DataContract]
    public class Comunity : INotifyPropertyChanged
    {
        public Comunity()
        {
            Init();
        }

        public void Init()
        {
            brush = new SolidColorBrush(Colors.White);
            color = Colors.White;
            opacity = 1;
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string ImageUrl { get; set; }
        [DataMember]
        public string Tag { get; set; }
        [DataMember]
        public double Index { get; set; }
        [DataMember]
        public int Count { get; set; }


        private List<ItemRelation> relations = new List<ItemRelation>();
        [DataMember]
        public List<ItemRelation> Relations
        {
            get { return relations; }
            set { relations = value; }
        }

        public void AddRelations(TSVLine line)
        {
            relations.Add(new ItemRelation() { ItemId = line.GetIntValue("ITEM2"), 確信度 = line.GetDoubleValue("確信度"), 補正確信度 = line.GetDoubleValue("補正確信度") });
        }

        Brush brush = new SolidColorBrush(Colors.White);

        Color color = Colors.White;

        public Color Color
        {
            get
            {
                if (color == null) color = Colors.White;
                return color;
            }
            set
            {
                color = value;
            }
        }

        double opacity = 1;
        public double Opacity
        {
            get { return opacity; }
            set
            {
                if (opacity != value)
                {
                    opacity = value;
                    OnPropertyChanged("Brush");
                }
            }
        }

        public void NewBrush(Color color, double opacity)
        {
            brush = new SolidColorBrush(color);
            this.opacity = opacity;
            brush.Opacity = opacity;
            OnPropertyChanged("Brush");
        }

        public Brush Brush
        {
            get
            {
                if (brush == null)
                {
                    brush = new SolidColorBrush(Colors.White);
                }
                return brush;
            }
            set
            {
                if (brush != value)
                {
                    brush = value;
                    OnPropertyChanged("Brush");
                }
            }
        }

        #region INotifyPropertyChanged メンバー

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

    [DataContract]
    public class ItemRelation
    {
        [DataMember]
        public int ItemId { get; set; }
        [DataMember]
        public double 確信度 { get; set; }
        [DataMember]
        public double 補正確信度 { get; set; }
        public double GetIndex(RelationIndexType type)
        {
            if (type == RelationIndexType.確信度)
            {
                return this.確信度;
            }
            if (type == RelationIndexType.補正確信度)
            {
                return this.補正確信度;
            }
            return 0;
        }
    }

    public enum RelationIndexType
    {
        確信度, 補正確信度
    }
}
