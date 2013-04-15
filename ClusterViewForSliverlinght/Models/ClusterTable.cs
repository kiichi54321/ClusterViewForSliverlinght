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

namespace ClusterViewForSliverlinght.Models
{
    [DataContract]
    public class ClusterTable
    {
        [DataMember]
        public System.Collections.ObjectModel.ObservableCollection<Category> Categories { get; set; }
        [DataMember]
        public List<LayerGroup> LayerGroup { get; set; }
        [DataMember]
        public System.Collections.ObjectModel.ObservableCollection<CategoryAttributeGroup> CategoryAttributeGroup { get; set; }

        [DataMember]
        public Users UserData { get; set; }

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
            ct.Init();


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
            foreach (var item in LayerGroup)
            {
                foreach (var layer in item.Items)
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

        private IEnumerable<Comunity> AllComunity
        {
            get
            {
                foreach (var item in LayerGroup)
                {
                    foreach (var item2 in item.Items)
                    {
                        foreach (var item3 in item2.Comunities)
                        {
                            yield return item3;
                        }
                    }
                }
            }
        }

        public void Init()
        {
            CreateComunityDic();
            foreach (var item in comunityDic.Values)
            {
                item.Init();
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
                    Count = item.GetIntValue("User_Idのカウント", 0),
                    ImageUrl = item.GetValue("Image_Url",string.Empty),
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

        public void AddComunityUserData(System.IO.StreamReader sr)
        {
            foreach (var item in comunityDic.Values)
            {
                item.UserIds = new List<int>();
            }

            foreach (var item in TSVFile.ReadLines(sr))
            {
                int c_id = item.GetIntValue("Community_Id");
                int u_id = item.GetIntValue("User_Id");
                if (comunityDic.ContainsKey(c_id))
                {
                    comunityDic[c_id].UserIds.Add(u_id);
                }
            }
        }

        public void AddUserData(System.IO.StreamReader sr)
        {
            UserData = new Users();
            UserData.Create(sr);
        }

        public IEnumerable<UserAttributeGroup> CreateUserAttributeGroup(IEnumerable<int> userIdList)
        {
            if (UserData != null && userIdList != null && userIdList.Any() == true)
            {
                Dictionary<string, UserAttributeGroup> userAttributeGroupDic = new Dictionary<string, UserAttributeGroup>();
                UserData.SetUpData();

                foreach (var item in userIdList.Distinct())
                {
                    if (UserData.UserDic.ContainsKey(item))
                    {
                        foreach (var item2 in UserData.UserDic[item].AttributeDic)
                        {
                            if (userAttributeGroupDic.ContainsKey(item2.Key))
                            {
                                userAttributeGroupDic[item2.Key].Add(item2.Value);
                            }
                            else
                            {
                                userAttributeGroupDic.Add(item2.Key, new UserAttributeGroup() { GroupName = item2.Key });
                                userAttributeGroupDic[item2.Key].Add(item2.Value);
                            }
                        }
                    }
                }
                return userAttributeGroupDic.Values;
            }
            return new List<UserAttributeGroup>();
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

            int half = max / 2;
            int count = 0;
            foreach (var item in comunity.Relations.OrderByDescending(n => n.GetIndex(type)).Take(max))
            {
                if (comunityDic.ContainsKey(item.ItemId))
                {
                    if (count > half)
                    {
                        comunityDic[item.ItemId].NewBrush(Colors.Orange, 0.2);
                    }
                    else
                    {
                        comunityDic[item.ItemId].NewBrush(Colors.Orange, 0.6);
                    }
                    count++;
                }
            }

        }
        public void ViewRelation(int max, RelationIndexType type)
        {
            if (comunityDic == null)
            {
                CreateComunityDic();
            }
            foreach (var item in comunityDic.Values.Where(n=>n.Selected==false))
            {
                item.NewBrush(Colors.White, 1);
            }


            var selectedList = AllComunity.Where(n => n.Selected == true);
            List<ItemRelation> relationList = new List<ItemRelation>();
            foreach (var item in selectedList)
            {
                relationList.AddRange(item.Relations.Where(n=>selectedList.Where(m=>m.Id ==n.ItemId).Any()==false)) ;
            }
            var d = relationList.GroupBy(n=>n.ItemId).Select(n=>new { n.Key, a = n.Aggregate(1.0,(m,l)=> l.GetIndex(type)*m)});

            int half = max / 2;
            int count = 0;

            foreach (var item in d.OrderByDescending(n=>n.a).Where(n=>comunityDic.ContainsKey( n.Key)==true).Take(max))
            {
                    if (count > half)
                    {
                        comunityDic[item.Key].NewBrush(Colors.Orange, 0.2);
                    }
                    else
                    {
                        comunityDic[item.Key].NewBrush(Colors.Orange, 0.6);
                    }
                    count++;
            }

            //foreach (var item in comunity.Relations.OrderByDescending(n => n.GetIndex(type)).Take(max))
            //{
            //    if (comunityDic.ContainsKey(item.ItemId))
            //    {
            //        if (count > half)
            //        {
            //            comunityDic[item.ItemId].NewBrush(Colors.Orange, 0.2);
            //        }
            //        else
            //        {
            //            comunityDic[item.ItemId].NewBrush(Colors.Orange, 0.6);
            //        }
            //        count++;
            //    }
            //}

        }
        public void MoveComunity(Comunity comunity, Comunity mouseOver, Layer moveTolayer)
        {
            Layer currentLayer = null;
            foreach (var item in this.LayerGroup)
            {
                foreach (var layer in item.Items)
                {
                    if (layer.Comunities.Contains(comunity))
                    {
                        currentLayer = layer;
                        break;
                    }
                }
            }

            //   if (currentLayer != moveTolayer)
            if (currentLayer.Comunities.Contains(comunity))
            {
                currentLayer.Comunities.Remove(comunity);
                if (mouseOver != null && moveTolayer.Comunities.Contains(mouseOver))
                {
                    var p = moveTolayer.Comunities.IndexOf(mouseOver);
                    moveTolayer.Comunities.Insert(p, comunity);
                }
                else
                {
                    moveTolayer.Comunities.Add(comunity);
                }
            }
        }

        public void ChangeVisibility()
        {
            foreach (var item in AllComunity)
            {
                item.ChaqngeVisibility();
            }
        }
        public void AllUnSeleted()
        {
            foreach (var item in AllComunity)
            {
                item.Selected = false;
            }
        }

        public IEnumerable<int> GetUserIdList(Comunity c, string typeText)
        {
            if (typeText == "単独")
            {
                if (MainPage.Mode == MainPage.MouseMode.選択)
                {
                    List<int> list = new List<int>();
                    foreach (var item in AllComunity.Where(n=>n.Selected==true))
                    {
                        list.AddRange(item.UserIds);
                    }
                    return list.Distinct();
                }
                else
                {
                    return c.UserIds;
                }
            }
            else
            {
                Layer layer = null;
                int p = 0;
                bool flag = false;
                foreach (var item in LayerGroup)
                {
                    p = 0;
                    foreach (var item2 in item.Items)
                    {
                        if (item2.Comunities.Contains(c))
                        {
                            layer = item2;
                            flag = true;
                            break;
                        }
                        p++;
                    }
                    if (flag) break;
                }
                if (layer == null)
                {
                    return new List<int>();
                }
                if (typeText == "列")
                {
                    List<int> list = new List<int>();

                    foreach (var item in LayerGroup)
                    {
                        if (item.Items.Count > p)
                        {
                            foreach (var item2 in item.Items.ElementAtOrDefault(p).Comunities.Where(n=>n.Deleted == false))
                            {
                                list.AddRange(item2.UserIds);
                            }
                        }
                    }
                    return list.Distinct();
                }
                else
                {
                    List<int> list = new List<int>();
                    foreach (var item in layer.Comunities.Where(n => n.Deleted == false))
                    {
                        list.AddRange(item.UserIds);
                    }
                    return list.Distinct();
                }
            }

        }

    }

}
