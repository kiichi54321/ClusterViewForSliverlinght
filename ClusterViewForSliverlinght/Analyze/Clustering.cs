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

namespace ClusterViewForSliverlinght.Analyze
{
    public class Clustering
    {
        public Models.ClusterTable ClusterTable { get; set; }
        int tryCount = 10;
        int takeComunity = 20;

        public int TryCount
        {
            get { return tryCount; }
            set { tryCount = value; }
        }

        public void RandomLayout()
        {
            foreach (var item in ClusterTable.LayerGroup)
            {
                List<Models.Comunity> comunityList = new List<Models.Comunity>();
                List<Models.Layer> layerList = new List<Models.Layer>();
                foreach (var layer in item.Items)
                {
                    layerList.Add(layer);
                    comunityList.AddRange(layer.Comunities);
                    layer.Comunities.Clear();
                }

                int c = 0;
                foreach (var comunity in comunityList.OrderBy(n=>new Guid()))
                {
                    layerList[c % layerList.Count].Comunities.Add(comunity);
                    c++;
                }
            }
        }

        public void Run()
        {
            foreach (var item in ClusterTable.Categories)
            {
                item.Update();
            }
            foreach (var item in ClusterTable.ComunityDic.Values)
            {
                foreach (var item2 in item.Relations)
                {
                    if (ClusterTable.ComunityDic.ContainsKey(item2.ItemId) == false)
                    {
                        item2.NotUse = true;
                    }
                    else
                    {
                        item2.NotUse = false;
                    }
                }
            }

            for (int i = 0; i < TryCount; i++)
            {
                bool endFlag = true;
                foreach (var item in ClusterTable.AllComunity)
                {
                    Dictionary<Models.Category, double> countDic = new Dictionary<Models.Category, double>();
                    foreach (var item2 in item.Relations.Where(n=>n.NotUse == false).OrderByDescending(n => n.補正確信度).Take(takeComunity))
                    {
                        var c = ClusterTable.ComunityDic[item2.ItemId].Category;

                        if (countDic.ContainsKey(c))
                        {
                            countDic[c] += item2.補正確信度;
                        }
                        else
                        {
                            countDic.Add(c, item2.補正確信度);
                        }
                    }
                    if (countDic.Count == 0)
                    {
                        item.Layer.Tmp2Comunities.Add(item);
                    }
                    else
                    {

                        var selectedCategory = countDic.OrderByDescending(n => n.Value / (double)n.Key.GetTmpComuity().Count()).Select(n => n.Key).First();
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

                foreach (var item in ClusterTable.Categories)
                {
                    item.UpdateTmp();
                }
                if (endFlag) break;
            }

            foreach (var item in ClusterTable.Categories)
            {
                item.UpDateView();
            }
        }
    }
}
