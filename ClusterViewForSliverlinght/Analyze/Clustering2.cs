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
using System.Collections.Generic;
using System.Linq;
using ClusterViewForSliverlinght.Models;
using RawlerLib.MyExtend;

namespace ClusterViewForSliverlinght.Analyze
{
    public class Clustering2
    {
        public Clustering2()
        {
            Result = new System.Collections.ObjectModel.ObservableCollection<ClusteringResult>();
        }

        public Models.ClusterTable ClusterTable { get; set; }
        int tryCount = 10;
        int takeComunity = 20;
        public int TakeComunity
        {
            get { return takeComunity; }
            set { takeComunity = value; }
        }
        Random random = new Random();
        public int TryCount
        {
            get { return tryCount; }
            set { tryCount = value; }
        }
        int wideCount = 1;

        public int WideCount
        {
            get { return wideCount; }
            set { wideCount = value; }
        }

        int minCategoryComunityCount = 5;

        public int MinCategoryComunityCount
        {
            get { return minCategoryComunityCount; }
            set { minCategoryComunityCount = value; }
        }

        public System.Collections.ObjectModel.ObservableCollection<ClusteringResult> Result { get; set; }

        public Action<string> Report { get; set; }
        public Action<double> Progress { get; set; }
        protected void OnReport(string text)
        {
            if (Report != null)
            {
                Report(DateTime.Now.ToShortTimeString() + "\t" + text);
            }
        }
        protected void OnProgress(double d)
        {
            if (this.Progress != null)
            {
                Progress(d);
            }

        }

        ClusteringResult Run3()
        {
            OnReport("開始");
            ClusteringResult cr = new ClusteringResult() { Parent = this };
            cr.CreateRandom(ClusterTable, random.Next());
            cr.Run();
            OnReport("計算終了\n");
            GC.Collect();
            return cr;
        }

        /// <summary>
        /// 過去のもの。頑張りすぎた。
        /// </summary>
        /// <returns></returns>
        ClusteringResult Run2()
        {
            List<ClusteringResult> tmpResult = new List<ClusteringResult>();
            OnReport("開始");
            for (int i = 0; i < wideCount; i++)
            {
                ClusteringResult cr = new ClusteringResult() { Parent = this };
                cr.CreateRandom(ClusterTable, random.Next());
                tmpResult.Add(cr);
            }


            int loopCount = 0;
            int sameCount = 0;
            double sameRate = 0;
            while (true)
            {

                loopCount++;
                if (tmpResult.First().GetLockRate() == sameRate)
                {
                    sameCount++;
                }
                else
                {
                    sameRate = tmpResult.First().GetLockRate();
                    sameCount = 0;
                }

                OnReport(loopCount + "回目開始");
                foreach (var item in tmpResult)
                {
                    item.Run();
                }
                OnReport("計算終了");

                var baseResult = tmpResult.OrderByDescending(n => n.GetPerformanceIndex()).First();
                foreach (var item in tmpResult)
                {
                    item.CategorySort(baseResult);
                }
                tmpResult = tmpResult.OrderByDescending(n => n.GetPerformanceIndex()).ToList();
                List<ClusteringResult> list = new List<ClusteringResult>();
                foreach (var item in tmpResult.Take(tmpResult.Count / 2))
                {
                    var clone = item.Clone();
                    list.Add(clone);
                }
                if (tmpResult.Count > 3)
                {
                    foreach (var item in tmpResult.Skip(tmpResult.Count / 2).ToArray())
                    {
                        tmpResult.Remove(item);
                    }
                    foreach (var item in list)
                    {
                        tmpResult.Add(item.Clone());
                    }
                }

                if (tmpResult.First().GetLockRate() < 0.9 && loopCount < 100 && sameCount < 10)
                {
                    int c = 0;
                    OnReport("マージ・ランダム振り開始");
                    foreach (var item in tmpResult)
                    {
                        var count = item.CreateMargeData(list, true);
                        //  lock (this)
                        {
                            c += count;
                        }
                    }
                    OnReport("レポート開始");
                    OnReport(tmpResult.First().View());

                    if (c == 0) break;
                }
                else
                {
                    foreach (var item in tmpResult)
                    {
                        var count = item.CreateMargeData(list, false);
                    }
                    tmpResult.First().Run();
                    break;
                }

                if (tmpResult.First().DivideCategory())
                {
                    OnReport("クラスタ分割発生");
                    var r = tmpResult.First();
                    tmpResult.Clear();
                    for (int i = 0; i < TryCount; i++)
                    {
                        tmpResult.Add(r.Clone());
                    }
                }
                GC.Collect();
            }
            GC.Collect();
            OnReport("完了");
            return tmpResult.First();
        }
        public bool IsBusy { get; set; }



