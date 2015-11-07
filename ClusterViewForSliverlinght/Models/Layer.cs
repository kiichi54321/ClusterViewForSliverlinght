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
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace ClusterViewForSliverlinght.Models
{
    [DataContract]
    public class Layer : System.ComponentModel.INotifyPropertyChanged
    {
        public Layer()
        {
            comunities = new System.Collections.ObjectModel.ObservableCollection<Comunity>();
            Init();
        }

        public event EventHandler ComunitiesChange;

        public void Init()
        {
            comunities.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(comunities_CollectionChanged);
        }

        void comunities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ComunitiesChange != null)
            {
                ComunitiesChange(sender, e);
            }
        }
        //    [DataMember]
        public Category Category { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public System.Collections.ObjectModel.ObservableCollection<Comunity> Comunities
        {
            get
            {
                if (comunities == null)
                {
                    comunities = new System.Collections.ObjectModel.ObservableCollection<Comunity>();
                    comunities.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(comunities_CollectionChanged);                    
                }
                return comunities;
            }
            set
            {
                comunities = value;
            }
        }

        System.Collections.ObjectModel.ObservableCollection<Comunity> comunities = new System.Collections.ObjectModel.ObservableCollection<Comunity>();

        private List<Comunity> tmpComunities = new List<Comunity>();

        public List<Comunity> TmpComunities
        {
            get
            {
                if (tmpComunities == null) tmpComunities = new List<Comunity>();
                return tmpComunities;
            }
            set { tmpComunities = value; }
        }

        private List<Comunity> tmp2Comunities = new List<Comunity>();

        public List<Comunity> Tmp2Comunities
        {
            get
            {
                if (tmp2Comunities == null) tmp2Comunities = new List<Comunity>();
                return tmp2Comunities;
            }
            set { tmp2Comunities = value; }
        }

        public void UpDataTmp()
        {
            if (tmp2Comunities == null) tmp2Comunities = new List<Comunity>();
            tmpComunities.Clear();

            foreach (var item in tmp2Comunities)
            {
                tmpComunities.Add(item);
                item.Layer = this;
            }
            tmp2Comunities.Clear();
        }


        public void Update()
        {
            foreach (var item in comunities)
            {
                item.Layer = this;
            }
            if (tmpComunities == null) tmpComunities = new List<Comunity>();
            tmpComunities.Clear();
            foreach (var item in Comunities)
            {
                item.Layer = this;
                tmpComunities.Add(item);
            }
        }

        public void UpDataView()
        {
            comunities.Clear();
            foreach (var item in tmpComunities)
            {
                item.Layer = this;
                comunities.Add(item);
            }
        }

        Brush brush;
        public void NewBrush(Color color, double opacity)
        {
            brush = new SolidColorBrush(color);

            brush.Opacity = 0.5;
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

        Color bgColor = Colors.White;
        public Color BGColor
        {
            get
            {
                return bgColor;
            }
            set
            {
                if (bgColor != value)
                {
                    bgColor = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("BGColor"));
                    }

                }

            }
        }

        public void ChangeAttributeColor(Models.Users users, string AttributeName, string value, double baseRate)
        {
            List<int> list = new List<int>();
            foreach (var item in comunities.Where(n => n.UserIds != null))
            {
                list.AddRange(item.UserIds);
            }

            var rate = users.GetRate(AttributeName, value, list.Distinct());

            if (rate != double.NaN)
            {
                var r = rate / baseRate;
                if (r > 1.2)
                {
                    NewBrush(Color.FromArgb(255, 231, 159, 190), 0.5);
                }
                else if (r < 0.8)
                {
                    NewBrush(Color.FromArgb(255, 175,238, 238), 0.5);

                }
                else
                {
                    NewBrush(Colors.White, 0.5);

                }
            }
            else
            {
                NewBrush(Colors.White, 0.5);

            }

        }

        #region INotifyPropertyChanged メンバー

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
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
    public class LayerGroup
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public System.Collections.ObjectModel.ObservableCollection<Layer> Items { get; set; }
    }
}
