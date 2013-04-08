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
    public class Layer
    {
        public Layer()
        {
            comunities = new List<Comunity>();
        }
        //    [DataMember]
        public Category Category { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<Comunity> Comunities
        {
            get
            {
                return comunities;
            }
            set
            {
                comunities = value;
            }
        }

        List<Comunity> comunities = new List<Comunity>();
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