        public void Run()
        {
            IsBusy = true;
            for (int i = 0; i < tryCount; i++)
            {
                var r = Run3();

                MyLib.Task.Utility.UITask(() =>
                    {
                        bool flag = true;
                        for (int k = 0; k < Result.Count; k++)
                        {
                            var p = r.GetPerformanceIndex();
                            if (Result[k].PerformaceIndex < p)
                            {
                                Result.Insert(k, r);
                                flag = false;
                                break;
                            }
                        }
                        if (flag) Result.Add(r);
                    });
                Progress(i / (double)tryCount);
            }
            Progress(1);


            var baseResult = Result.OrderByDescending(n => n.ClusterNum).ThenByDescending(n => n.PerformaceIndex).First();
            foreach (var item in Result)
            {
                item.CategorySort(baseResult);
            }

            MyLib.Task.Utility.UITask(() =>
            {
                Result.SetList(Result.OrderByDescending(n => n.ClusterNum).ThenByDescending(n => n.PerformaceIndex));     
            });
      

            IsBusy = false;
        }
        public void Update()
        {
            Result.First().Update(ClusterTable);
        }

        public void Update(int i)
        {
            if (Result.Count > i)
            {
                Result[i].Update(ClusterTable);
            }
            else
            {
                Update();
            }
        }
    }

    public class ComunityEx : Comunity
    {
        public ComunityEx(Comunity c)
            : base()
        {
            this.Deleted = c.Deleted;
            this.Id = c.Id;
            this.ImageUrl = c.ImageUrl;
            this.Index = c.Index;
            this.Name = c.Name;
            this.Relations = c.Relations;
            this.Tag = c.Tag;
            this.UserIds = c.UserIds;
            this.OrignalComunity = c;
            this.Lock = c.Lock;
        }
        public int MatchCount { get; set; }
        public Comunity OrignalComunity { get; set; }

        private List<RelationData> relationDataList = new List<RelationData>();

        public List<RelationData> RelationDataList
        {
            get { return relationDataList; }
            set { relationDataList = value; }
        }

        public void CreateRealtionData(int takeComunity)
        {
            relationDataList = Relations.Where(n => n.NotUse == false).OrderByDescending(n => n.補正確信度).Take(takeComunity).Select(n => new RelationData() { ComunityId = n.ItemId, Value = n.補正確信度 }).ToList();
        }
     
    }

    public struct RelationData
    {
        public int ComunityId { get; set; }
        public double Value { get; set; }
    }

    public class ClusteringResult
    {
        public System.Collections.ObjectModel.ObservableCollection<Category> Categories { get; set; }
        public Dictionary<string, LayerGroup> LayerGroup { get; set; }
        public List<ComunityEx> ComunityList { get; set; }
        public Dictionary<int, ComunityEx> ComunityDic { get; set; }
        public string ViewText { get { return "評価指標:" + GetPerformanceIndex().ToString("F3")+ performanceText; } }
        public Clustering2 Parent { get; set; }

        //      int takeComunity = 20;
        public int TakeComunity
        {
            get { return Parent.TakeComunity; }
        }
        public int MinCategoryComunityCount
        {
            get { return Parent.MinCategoryComunityCount; }
        }
        public ClusteringResult()
        {
            Categories = new System.Collections.ObjectModel.ObservableCollection<Category>();
            LayerGroup = new Dictionary<string, LayerGroup>();
            ComunityList = new List<ComunityEx>();
            ComunityDic = new Dictionary<int, ComunityEx>();
        }

