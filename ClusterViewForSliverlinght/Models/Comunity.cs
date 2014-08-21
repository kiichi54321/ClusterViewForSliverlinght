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
           // brush = new SolidColorBrush(Colors.White);
            color = Colors.White;
            opacity = 1;      
        }

        public Comunity Clone()
        {
            Comunity c = new Comunity()
            {
                Deleted = this.Deleted,
                Id = this.Id,
                UserIds = this.UserIds,
                Relations = this.Relations,
                Tag = this.Tag,
                Index = this.Index,
                Name = this.Name,
                ImageUrl = this.ImageUrl,
                IsHub = this.IsHub
            };
            return c;
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
        private bool isHub = false;
        [DataMember]
        public bool IsHub { get { return isHub; } set { isHub = value; OnPropertyChanged("IsHubText"); } }
        private bool isLock = false;
        [DataMember]
        public bool Lock { get { return isLock; } set { isLock = value; OnPropertyChanged("Lock"); } }

        public string IsHubText
        {
            get { if (IsHub) return "●"; else return ""; }
        }

        public int Count
        {
            get
            {
                if (userIds != null)
                {
                    return userIds.Count;
                }
                else
                {
                    return 0;
                }
            }
        }


        bool deleted = false;
        [DataMember]
        public bool Deleted
        {
            get { return deleted; }
            set { deleted = value; }
        }

        private List<int> userIds = new List<int>();
        [DataMember]
        public List<int> UserIds
        {
            get
            {
                if (userIds == null)
                {
                    userIds = new List<int>();
                }
                return userIds;
            }
            set { userIds = value; }
        }
        public Visibility ImageVisibility
        {
            get
            {
                if (string.IsNullOrEmpty(this.ImageUrl))
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public Layer Layer { get; set; }
        public Category Category { get; set; }

        private List<ItemRelation> relations = new List<ItemRelation>();
        [DataMember]
        public List<ItemRelation> Relations
        {
            get { return relations; }
            set { relations = value; }
        }

        public void AddRelations(TSVLine line)
        {
            relations.Add(new ItemRelation() { ItemId = line.GetIntValue("ITEM2", -1), 確信度 = line.GetDoubleValue("確信度", -1), 補正確信度 = line.GetDoubleValue("補正確信度", -1) });
        }

        public void AddRelations(int itemId, double 確信度, double 補正確信度)
        {
            relations.Add(new ItemRelation() { ItemId = itemId, 確信度 = 確信度, 補正確信度 = 補正確信度 });
        }

        public void AttributeView(string attribute)
        {
            foreach (var item in userIds)
            {

            }
        }

        private bool selected = false;

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected != value)
                {
                    ChangeSelect();
                }
            }
        }

        public void ChangeSelect()
        {
            selected = !selected;
            if (selected)
            {
                this.NewBrush(Colors.Red, 0.8);
            }
            else
            {
                this.NewBrush(Colors.Transparent, 1);
            }
        }


        public Visibility Visibility
        {
            get
            {
                if (MainPage.Mode != MainPage.MouseMode.削除)
                {
                    if (deleted)
                    {

                        return System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        return System.Windows.Visibility.Visible;
                    }
                }
                return System.Windows.Visibility.Visible;
            }
        }

        public void ChangeDelete()
        {
            deleted = !deleted;
            OnPropertyChanged("ForeBrush");
        }
        public void ChaqngeVisibility()
        {
            OnPropertyChanged("Visibility");
        }

        Brush brush ;

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

        public Brush ForeBrush
        {
            get
            {
                if (deleted)
                {
                    return new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    return new SolidColorBrush(Colors.Black);
                }
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
                    brush = new SolidColorBrush(Colors.Transparent);
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
        public bool NotUse { get; set; }

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

    public class ItemRelationViewData
    {
        public int Rank { get; set; }
        public string Name { get; set; }
        public string 確信度 { get; set; }
        public string 補正確信度 { get; set; }
    }

    public enum RelationIndexType
    {
        確信度, 補正確信度
    }
}
