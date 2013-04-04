using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace ClusterViewForSliverlinght
{
    //public class TableData
    //{
    //    Dictionary<string, Category> categoryDic = new Dictionary<string, Category>();

    //    public Dictionary<string, Category> CategoryDic
    //    {
    //        get { return categoryDic; }

    //    }
    //    Dictionary<string, List<Layer>> layerListDic = new Dictionary<string, List<Layer>>();
    //    List<Comunity> comunityList = new List<Comunity>();

    //    public Dictionary<string, List<Layer>> LayerListDic
    //    {
    //        get { return layerListDic; }

    //    }
    //    public void Add(string category, string layer, Comunity comunity)
    //    {
    //        Category c = null;
    //        if (categoryDic.ContainsKey(category))
    //        {
    //            c = categoryDic[category];
    //        }
    //        else
    //        {
    //            categoryDic.Add(category, new Category() { Name = category });
    //            c = categoryDic[category];
    //        }
    //        if (c.Layer.ContainsKey(layer))
    //        {
    //            c.Layer[layer].Comunities.Add(comunity);
    //        }
    //        else
    //        {
    //            c.Layer.Add(layer, new Layer() { Name = layer, Category = c });
    //            c.Layer[layer].Comunities.Add(comunity);
    //        }

    //    }

    //    void CreateLayerList()
    //    {
    //        layerListDic.Clear();
    //        clusterTable = new ClusterTable();
    //        clusterTable.Categories = new System.Collections.ObjectModel.ObservableCollection<Category>();
    //        clusterTable.LayerGroup = new List<LayerGroup>();

    //        List<string> layerNameList = new List<string>();
    //        foreach (var item in categoryDic)
    //        {
    //            foreach (var item2 in item.Value.Layer)
    //            {
    //                layerNameList.Add(item2.Value.Name);
    //            }
    //        }
    //        layerNameList = layerNameList.Distinct().OrderBy(n => n).ToList();

    //        foreach (var item in categoryDic)
    //        {
    //            clusterTable.Categories.Add(item.Value);
    //            foreach (var item2 in layerNameList)
    //            {
    //                if (layerListDic.ContainsKey(item2))
    //                {
    //                    layerListDic[item2].Add(item.Value.GetLayer(item2));
    //                }
    //                else
    //                {
    //                    layerListDic.Add(item2, new List<Layer>() { item.Value.GetLayer(item2) });
    //                }
    //            }
    //        }

    //        foreach (var item in layerNameList)
    //        {
    //            var lg = new LayerGroup() { Name = item, Items = new System.Collections.ObjectModel.ObservableCollection<Layer>() };
    //            clusterTable.LayerGroup.Add(lg);
    //            foreach (var item2 in categoryDic.Values)
    //            {
    //                lg.Items.Add(item2.Layer[item]);
    //            }
    //        }

    //        comunityList.Clear();
    //        foreach (var item in layerListDic.Values)
    //        {
    //            foreach (var item2 in item)
    //            {
    //                comunityList.AddRange(item2.Comunities);
    //            }
    //        }

    //    }

    //    public void Create(System.IO.StreamReader sr)
    //    {
    //        foreach (var item in CSVFile.ReadLines(sr))
    //        {
    //            Comunity c = new Comunity()
    //            {
    //                Id = item.GetIntValue("Community_Id"),
    //                //  ImageUrl = item.GetValue("Image_Url"),
    //                //  Index = item.GetDoubleValue("Index"),
    //                Name = item.GetValue("Community_Name")
    //            };
    //            Add(item.GetValue("帰属系"), item.GetValue("レイヤー"), c);
    //        }
    //        CreateLayerList();
    //    }


    //    ClusterTable clusterTable;
    //    public ClusterTable ClusterTable
    //    {
    //        get
    //        {
    //            return clusterTable;
    //        }
    //    }


    //}


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
    public class Comunity : INotifyPropertyChanged
    {
        public Comunity()
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
        public int ItemId { get; set; }
        public double 確信度 { get; set; }
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


    [DataContract]
    public class ClusterTable
    {
        [DataMember]
        public System.Collections.ObjectModel.ObservableCollection<Category> Categories { get; set; }
        [DataMember]
        public List<LayerGroup> LayerGroup { get; set; }
        [DataMember]
        public System.Collections.ObjectModel.ObservableCollection<CategoryAttributeGroup> CategoryAttributeGroup { get; set; }

        public void LeftShift(Category c)
        {
            int postinon = this.Categories.IndexOf(c);
            if (postinon > 0)
            {
                int postinon2 = postinon - 1;
                this.Categories.Remove(c);
                this.Categories.Insert(postinon2, c);
                foreach (var item in LayerGroup)
                {
                    var tmp = item.Items[postinon];
                    item.Items.RemoveAt(postinon);
                    item.Items.Insert(postinon2, tmp);
                }
                foreach (var item in CategoryAttributeGroup)
                {
                    var tmp = item.Items[postinon];
                    item.Items.RemoveAt(postinon);
                    item.Items.Insert(postinon2, tmp);
                }
            }
        }
        public void RightShift(Category c)
        {
            int postinon = this.Categories.IndexOf(c);
            if (postinon < this.Categories.Count - 1)
            {
                int postinon2 = postinon + 1;
                this.Categories.Remove(c);
                this.Categories.Insert(postinon, c);
                foreach (var item in LayerGroup)
                {
                    var tmp = item.Items[postinon];
                    item.Items.RemoveAt(postinon);
                    item.Items.Insert(postinon2, tmp);
                }
                foreach (var item in CategoryAttributeGroup)
                {
                    var tmp = item.Items[postinon];
                    item.Items.RemoveAt(postinon);
                    item.Items.Insert(postinon2, tmp);
                }
            }

        }

        #region SaveLoad
        public void Save(System.IO.Stream stream)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(ClusterTable));
            serializer.WriteObject(stream, this);
            stream.Close();
        }
        public static ClusterTable Load(System.IO.Stream stream)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(ClusterTable));
            ClusterTable ct = (ClusterTable)serializer.ReadObject(stream);
            stream.Close();
            return ct;
        }

        #endregion

        Dictionary<int, Comunity> comunityDic;
        void CreateComunityDic()
        {
            if (comunityDic == null)
            {
                comunityDic = new Dictionary<int, Comunity>();
            }
            comunityDic.Clear();
            foreach (var item in Categories)
            {
                foreach (var layer in item.Layer.Values)
                {
                    foreach (var comunity in layer.Comunities)
                    {
                        if (comunityDic.ContainsKey(comunity.Id) == false)
                        {
                            comunityDic.Add(comunity.Id, comunity);
                        }
                    }
                }
            }
        }

        public static ClusterTable Create(System.IO.StreamReader sr)
        {
            Dictionary<string, Category> categoryDic = new Dictionary<string, Category>();
            Dictionary<string, List<Layer>> layerListDic = new Dictionary<string, List<Layer>>();
            List<Comunity> comunityList = new List<Comunity>();


            foreach (var item in TSVFile.ReadLines(sr))
            {
                Comunity comunity = new Comunity()
                {
                    Id = item.GetIntValue("Community_Id"),
                    //  ImageUrl = item.GetValue("Image_Url"),
                    //  Index = item.GetDoubleValue("Index"),
                    Name = item.GetValue("Community_Name")
                };

                string categoryStr = item.GetValue("帰属系");
                string layer = item.GetValue("レイヤー");

                Category category = null;
                if (categoryDic.ContainsKey(categoryStr))
                {
                    category = categoryDic[categoryStr];
                }
                else
                {
                    categoryDic.Add(categoryStr, new Category() { KeyName = categoryStr, Name = item.GetValue("系名", string.Empty) });
                    category = categoryDic[categoryStr];
                    category.CategoryAttributeList = new List<CategoryAttribute>();
                    foreach (var item2 in item.GetValue("系指標", string.Empty).Split(','))
                    {
                        var array = item2.Split(':');
                        if (array.Length == 1)
                        {
                            category.CategoryAttributeList.Add(new CategoryAttribute() { Name = string.Empty, Value = array[0] });
                        }
                        else
                        {
                            category.CategoryAttributeList.Add(new CategoryAttribute() { Name = array[0], Value = array[1] });
                        }
                    }
                }
                if (category.Layer.ContainsKey(layer))
                {
                    category.Layer[layer].Comunities.Add(comunity);
                }
                else
                {
                    category.Layer.Add(layer, new Layer() { Name = layer, Category = category });
                    category.Layer[layer].Comunities.Add(comunity);
                }
            }

            var clusterTable = new ClusterTable();
            clusterTable.Categories = new System.Collections.ObjectModel.ObservableCollection<Category>();
            clusterTable.LayerGroup = new List<LayerGroup>();
            clusterTable.CategoryAttributeGroup = new System.Collections.ObjectModel.ObservableCollection<CategoryAttributeGroup>();

            List<string> layerNameList = new List<string>();
            List<string> categoryAttributeNameList = new List<string>();
            foreach (var item in categoryDic)
            {
                foreach (var item2 in item.Value.Layer)
                {
                    layerNameList.Add(item2.Value.Name);
                }
                foreach (var item2 in item.Value.CategoryAttributeList)
                {
                    categoryAttributeNameList.Add(item2.Name);
                }
            }
            layerNameList = layerNameList.Distinct().OrderBy(n => n).ToList();
            categoryAttributeNameList = categoryAttributeNameList.Distinct().ToList();
            Dictionary<string, CategoryAttributeGroup> categoryAttributeGroupDic = new Dictionary<string, CategoryAttributeGroup>();

            foreach (var item in categoryDic)
            {
                clusterTable.Categories.Add(item.Value);
                foreach (var item2 in layerNameList)
                {
                    if (layerListDic.ContainsKey(item2))
                    {
                        layerListDic[item2].Add(item.Value.GetLayer(item2));
                    }
                    else
                    {
                        layerListDic.Add(item2, new List<Layer>() { item.Value.GetLayer(item2) });
                    }
                }
                foreach (var item2 in categoryAttributeNameList)
                {
                    var attribute = item.Value.CategoryAttributeList.Where(n => n.Name == item2).FirstOrDefault();
                    if (attribute != null)
                    {
                        if (categoryAttributeGroupDic.ContainsKey(item2))
                        {
                            categoryAttributeGroupDic[item2].Items.Add(attribute);
                        }
                        else
                        {
                            categoryAttributeGroupDic.Add(item2, new CategoryAttributeGroup() { Name = item2, Items = new System.Collections.ObjectModel.ObservableCollection<CategoryAttribute>() });
                            categoryAttributeGroupDic[item2].Items.Add(attribute);
                        }
                    }
                    else
                    {
                        attribute = new CategoryAttribute();
                        if (categoryAttributeGroupDic.ContainsKey(item2))
                        {
                            categoryAttributeGroupDic[item2].Items.Add(attribute);
                        }
                        else
                        {
                            categoryAttributeGroupDic.Add(item2, new CategoryAttributeGroup() { Name = item2, Items = new System.Collections.ObjectModel.ObservableCollection<CategoryAttribute>() });
                            categoryAttributeGroupDic[item2].Items.Add(attribute);
                        }
                    }
                }
            }

            foreach (var item2 in categoryAttributeGroupDic.Values)
            {
                clusterTable.CategoryAttributeGroup.Add(item2);
            }

            foreach (var item in layerNameList)
            {
                var lg = new LayerGroup() { Name = item, Items = new System.Collections.ObjectModel.ObservableCollection<Layer>() };
                clusterTable.LayerGroup.Add(lg);
                foreach (var item2 in categoryDic.Values)
                {
                    lg.Items.Add(item2.Layer[item]);
                }
            }

            comunityList.Clear();
            foreach (var item in layerListDic.Values)
            {
                foreach (var item2 in item)
                {
                    comunityList.AddRange(item2.Comunities);
                }
            }
            return clusterTable;
        }

        public void AddRelationData(System.IO.StreamReader sr)
        {
            CreateComunityDic();
            foreach (var item in comunityDic.Values)
            {
                item.Relations.Clear();
            }
            foreach (var item in TSVFile.ReadLines(sr))
            {
                var id = item.GetIntValue("ITEM1");
                if (comunityDic.ContainsKey(id))
                {
                    comunityDic[id].AddRelations(item);
                }
            }
        }

        public void ViewRelation(Comunity comunity, int max, RelationIndexType type)
        {
            if (comunityDic == null)
            {
                CreateComunityDic();
            }
            foreach (var item in comunityDic.Values)
            {
                item.NewBrush(Colors.White, 1);
            }
            comunity.NewBrush(Colors.Red, 0.8);

            foreach (var item in comunity.Relations.OrderByDescending(n => n.GetIndex(type)).Take(max))
            {
                if (comunityDic.ContainsKey(item.ItemId))
                {
                    comunityDic[item.ItemId].NewBrush(Colors.Orange, 0.5);
                }
            }

        }

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