        private string performanceText = string.Empty;
        double performaceIndex = 0;

        public double PerformaceIndex
        {
            get { return performaceIndex; }
            set { performaceIndex = value; }
        }

        public int ClusterNum { get; set; }

        public double GetPerformanceIndex()
        {
            foreach (var item in Categories)
            {
                item.Update();
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

                foreach (var item2 in item.Value.Relations.Where(n=>n.NotUse == false).OrderByDescending(n => n.補正確信度).Take(TakeComunity))
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

            double sum = 1;
            var l1 = this.Categories.Select(n => (double)n.Layer.SelectMany(m => m.Value.Comunities).SelectMany(l=>l.UserIds).Distinct().Count()).ToArray();
            sum = MyLib.Statistics.AvgStd.Getジニ係数(l1);
            ClusterNum = l1.Where(n => n> 0).Count();
            //foreach (var item in this.LayerGroup)
            //{
            //    List<double> list2 = new List<double>();
            //    foreach (var item2 in item.Value.Items)
            //    {
            //        list2.Add(item2.Comunities.Count);
            //    }
            //    sum = sum * (1 - MyLib.Statistics.AvgStd.Getジニ係数(list2.ToArray()));
            //}
            performanceText = "クラスタ数:" + ClusterNum + " 集中度:" + list.Where(n => double.IsNaN(n.Item1) == false).Select(n => n.Item1).Average().ToString("F3") + " 分散度:" + sum.ToString("F3");
            PerformaceIndex = list.Where(n=>double.IsNaN( n.Item1)==false).Select(n => n.Item1).Average();
            return PerformaceIndex;
            //return list.Select(n => n.Item1).Average() * Math.Log(1/ sum);
        }


        public ClusteringResult Clone()
        {
            ClusteringResult target = new ClusteringResult();
            target.Parent = this.Parent;
            HashSet<string> layerNames = new HashSet<string>();
            List<Category> categoryList = new List<Category>();
            target.checkDividCategoryDic = this.checkDividCategoryDic;
            target.random = new Random(random.Next());
            foreach (var category in Categories)
            {
                var ca = new Category();
                target.Categories.Add(ca);
                categoryList.Add(ca);
                foreach (var item in category.Layer)
                {
                    var layer = ca.GetLayer(item.Key);
                    layerNames.Add(item.Key);
                    foreach (ComunityEx comuinity in item.Value.Comunities)
                    {
                        layer.Comunities.Add(comuinity);
                        target.ComunityList.Add(comuinity);
                        target.ComunityDic.Add(comuinity.Id, comuinity);
                    }
                }
            }

            foreach (var item in layerNames)
            {
                LayerGroup lg = new LayerGroup()
                {
                    Name = item,
                    Items = new System.Collections.ObjectModel.ObservableCollection<Layer>()
                };
                target.LayerGroup.Add(item, lg);
                foreach (var item2 in Categories)
                {
                    lg.Items.Add(item2.GetLayer(item));
                }
            }
            return target;
        }

        public void AddNewCategory()
        {
            var category = new Category();
            category.Name = "残余";
            foreach (var item in this.ComunityDic.Values.Where(n => n.Lock == false))
            {
                category.GetLayer(item.Layer.Name).Comunities.Add(item);
            }
            this.Categories.Add(category);
        }

        public void CategorySort(ClusteringResult baseResult)
        {
            List<Category> list = new List<Category>();
            List<Category> list2 = new List<Category>(this.Categories);
            foreach (var item in baseResult.Categories)
            {
                if (list2.Count > 0)
                {
                    var idList = item.GetComuity().Select(n => n.Id);
                    var c = list2.OrderByDescending(n => n.GetComuity().Select(m => m.Id).Intersect(idList).Count()).First();
                    list2.Remove(c);
                    list.Add(c);
                }
                else
                {
                    list.Add(new Category());
                }
            }
            this.Categories.Clear();
            foreach (var item in list)
            {
                this.Categories.Add(item);
            }

            var layerNames = this.LayerGroup.Select(n => n.Key).Distinct().ToArray();
            LayerGroup.Clear();
            foreach (var item in layerNames)
            {
                LayerGroup lg = new LayerGroup() { Name = item, Items = new System.Collections.ObjectModel.ObservableCollection<Layer>() };
                LayerGroup.Add(item, lg);
                foreach (var item2 in Categories)
                {
                    lg.Items.Add(item2.GetLayer(item));
                }
            }
        }


        public void Update(ClusterTable ct)
        {
            MyLib.Task.Utility.UITask(() =>
            {
                var orignalCategory = ct.Categories.GetEnumerator();
                foreach (var item in this.Categories)
                {
                    orignalCategory.MoveNext();
                    if (orignalCategory.Current != null)
                    {
                        foreach (var item2 in orignalCategory.Current.Layer)
                        {
                            item2.Value.Comunities.Clear();
                            foreach (ComunityEx item3 in item.GetLayer(item2.Key).Comunities)
                            {
                                item3.OrignalComunity.IsHub = item3.IsHub;
                                item2.Value.Comunities.Add(item3.OrignalComunity);
                            }
                        }
                    }
                }
            });
        }
        Random random;
        public void CreateRandom(ClusterTable ct, int seed)
        {
            Dictionary<Category, Category> cDic = new Dictionary<Category, Category>();
            foreach (var item in ct.Categories)
            {
                var c = new Category() { KeyName = item.KeyName, Name = item.Name };
                Categories.Add(c);
                cDic.Add(item, c);
                item.Update();
            }           

            random = new Random(seed);

            foreach (var item in ct.LayerGroup)
            {
                List<Models.Comunity> comunityList = new List<Models.Comunity>();
                List<Models.Layer> layerList = new List<Models.Layer>();
                foreach (var layer in item.Items)
                {
                    comunityList.AddRange(layer.Comunities);
                    // layer.Comunities.Clear();
                }
                foreach (var item2 in Categories)
                {
                    var l = item2.GetLayer(item.Name);
                    layerList.Add(l);
                }
                int c = 0;
                foreach (var comunity in comunityList.OrderBy(n => random.NextDouble()))
                {
                    var cc = new ComunityEx(comunity);

                    if (cc.Lock)
                    {
                        cDic[cc.OrignalComunity.Category].GetLayer(item.Name).Comunities.Add(cc);
                    }
                    else
                    {
                        layerList[c % layerList.Count].Comunities.Add(cc);
                        c++;
                    }
                    comunityList.Add(cc);
                    ComunityDic.Add(comunity.Id, cc);
                    ComunityList.Add(cc);
                }
                LayerGroup.Add(item.Name, new LayerGroup() { Name = item.Name, Items = new System.Collections.ObjectModel.ObservableCollection<Layer>(layerList) });
            }
        }

        public double GetLockRate()
        {
            return this.ComunityDic.Values.Where(n => n.Lock == true).Count() / (double)this.ComunityDic.Count;
        }

        public int CreateMargeData(IEnumerable<ClusteringResult> results, bool arrangeRandom)
        {
            // var baseResult = results.OrderBy(n => new Grid()).First();
            foreach (var item in this.ComunityList)
            {
                item.MatchCount = 0;
            }
            var resultList = results.Where(n => n != this);

            foreach (var result in resultList)
            {
                var resultEnumerator = result.Categories.GetEnumerator();

                foreach (var item in Categories)
                {
                    resultEnumerator.MoveNext();
                    var list = resultEnumerator.Current.GetComuity().Select(n => n.Id).ToArray();
                    foreach (ComunityEx item2 in item.GetComuity())
                    {
                        if (list.Contains(item2.Id))
                        {
                            item2.MatchCount++;
                        }
                    }
                }
            }

            int border = resultList.Count();
            int moveCount = 0;
            foreach (var layerGroup in this.LayerGroup.Values)
            {
                List<ComunityEx> list = new List<ComunityEx>();
                foreach (var item in layerGroup.Items)
                {
                    List<ComunityEx> tmplist = new List<ComunityEx>();
                    foreach (ComunityEx item2 in item.Comunities)
                    {
                        if (item2.Lock == false)
                        {
                            if (item2.MatchCount < border)
                            {
                                list.Add(item2);
                                tmplist.Add(item2);
                            }
                            else
                            {
                                item2.Lock = true;
                            }
                        }
                    }
                    foreach (var item2 in tmplist)
                    {
                        item.Comunities.Remove(item2);
                    }
                }

                if (arrangeRandom)
                {
                    foreach (var item in list)
                    {
                        layerGroup.Items.OrderBy(n => random.NextDouble()).First().Comunities.Add(item);
                    }
                }
                else
                {
                    foreach (var item in list)
                    {
                        Dictionary<Models.Category, double> countDic = new Dictionary<Models.Category, double>();
                        foreach (var item2 in item.Relations.OrderByDescending(n => n.補正確信度))
                        {
                            if (ComunityDic[item2.ItemId].Lock == true)
                            {
                                var c = ComunityDic[item2.ItemId].Category;

                                if (countDic.ContainsKey(c))
                                {
                                    countDic[c] += item2.補正確信度;
                                }
                                else
                                {
                                    countDic.Add(c, item2.補正確信度);
                                }
                            }
                        }

                        var selectedCategory = countDic.OrderByDescending(n => n.Value / (double)n.Key.GetComuity().OfType<ComunityEx>().Where(m => m.Lock == true).Count()).Select(n => n.Key).First();
                        selectedCategory.GetLayer(layerGroup.Name).Comunities.Add(item);
                    }
                }
                moveCount += list.Count;
            }
            return moveCount;
        }


        #region 不使用
        Dictionary<Category, int> checkDividCategoryDic = new Dictionary<Category, int>();

        public bool DivideCategory()
        {
            bool flag = false;
            List<ChageCategoryData> list = new List<ChageCategoryData>();
            foreach (var item in Categories)
            {
                bool change = true;
                var count = item.GetComuity().OfType<ComunityEx>().Where(n => n.Lock == true).Count();
                if (checkDividCategoryDic.ContainsKey(item))
                {
                    if (checkDividCategoryDic[item] == count)
                    {
                        change = false;
                    }
                    else
                    {
                        checkDividCategoryDic[item] = count;
                    }
                }
                else
                {
                    checkDividCategoryDic.Add(item, count);
                }

                if (change)
                {
                    if (count > MinCategoryComunityCount * 2)
                    {

                        var f = DivideCategory(item);
                        if (f.IsChanged == true)
                        {
                            list.Add(f);
                        }
                    }
                }
            }
            foreach (var item in list)
            {
                Categories.Remove(item.OrignalCategory);
                foreach (var item2 in item.ChangedCategory)
                {
                    Categories.Add(item2);
                }
            }
            return flag;
        }

        private ChageCategoryData DivideCategory(Category category)
        {
            Category c1 = new Category();
            Category c2 = new Category();
            List<Category> categoryList = new List<Category>() { c1, c2 };
            List<ComunityEx> comunityList = new List<ComunityEx>();
            List<ComunityEx> comunityUnLockList = new List<ComunityEx>();

            bool flag = true;
            foreach (ComunityEx item in category.GetComuity().OfType<ComunityEx>().OrderBy(n => random.NextDouble()))
            {
                if (item.Lock)
                {
                    if (flag) c1.GetLayer(item.Layer.Name).Comunities.Add(item);
                    else
                    {
                        c2.GetLayer(item.Layer.Name).Comunities.Add(item);
                    }
                    flag = !flag;
                    comunityList.Add(item);
                }
                else
                {
                    comunityUnLockList.Add(item);
                }
            }
            foreach (var item in categoryList)
            {
                item.Update();
            }

            for (int i = 0; i < 500; i++)
            {
                bool endFlag = true;
                foreach (var item in comunityList)
                {
                    Dictionary<Models.Category, double> countDic = new Dictionary<Models.Category, double>();

                    foreach (var item2 in categoryList)
                    {
                        double sum = 0;
                        foreach (var item3 in item2.GetComuity())
                        {
                            if (item3.Relations.Where(n => n.ItemId == item.Id).Any())
                            {
                                sum += item3.Relations.Where(n => n.ItemId == item.Id).First().補正確信度;
                            }
                        }
                        countDic.Add(item2, sum);
                    }

                    {
                        var selectedCategory = countDic.OrderByDescending(n => n.Value / (double)n.Key.GetTmpComuity().Where(m => m != item).Count()).Select(n => n.Key).First();
                        if (selectedCategory == item.Category)
                        {
                            item.Layer.Tmp2Comunities.Add(item);
                        }
                        else
                        {
                            var layer = selectedCategory.GetLayer(item.Layer.Name);
                            layer.Tmp2Comunities.Add(item);
                            endFlag = false;
                        }
                    }
                }


                foreach (var item in categoryList)
                {
                    item.UpdateTmp();
                }
                if (endFlag) break;
            }

            foreach (var item in categoryList)
            {
                item.UpDateView();
            }

            if (c1.GetComuity().Count() >= MinCategoryComunityCount && c2.GetComuity().Count() >= MinCategoryComunityCount)
            {
                ChageCategoryData ccd = new ChageCategoryData() { IsChanged = true, OrignalCategory = category, ChangedCategory = new List<Category>() { c1, c2 } };
                flag = true;
                foreach (var item in comunityUnLockList)
                {
                    if (flag) c1.GetLayer(item.Layer.Name).Comunities.Add(item);
                    else
                    {
                        c2.GetLayer(item.Layer.Name).Comunities.Add(item);
                    }
                    flag = !flag;
                }
                return ccd;
            }
            return new ChageCategoryData() { IsChanged = false };
        }

        #endregion



        public struct ChageCategoryData
        {
            public bool IsChanged { get; set; }
            public Category OrignalCategory { get; set; }
            public List<Category> ChangedCategory { get; set; }
        }

        public string View()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //            sb.AppendLine("---");
            sb.AppendLine("LockRate：" + this.GetLockRate().ToString("F3"));
            //            sb.AppendLine("LockCount:" + ComunityList.Where(n => n.Lock == true).Count());
            //foreach (var item in ComunityDic.Values.Where(n => n.Lock == false))
            //{
            //    sb.Append(item.Name + ",");
            //}
            //sb.AppendLine();
            return sb.ToString();
        }

