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
using System.ComponentModel;

namespace ClusterViewForSliverlinght.View
{
    public class Graph
    {
        public Graph(Canvas canvas)
        {
            this.canvas = canvas;
            this.graphManage = new MyWpfLib.Graph.GraphManage(canvas);
            this.spring.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(spring_RunWorkerCompleted);
        }

        void spring_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            spring.NodeUpdate();
            graphManage.Update();
        }
        Canvas canvas;
        MyWpfLib.Graph.GraphManage graphManage;
        //        List<MyWpfLib.Graph.NodeControl> nodeList = new List<MyWpfLib.Graph.NodeControl>();
        List<MyWpfLib.Graph.Link> linkList = new List<MyWpfLib.Graph.Link>();
        Dictionary<Models.Category, MyWpfLib.Graph.NodeControl> nodeDic = new Dictionary<Models.Category, MyWpfLib.Graph.NodeControl>();
        Dictionary<string, Models.Category> categoryDic = new Dictionary<string, Models.Category>();
        Dictionary<Models.Category, List<int>> useridDic = new Dictionary<Models.Category, List<int>>();
        List<RelationData> relationDataList = new List<RelationData>();
        Models.ClusterTable clusterTable;
        bool viewCategoryCount = false;

        public bool ViewCategoryCount
        {
            get { return viewCategoryCount; }
            set {
                if (viewCategoryCount != value)
                {
                    viewCategoryCount = value;
                    foreach (var item in nodeDic)
                    {
                       
                        item.Value.Node.NodeName = item.Key.GetName(viewCategoryCount);
                    }
                }
            }
        }


        public void SetData(Models.ClusterTable clusterTable)
        {
            this.clusterTable = clusterTable;
            graphManage.Clear();
            nodeDic.Clear();
            categoryDic.Clear();
            useridDic.Clear();
            linkList.Clear();
            relationDataList.Clear();
            isNewGraph = true;

            List<int> userList = new List<int>();
            foreach (var item in clusterTable.Categories)
            {                
                var node = graphManage.CreateNode(item.GetName(viewCategoryCount), new Point(), Colors.Blue);
                node.Node.Size = new Size(20, 20);
                node.CanRemove = false;
                node.Tag = item;
                node.Node.NodeSubText = "("+item.GetCount().ToString() + ")";
                node.Node.SubLabelVisibility = subCountTextVisibility;
                node.NodeMouseDown += new MouseButtonEventHandler(node_NodeMouseDown);
                categoryDic.Add(item.GetName(), item);
                nodeDic.Add(item, node);
                useridDic.Add(item, new List<int>());
                foreach (var item2 in item.Layer)
                {
                    foreach (var item3 in item2.Value.Comunities)
                    {
                        if (item3.Deleted == false && item3.UserIds != null)
                        {
                            useridDic[item].AddRange(item3.UserIds);
                        }
                    }
                }
                useridDic[item] = useridDic[item].Distinct().ToList();
                userList.AddRange(useridDic[item]);
            }
            userList = userList.Distinct().ToList();

            foreach (var item in clusterTable.Categories)
            {
                foreach (var item2 in clusterTable.Categories.SkipWhile(n => n != item).Where(n => n != item))
                {
                    var link = graphManage.CreateLink(nodeDic[item].Node, nodeDic[item2].Node);
                    link.Visibility = Visibility.Collapsed;
                    link.SortKey = MyWpfLib.Graph.Sort.Jaccard(useridDic[item].Intersect(useridDic[item2]).Count(), useridDic[item].Count, useridDic[item2].Count);
                    link.Tag = MyWpfLib.Graph.Sort.信頼度比(useridDic[item].Intersect(useridDic[item2]).Count(), useridDic[item].Count, useridDic[item2].Count, userList.Count);
                    //  link.SortKey = MyWpfLib.Graph.Sort.MutualInformation(useridDic[item].Intersect(useridDic[item2]).Count(), useridDic[item].Count, useridDic[item2].Count,userList.Count);
                    link.TextValue = link.SortKey.ToString("F3");
                    link.TextVisibility = linkTextVisibility;
                    linkList.Add(link);

                    relationDataList.Add(new RelationData()
                    {
                        NameA = nodeDic[item].Node.NodeName,
                        NameB = nodeDic[item2].Node.NodeName,
                        Jaccard = link.SortKey,
                        信頼度比 = MyWpfLib.Graph.Sort.信頼度比(useridDic[item].Intersect(useridDic[item2]).Count(), useridDic[item].Count, useridDic[item2].Count, userList.Count),
                        Parent = this
                    });
                }
            }
            allCount = userList.Count;
            linkList = linkList.OrderByDescending(n => n.SortKey).ToList();
            graphManage.Update();
        }
        int allCount = 0;
        void node_NodeMouseDown(object sender, MouseButtonEventArgs e)
        {
            var nodeC = (MyWpfLib.Graph.NodeControl)sender;
            var category = nodeC.Tag as ClusterViewForSliverlinght.Models.Category;
            List<RelationData> list = new List<RelationData>();
            foreach (var item in clusterTable.Categories.Where(n => n != category))
            {
                list.Add(new RelationData()
                {
                    NameA = category.GetName(),
                    NameB = nodeDic[item].Node.NodeName,
                    Jaccard = MyWpfLib.Graph.Sort.Jaccard(useridDic[category].Intersect(useridDic[item]).Count(), useridDic[item].Count, useridDic[category].Count),
                    信頼度比 = MyWpfLib.Graph.Sort.信頼度比(useridDic[category].Intersect(useridDic[item]).Count(), useridDic[item].Count, useridDic[category].Count,allCount),
                    Parent = this
                });
        
            }
            list = list.OrderByDescending(n => n.Jaccard).ToList();
            ChangeDataGrid(this, new MyLib.Event.Args<List<RelationData>>(list));

        }

