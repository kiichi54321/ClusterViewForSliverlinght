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
using RawlerLib.MyExtend;

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


        #region Shift

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
                //カテゴリの追加時にバグる
                //foreach (var item in CategoryAttributeGroup)
                //{
                //    var tmp = item.Items[postinon];
                //    item.Items.RemoveAt(postinon);
                //    item.Items.Insert(postinon2, tmp);
                //}

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
                //カテゴリの追加時にバグる
                //foreach (var item in CategoryAttributeGroup)
                //{
                //    var tmp = item.Items[postinon];
                //    item.Items.RemoveAt(postinon);
                //    item.Items.Insert(postinon2, tmp);
                //}
            }

        }
        #endregion

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

        public Dictionary<int, Comunity> ComunityDic
        {
            get { return comunityDic; }
            set { comunityDic = value; }
        }
        void CreateComunityDic()
        {
            if (comunityDic == null)
            {
                comunityDic = new Dictionary<int, Comunity>();
            }
            comunityDic.Clear();
            foreach (var item in Categories)
            {
                foreach (var item2 in item.Layer)
                {
                    foreach (var comunity in item2.Value.Comunities)
                    {
                        if (comunityDic.ContainsKey(comunity.Id) == false)
                        {
                            comunityDic.Add(comunity.Id, comunity);
                        }
                    }
                }
            }
        }

        public IEnumerable<Comunity> AllComunity
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
        public void SetAllUser()
        {
            List<int> userList = new List<int>();
            foreach (var item in AllComunity)
            {
                if (item.Deleted == false && item.UserIds != null)
                {
                    userList.AddRange(item.UserIds);
                }
            }
            var all = userList.Distinct().Count();

            foreach (var item in Categories)
            {
                item.AllUser = all;
            }
        }


        public void Init()
        {
            CreateComunityDic();
            CreateLayerGroup(this);
            foreach (var item in Categories)
            {
                item.Init();
            }
            foreach (var item in comunityDic.Values)
            {
                item.Init();
            }
            SetAllUser();

        }

        public void ChangeColorByAttribute(string attributeName, string value)
        {
            var baseRate = UserData.GetRate(attributeName, value, this.UserData.UserList.Select(n => n.Id));

            foreach (var item in LayerGroup)
            {
                foreach (var item2 in item.Items)
                {
                    item2.ChangeAttributeColor(UserData, attributeName, value, baseRate);
                }
            }
        }

        public static ClusterTable Create(System.IO.StreamReader sr, out IEnumerable<string> errList)
        {
            Dictionary<string, Category> categoryDic = new Dictionary<string, Category>();
            Dictionary<string, List<Layer>> layerListDic = new Dictionary<string, List<Layer>>();
            List<Comunity> comunityList = new List<Comunity>();


            foreach (var item in TSVFile.ReadLines(sr, out errList))
            {
                Comunity comunity = new Comunity()
                {
                    Id = item.GetIntValue("Community_Id"),
                //    Count = item.GetIntValue("User_Idのカウント", 0),
                    ImageUrl = item.GetValue("Image_Url", string.Empty),
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

            CreateLayerGroup(clusterTable);

            //foreach (var item in layerNameList)
            //{
            //    var lg = new LayerGroup() { Name = item, Items = new System.Collections.ObjectModel.ObservableCollection<Layer>() };
            //    clusterTable.LayerGroup.Add(lg);
            //    foreach (var item2 in categoryDic.Values)
            //    {
            //        if (item2.Layer.ContainsKey(item)==false)
            //        {
            //            item2.Layer.Add(item, new Layer());
            //        }
            //        lg.Items.Add(item2.Layer[item]);
            //    }
            //}

            comunityList.Clear();
            foreach (var item in layerListDic.Values)
            {
                foreach (var item2 in item)
                {
                    comunityList.AddRange(item2.Comunities);
                }
            }
            clusterTable.CreateRelation();
            return clusterTable;
        }

        public static void CreateLayerGroup(ClusterTable clusterTable)
        {
            clusterTable.LayerGroup = new List<LayerGroup>();
            List<string> layerNameList = new List<string>();
            foreach (var item in clusterTable.Categories)
            {
                foreach (var item2 in item.Layer)
                {
                    layerNameList.Add(item2.Value.Name);
                }
            }
            layerNameList = layerNameList.Where(n => n != null).Distinct().OrderBy(n => n).ToList();
            foreach (var item in layerNameList)
            {
                var lg = new LayerGroup() { Name = item, Items = new System.Collections.ObjectModel.ObservableCollection<Layer>() };
                clusterTable.LayerGroup.Add(lg);
                foreach (var item2 in clusterTable.Categories)
                {
                    if (item2.Layer.ContainsKey(item) == false)
                    {
                        item2.Layer.Add(item, new Layer());
                    }
                    lg.Items.Add(item2.Layer[item]);
                }
            }

        }

        public void ReadCominityLayerData(System.IO.StreamReader sr, int clusterNum, out IEnumerable<string> errList)
        {
            if (Categories == null) Categories = new System.Collections.ObjectModel.ObservableCollection<Category>();
            Categories.Clear();
            for (int i = 0; i < clusterNum; i++)
            {
                Categories.Add(new Category() { KeyName = "C_" + i });
            }
            this.CategoryAttributeGroup = new System.Collections.ObjectModel.ObservableCollection<Models.CategoryAttributeGroup>();
            Random random = new Random();
            foreach (var item in TSVFile.ReadLines(sr, out errList))
            {
                Comunity comunity = new Comunity()
                {
                    Id = item.GetIntValue("Community_Id"),
                    ImageUrl = item.GetValue("Image_Url", string.Empty),
                    Name = item.GetValue("Community_Name")
                };
                string layer = item.GetValue("レイヤー");
                var r = random.Next(clusterNum);
                Categories[r].GetLayer(layer).Comunities.Add(comunity);
            }
            CreateLayerGroup(this);
        }

        public static ClusterTable CreateDataForTransaction(System.IO.StreamReader sr, int clusterNum, Dictionary<string, List<string>> layerDicData, string[] containItems,Dictionary<string,string> nayoseDic, out IEnumerable<string> errList)
        {
            var clusterTable = new ClusterTable();
            clusterTable.Categories = new System.Collections.ObjectModel.ObservableCollection<Category>();
            clusterTable.LayerGroup = new List<LayerGroup>();
            clusterTable.CategoryAttributeGroup = new System.Collections.ObjectModel.ObservableCollection<CategoryAttributeGroup>();

            ///レイヤー構造の作成
            for (int i = 0; i < clusterNum; i++)
            {
                clusterTable.Categories.Add(new Category() { KeyName = "C_" + i });
            }
            Random random = new Random();

            Dictionary<string, Comunity> comnutyDic = new Dictionary<string, Comunity>();
            int item_id = 0;
            foreach (var layer in layerDicData)
            {
                foreach (var item in layer.Value)
                {
                    Comunity comunity = new Comunity()
                    {
                        Id = item_id,
                        Name = item.Split(',').First()
                    };
                    comunity.GetImageData();
                    if (comnutyDic.ContainsKey(item) == false)
                    {
                        comnutyDic.Add(item, comunity);
                    }

                    foreach (var item2 in nayoseDic.Where(n=>n.Value == item).Select(n=>n.Key))
                    {
                        if(comnutyDic.ContainsKey(item2)==false)
                        {
                            comnutyDic.Add(item2, comunity);
                        }
                    }

                    var r = random.Next(clusterNum);
                    clusterTable.Categories[r].GetLayer(layer.Key).Comunities.Add(comunity);
                    item_id++;
                }
            }


            CreateLayerGroup(clusterTable);

            //関係データの挿入
            clusterTable.CreateComunityDic();
            foreach (var item in clusterTable.comunityDic.Values)
            {
                item.UserIds = new List<int>();
                item.Relations.Clear();
            }
            List<string> list = new List<string>();
            List<int> idList = new List<int>();
            string tmp_id = string.Empty;
            int u_id = 0;
            int u_count = 0;
            List<string> itemList = new List<string>();
            HashSet<string> hash = new HashSet<string>(containItems);
            Dictionary<string, int> userIdDic = new Dictionary<string, int>();
            Users users = new Users();
            foreach (var line in TSVFile.ReadLines(sr, out errList))
            {
                u_id = userIdDic.GetValueOrAdd(line.GetValue(0), u_count++);
                var c_name = line.GetValue(1, string.Empty);
                var array = line.Line.Split('\t').ToArray(3);
                if (array[1].IsNullOrEmpty()== false)
                {
                    if (containItems.Length == 0 || (containItems.Length > 0 && hash.Contains(array[1])))
                    {
                        if (array[2].IsNullOrEmpty())
                        {
                            comnutyDic[array[1]].AddUserId(u_id);
                        }
                        else
                        {
                            users.Add(u_id, array[2], array[1]);
                        }
                    }
                    itemList.Clear();
                    itemList.Add(c_name);
                }
            }

            list.AddRange(errList);
            errList = list;
            clusterTable.CreateRelation();
            clusterTable.SetAllUser();
            clusterTable.UserData = users;
            clusterTable.Update();

            return clusterTable;
        }

        /// <summary>
        /// コミュニティ名から画像を取得する
        /// </summary>
        void ComunityGetImage()
        {
            foreach (var item in comunityDic)
            {
                item.Value.GetImageData();
            }
        }

        public void Update()
        {
            foreach(var item in this.LayerGroup.SelectMany(n=>n.Items))
            {
                item.Update();
            }
        }


        public void AddRelationData(System.IO.StreamReader sr, out IEnumerable<string> errList)
        {
            CreateComunityDic();
            foreach (var item in comunityDic.Values)
            {
                item.Relations.Clear();
            }
            foreach (var item in TSVFile.ReadLines(sr, out errList))
            {
                var id = item.GetIntValue("ITEM1", -1);
                if (comunityDic.ContainsKey(id))
                {
                    comunityDic[id].AddRelations(item);
                }
            }
        }

        public void RemoveComunity(Comunity comunity)
        {
            
        }

        public void AddComunityUserData(System.IO.StreamReader sr, out IEnumerable<string> errList)
        {
            CreateComunityDic();
            foreach (var item in comunityDic.Values)
            {
                item.UserIds = new List<int>();
                item.Relations.Clear();
            }
            List<string> list = new List<string>();
            List<int> idList = new List<int>();
            foreach (var item in TSVFile.ReadLines(sr, out errList))
            {
                int c_id = item.GetIntValue("Community_Id",-1);
                int u_id = item.GetIntValue("User_Id",-1);
                if (c_id != -1 && u_id != -1)
                {
                    if (comunityDic.ContainsKey(c_id))
                    {
                        comunityDic[c_id].AddUserId(u_id);
                        idList.Add(u_id);
                    }

                }
                else
                {
                    list.Add(item.Count + "行目エラー。値がありませんでした"); 
                }

            }
            list.AddRange(errList);
            errList = list;
            CreateRelation();
            SetAllUser();
            //int allCount = idList.Distinct().Count();

            //foreach (var item in comunityDic.Values)
            //{
            //    foreach (var item2 in comunityDic.Values.Where(n=>n != item))
            //    {
            //        int countItem1 = item.UserIds.Count;
            //        int countItem2 = item2.UserIds.Count;
            //        int common = item.UserIds.Intersect(item2.UserIds).Count();

            //        double c1 = (double)common / (double)countItem1;
            //        double c2 = c1 - (double)countItem2 / (double)allCount;
            //        item.AddRelations(item2.Id, c1, c2);
            //    }
            //}

        }

        private void CreateRelation()
        {
            CreateComunityDic();
            List<int> idList = new List<int>();

            foreach (var item in comunityDic.Values)
            {
                item.Relations.Clear();
                idList.AddRange(item.UserIds);
            }
            int allCount = idList.Distinct().Count();
            foreach (var item in comunityDic.Values)
            {
                foreach (var item2 in comunityDic.Values.Where(n => n != item))
                {
                    int countItem1 = item.UserIds.Count();
                    int countItem2 = item2.UserIds.Count();
                    int common = item.UserIds.Intersect(item2.UserIds).Count();

                    double c1 = (double)common / (double)countItem1;
                    double c2 = c1 - (double)countItem2 / (double)allCount;
                    item.AddRelations(item2.Id, c1, c2,common);
                }
            }
        }


        public void AddUserData(System.IO.StreamReader sr)
        {
            UserData = new Users();
            UserData.Create(sr);
        }





        public void ViewRelation(Comunity comunity, int max, RelationIndexType type, out List<ItemRelationViewData> list)
        {
            if (comunityDic == null)
            {
                CreateComunityDic();
            }
            foreach (var item in comunityDic.Values)
            {
                item.NewBrush(Colors.Transparent, 1);
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
            list = new List<ItemRelationViewData>();
            int i = 1;
            foreach (var item in comunity.Relations.OrderByDescending(n => n.GetIndex(type)))
            {
                if (comunityDic.ContainsKey(item.ItemId))
                {
                    var c = comunityDic[item.ItemId];
                    list.Add(new ItemRelationViewData() { Rank = i, Name = c.Name, 共起数 = item.共起数 ,確信度 = item.確信度.ToString("F3"), 補正確信度 = item.補正確信度.ToString("F3") });
                    i++;
                }
            }

        }
        public void ViewRelation(int max, RelationIndexType type, out List<ItemRelationViewData> list)
        {
            if (comunityDic == null)
            {
                CreateComunityDic();
            }
            foreach (var item in comunityDic.Values.Where(n => n.Selected == false))
            {
                item.NewBrush(Colors.Transparent, 1);
            }


            var selectedList = AllComunity.Where(n => n.Selected == true);
            List<ItemRelation> relationList = new List<ItemRelation>();
            foreach (var item in selectedList)
            {
                relationList.AddRange(item.Relations.Where(n => selectedList.Where(m => m.Id == n.ItemId).Any() == false));
            }
            var d = relationList.GroupBy(n => n.ItemId).Select(n => new { n.Key, a = n.Aggregate(1.0, (m, l) => l.GetIndex(type) * m) });

            int half = max / 2;
            int count = 0;

            foreach (var item in d.OrderByDescending(n => n.a).Where(n => comunityDic.ContainsKey(n.Key) == true).Take(max))
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
            int i = 1;
            list = new List<ItemRelationViewData>();
            foreach (var item in d.OrderByDescending(n => n.a))
            {
                if (comunityDic.ContainsKey(item.Key))
                {
                    var c = comunityDic[item.Key];
                    if (type == RelationIndexType.確信度)
                    {
                        list.Add(new ItemRelationViewData() { Rank = i, Name = c.Name, 確信度 = item.a.ToString("F3"), });
                    }
                    else
                    {
                        list.Add(new ItemRelationViewData() { Rank = i, Name = c.Name, 補正確信度 = item.a.ToString("F3") });
                    }
                    i++;
                }
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

        public void AddCategroy()
        {
            var category = new Category();
            foreach (var item in LayerGroup)
            {
                var layer = category.GetLayer(item.Name);
                item.Items.Add(layer);
            }
            int count = 1;
            while (true)
            {
                if (this.Categories.Where(n => n.KeyName == "C_" + count).Any())
                {
                    count++;
                }
                else
                {
                    category.KeyName = "C_" + count.ToString();
                    break;
                }
            }
            
            Categories.Add(category);            
        }

        public void RemoveCategory(Category category)
        {
            foreach (var item in LayerGroup)
            {
                var layer = category.GetLayer(item.Name);
                item.Items.Remove(layer);
            }
            Categories.Remove(category);
        }

        public IEnumerable<int> GetUserIdList(Comunity c, string typeText)
        {
            if(typeText == "全体")
            {
                return AllComunity.SelectMany(n => n.UserIds).Distinct();
             //   return UserData.UserList.Select(n=>n.Id).Distinct();
            }
            if (typeText == "選択")
            {
                if (MainPage.Mode == MainPage.MouseMode.複数選択)
                {
                    List<int> list = new List<int>();
                    foreach (var item in AllComunity.Where(n => n.Selected == true))
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
                            foreach (var item2 in item.Items.ElementAtOrDefault(p).Comunities.Where(n => n.Deleted == false))
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
                    foreach (var item in layer.Comunities.Where(n => n.Deleted == false || n.UserIds != null))
                    {
                        if (item.UserIds != null)
                        {
                            list.AddRange(item.UserIds);
                        }
                    }
                    return list.Distinct();
                }
            }

        }


        public void SearchHub()
        {
            foreach (var item2 in Categories)
            {
                item2.Update();
            }
            List<Tuple<double, Comunity>> list = new List<Tuple<double, Comunity>>();
            foreach (var item in ComunityDic)
            {
                Dictionary<Category, double> cDic = new Dictionary<Category, double>();
                foreach (var item2 in Categories)
                {
                    cDic.Add(item2, 0);
                }
                // cDic[item.Value.Category]++;

                foreach (var item2 in item.Value.Relations.Where(n => n.NotUse == false).OrderByDescending(n => n.補正確信度).Take(20))
                {
                    if (ComunityDic.ContainsKey(item2.ItemId))
                    {
                        if (cDic.ContainsKey(ComunityDic[item2.ItemId].Category))
                        {
                            cDic[ComunityDic[item2.ItemId].Category] += item2.補正確信度;
                        }
                        else
                        {
                            cDic.Add(ComunityDic[item2.ItemId].Category, item2.補正確信度);
                        }
                    }
                }
                item.Value.IsHub = false;
                list.Add(new Tuple<double, Comunity>(MyLib.Statistics.AvgStd.Getジニ係数(cDic.Values.ToArray()), item.Value));
            }
            foreach (var item in list.OrderBy(n => n.Item1).Take(10))
            {
                item.Item2.IsHub = true;
            }
        }
    }

}