        public void Run()
        {
            foreach (var item in Categories)
            {
                item.Update();
            }
            foreach (var item in this.ComunityDic.Values)
            {
                foreach (var item2 in item.Relations)
                {
                    if (ComunityDic.ContainsKey(item2.ItemId) == false)
                    {
                        item2.NotUse = true;
                    }
                    else
                    {
                        if (ComunityDic[item2.ItemId].Category == null)
                        {
                            item2.NotUse = true;
                        }
                        else
                        {
                            item2.NotUse = false;
                        }
                    }
                }
                item.CreateRealtionData(TakeComunity);
            }


            for (int i = 0; i < 2000; i++)
            {
                bool endFlag = true;
                
                foreach (var item in ComunityList)
                {
                    if (item.Lock)
                    {
                        item.Layer.Tmp2Comunities.Add(item);
                    }
                    else
                    {
                        var countDic = item.RelationDataList.GroupBy(n => ComunityDic[n.ComunityId].Category).Select(n => new { key = n.Key, Value = n.Sum(m => m.Value) }).ToDictionary(n => n.key, n => n.Value);
                        if (countDic.Count == 0)
                        {
                            item.Layer.Tmp2Comunities.Add(item);
                        }
                        else
                        {
                            var selectedCategory = countDic.OrderByDescending(n => n.Value / (double)n.Key.GetTmpComuity().Where(m => m != item).Count()).Select(n => n.Key).First();
                            if (selectedCategory == item.Category)
                            {
                                item.Layer.Tmp2Comunities.Add(item);
                            }
                            else
                            {
                                var layer = selectedCategory.GetLayer(item.Layer.Name);
                                layer.Tmp2Comunities.Add(item);
                                endFlag = false;
                            }
                        }
                    }
                }

                foreach (var item in Categories)
                {
                    item.UpdateTmp();
                }
                if (endFlag) break;
            }

            foreach (var item in Categories)
            {
                item.UpDateView();
            }
        }

    }

}
