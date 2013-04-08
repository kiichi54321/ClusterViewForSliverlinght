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

namespace ClusterViewForSliverlinght.Models
{
    [DataContract]
    public class Category
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
                return new Layer() { Category = this, Name = name };
            }
        }
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
