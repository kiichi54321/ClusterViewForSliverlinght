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
using GalaSoft.MvvmLight.Command;
using System.Linq;

namespace ClusterViewForSliverlinght.Analyze
{
    public class ClusteringViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        public ClusteringViewModel()
        {
            clustering = new Clustering2();
            clustering.Progress = (n) =>
                {
                    MyLib.Task.Utility.UITask(() =>
                    this.Progress = (int)(n * 100));
                };
            clustering.Report = (n) =>
                {
                    MyLib.Task.Utility.UITask(() =>
                    this.ReportText += n);
                };
        }

        public Models.ClusterTable ClusterTable
        {
            get
            {
                return clustering.ClusterTable;
            }
            set
            {
                clustering.ClusterTable = value;
            }
        }

        Clustering2 clustering;

        public int TakeComunity
        {
            get { return clustering.TakeComunity; }
            set
            {
                clustering.TakeComunity = value;
                RaisePropertyChanged("TakeComunity");
            }
        }
        Random random = new Random();
        public int TryCount
        {
            get { return clustering.TryCount; }
            set { clustering.TryCount = value; RaisePropertyChanged("TryCount"); }
        }

        public int WideCount
        {
            get { return clustering.WideCount; }
            set { clustering.WideCount = value; RaisePropertyChanged("WideCount"); }
        }

        public System.Collections.ObjectModel.ObservableCollection<ClusteringResult> Results
        {
            get { return clustering.Result; }
            set { clustering.Result = value; }
        }

        public int MinCategoryComunityCount
        {
            get
            {
                return clustering.MinCategoryComunityCount;
            }
            set
            {
                clustering.MinCategoryComunityCount = value;
                RaisePropertyChanged("MinCategoryComunityCount");
            }
        }

        public bool IsBusy
        {
            get { return clustering.IsBusy; }

        }

        int progress = 0;
        public int Progress
        {
            get { return progress; }
            set { progress = value; RaisePropertyChanged("Progress"); }
        }
        string reportText = string.Empty;
        public string ReportText { get { return reportText; } set { reportText = value; RaisePropertyChanged("ReportText"); } }

        ClusteringResult seletectedResult = null;
        public ClusteringResult SeletectedResult
        {
            get { return seletectedResult; }
            set
            {
                seletectedResult = value; 
                RaisePropertyChanged("SeletectedResult");
                if (seletectedResult != null)
                {
                    seletectedResult.Update(this.ClusterTable);
                }
            }
        }

        RelayCommand<ClusteringResult> changeClusteringResult;
        public RelayCommand<ClusteringResult> ChangeClusteringResult
        {
            get
            {
                if (changeClusteringResult == null)
                {
                    changeClusteringResult = new RelayCommand<ClusteringResult>((n) =>
                        {
                            n.Update(ClusterTable);
                        });
                }
                return changeClusteringResult;
            }
        }

        RelayCommand runClustering;
        public RelayCommand RunClustering
        {
            get
            {
                if (runClustering == null)
                {
                    runClustering = new RelayCommand(() =>
                    {
                        Run();
                    });
                }
                return runClustering;
            }
        }

        public void Run()
        {
            if (clustering.IsBusy == false)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    clustering.Run();
                }).ContinueWith((n) =>
                     MyLib.Task.Utility.UITask(() => SeletectedResult = this.Results.FirstOrDefault())
                    );
            }
            else
            {
                ReportText += "クラスタリング中です。";
            }
        }

        RelayCommand resultClear;
        public RelayCommand ResultClear
        {
            get
            {
                if (resultClear == null)
                {
                    resultClear = new RelayCommand(() => { if(clustering.IsBusy==false) this.Results.Clear(); });
                }
                return resultClear;
            }
        }
    }
}
