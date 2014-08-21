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

namespace ClusterViewForSliverlinght.Models
{
    [DataContract]
    public class Category : System.ComponentModel.INotifyPropertyChanged
    {
        public Category()
        {
            dic = new Dictionary<string, Layer>();
        }
        [DataMember]
        public string KeyName { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Tag { get; set; }

        Dictionary<string, Layer> dic = new Dictionary<string, Layer>();
        [DataMember]
        public Dictionary<string, Layer> Layer
        {
            get { return dic; }
            set { dic = value; }
        }

        public void Init()
        {
            if (dic != null)
            {
                foreach (var item in dic)
                {
                    item.Value.ComunitiesChange += new EventHandler(layer_ComunitiesChange);
                    item.Value.Init();
                }
                layer_ComunitiesChange(this, new EventArgs());
            }
        }

        public IEnumerable<Comunity> GetComuity()
        {
            foreach (var item in Layer.Values)
            {
                foreach (var item2 in item.Comunities.Where(n => n.Deleted == false))
                {
                    yield return item2;
                }
            }
        }
        public IEnumerable<Comunity> GetTmpComuity()
        {
            foreach (var item in Layer.Values)
            {
                foreach (var item2 in item.TmpComunities.Where(n => n.Deleted == false))
                {
                    yield return item2;
                }
            }
        }

        public void Update()
        {
            foreach (var item in dic.Values)
            {
                item.Update();
            }
            foreach (var item in GetComuity())
            {
                item.Category = this;
            }
        }

        public void UpdateTmp()
        {
            foreach (var item in dic.Values)
            {
                item.UpDataTmp();
            }
            foreach (var item in GetTmpComuity())
            {
                item.Category = this;
            }
        }

        public void UpDateView()
        {
            foreach (var item in dic.Values)
            {
                item.UpDataView();
            }
        }


        [DataMember]
        public List<CategoryAttribute> CategoryAttributeList { get; set; }

        public Layer GetLayer(string name)
        {
            if (dic.ContainsKey(name))
            {
                return dic[name];
            }
            else
            {
                var l = new Layer() { Category = this, Name = name };
                l.ComunitiesChange += new EventHandler(layer_ComunitiesChange);
                dic.Add(name, l);
                return l;
            }
        }

        void layer_ComunitiesChange(object sender, EventArgs e)
        {
            if (this.Layer.All(n => n.Value.Comunities.Any() == false))
            {
                DeleteButtonVisibility = Visibility.Visible;
            }
            else
            {
                DeleteButtonVisibility = Visibility.Collapsed;
            }
        }

        Visibility deleteButtonVisibility = Visibility.Visible;
        public Visibility DeleteButtonVisibility
        {
            get { return deleteButtonVisibility; }
            set
            {
                if (deleteButtonVisibility != value)
                {
                    deleteButtonVisibility = value;
                    OnPropertyChanged("DeleteButtonVisibility");
                }
            }
        }

        public string GetName()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return KeyName;
            }
            else
            {
                return Name;
            }
        }

        public string GetName(bool flag)
        {
            if (flag)
            {
                List<int> list = new List<int>();
                foreach (var item in dic.Values)
                {
                    foreach (var item2 in item.Comunities)
                    {
                        list.AddRange(item2.UserIds);
                    }
                }
                list = list.Distinct().ToList();
                return GetName() + "(" + list.Count + ")";
            }
            else
            {
                return GetName();
            }


        }


        internal int GetCount()
        {
            List<int> list = new List<int>();
            foreach (var item in dic.Values)
            {
                foreach (var item2 in item.Comunities)
                {
                    if (item2.Deleted == false && item2.UserIds != null)
                    {
                        list.AddRange(item2.UserIds);
                    }
                }
            }
            list = list.Distinct().ToList();
            return list.Count();
        }

        public int Count { get { return GetCount(); } }
        public double Rate { get { return Count * 100 / (double)AllUser; } }

        public int AllUser { get; set; }

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
    public class CategoryAttribute
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    [DataContract]
    public class CategoryAttributeGroup
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public System.Collections.ObjectModel.ObservableCollection<CategoryAttribute> Items { get; set; }
    }


}