        public event EventHandler<MyLib.Event.Args<List<RelationData>>> ChangeDataGrid;

        int linkNum = 0;
        public void ChangeLinkNum(int num)
        {
            int c = 1;
            foreach (var item in linkList)
            {
                if (c <= num)
                {
                    if ((float)item.Tag >= min信頼度比)
                    {
                        item.Visibility = Visibility.Visible;
                        c++;
                    }
                    else
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    item.Visibility = Visibility.Collapsed;
                }

            }
            linkNum = num;
            graphManage.Update();
        }

        double min信頼度比 = 1.15;

        public double Min信頼度比
        {
            get { return min信頼度比; }
            set
            {
                if (min信頼度比 != value)
                {
                    min信頼度比 = value;
                    ChangeLinkNum(linkNum);
                    foreach (var item in relationDataList)
                    {
                        item.ChangeColor();
                    }
                }

            }
        }



        public class CategoryLink
        {
            public Models.Category Category1 { get; set; }
            public Models.Category Category2 { get; set; }

            public double SortKey { get; set; }
        }
        MyWpfLib.Graph.Spring spring = new MyWpfLib.Graph.Spring();

        bool isNewGraph = true;
        public void Spring()
        {
            if (spring.IsBusy == false)
            {
                spring.ClientArea = new MyWpfLib.Draw.Area(new Size() { Height = canvas.ActualHeight, Width = canvas.ActualWidth });

                spring.Clear();
                foreach (var item in nodeDic)
                {
                    spring.AddNode(item.Value.Node);
                }
                if (isNewGraph == false)
                {
                    spring.AutoParameter();
                }
                spring.Run();
                isNewGraph = false;
            }
        }

        bool autoMode = false;
        double sleepTime = 1;
        System.Threading.CancellationTokenSource cancellationTokenSource = new System.Threading.CancellationTokenSource();
        public bool AutoRun(Slider slider, Slider timeSlider, System.Threading.Tasks.TaskScheduler scheduler)
        {
            if (autoMode == false)
            {
                autoMode = true;
                cancellationTokenSource = new System.Threading.CancellationTokenSource();
                System.Threading.CancellationToken cancellationToken = cancellationTokenSource.Token;

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        while (autoMode)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                cancellationTokenSource.Dispose();
                                break;
                            }
                            System.Threading.Tasks.Task.Factory.StartNew(() =>
                                {
                                    if (slider.Maximum == slider.Value)
                                    {
                                        slider.Value = slider.Minimum;
                                    }
                                    else
                                    {
                                        slider.Value = slider.Value + 1;
                                    }
                                    //       ChangeLinkNum((int)slider.Value);
                                    sleepTime = timeSlider.Value;
                                }, cancellationToken, System.Threading.Tasks.TaskCreationOptions.AttachedToParent, scheduler);

                            System.Threading.Thread.Sleep((int)(sleepTime * 1000));
                        }
                    }
                );
                return true;
            }
            else
            {
                autoMode = false;
                cancellationTokenSource.Cancel();
                return false;
            }
        }

        Visibility linkTextVisibility = Visibility.Collapsed;
        public void ChangeLinkTextVisibility()
        {
            if (linkTextVisibility == Visibility.Collapsed)
            {
                linkTextVisibility = Visibility.Visible;
            }
            else
            {
                linkTextVisibility = Visibility.Collapsed;
            }
            foreach (var item in graphManage.Links)
            {
                item.TextVisibility = linkTextVisibility;
            }
        }

        Visibility subCountTextVisibility = Visibility.Collapsed;
        internal void ChangeNodeCountTextVisibility()
        {
            if (subCountTextVisibility == Visibility.Collapsed)
            {
                subCountTextVisibility = Visibility.Visible;
            }
            else
            {
                subCountTextVisibility = Visibility.Collapsed;
            }
            foreach (var item in nodeDic)
            {
                item.Value.Node.SubLabelVisibility = subCountTextVisibility;
            }
        }

        internal System.Collections.IEnumerable GetRelationData()
        {
            return relationDataList;
        }


        public void NodeClick()
        {

        }

        public class RelationData : INotifyPropertyChanged
        {
            public string NameA { get; set; }
            public string NameB { get; set; }
            public double Jaccard { get; set; }
            public double 信頼度比 { get; set; }
            public Graph Parent { get; set; }
            public void ChangeColor()
            {
                NotifyPropertyChanged("信頼度比Color");
            }
            public bool 信頼度比Color
            {
                get
                {
                    if (Parent != null)
                    {
                        if (Parent.Min信頼度比 > this.信頼度比)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return false;

                }
            }
            //   public double 信頼度比B { get; set; }

            #region INotifyPropertyChanged メンバー

            public event PropertyChangedEventHandler PropertyChanged = delegate { };
            protected void NotifyPropertyChanged(string propertyName)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }


    }
}
