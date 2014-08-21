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
using System.Runtime.Serialization;
using System.Linq;
using System.ComponentModel;

namespace ClusterViewForSliverlinght.Models
{
    [DataContract]
    public class User
    {
        [DataMember]
        public int Id { get; set; }
        Dictionary<string, string> attributeDic = new Dictionary<string, string>();

        [DataMember]
        public Dictionary<string, string> AttributeDic
        {
            get { return attributeDic; }
            set { attributeDic = value; }
        }

        public void AddAttribute(string attributeName, string value)
        {
            if (attributeDic.ContainsKey(attributeName) == false)
            {
                attributeDic.Add(attributeName, value);
            }
        }

        public string GetAttribute(string attributeName)
        {
            if (attributeDic.ContainsKey(attributeName))
            {
                return attributeDic[attributeName];
            }
            else
            {
                return null;
            }
        }
        public IEnumerable<string> AttributeNameList
        {
            get { return attributeDic.Keys; }
        }
    }
    [DataContract]
    public class Users
    {
        List<User> userList = new List<User>();
        [DataMember]
        public List<User> UserList
        {
            get { return userList; }
            set { userList = value; }
        }


        Dictionary<int, User> userDic;

        public Dictionary<int, User> UserDic
        {
            get { return userDic; }
            set { userDic = value; }
        }
        List<string> attributeNameList;



        public void SetUpData()
        {
            if (userDic == null && attributeNameList == null)
            {
                userDic = new Dictionary<int, User>();
                List<string> list = new List<string>();
                foreach (var item in userList)
                {
                    if (userDic.ContainsKey(item.Id) == false)
                    {
                        userDic.Add(item.Id, item);
                    }
                    list.AddRange(item.AttributeNameList);
                }
                attributeNameList = list.Distinct().ToList();
            }
        }
        IEnumerable<string> errList;
        public void Create(System.IO.StreamReader sr)
        {
            foreach (var item in TSVFile.ReadLines(sr, out errList))
            {
                User u = new User()
                {
                    Id = item.GetIntValue("User_Id")
                };
                //Community_Id
                bool flag = false;
                foreach (var d in item.GetValue("属性", string.Empty).Split(','))
                {
                    var dd = d.Split(':');
                    if (dd.Length > 1)
                    {
                        u.AddAttribute(dd[0], dd[1]);
                    }
                    flag = true;
                }
                if(flag) userList.Add(u);
            }
        }

        public double GetRate(string attributeName, string value, IEnumerable<int> userIdList)
        {
            int count = 0;
            int all = 0;
            foreach (var item in userIdList)
            {
                if (userDic.ContainsKey(item))
                {
                    var v = userDic[item].GetAttribute(attributeName);
                    if (v == value)
                    {
                        count++;
                    }
                    all++;
                }
                
            }
            return (double)(count / (double)all);
        }

    }

    public class UserAttributeData:INotifyPropertyChanged
    {
        public string AttributeName { get; set; }
        int count = 0;
        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public double Rate
        {
            get
            {
                return (double)this.Count * 100 / (double)Parent.Count;
            }
        }
        public UserAttributeGroup Parent { get; set; }

        bool selected = false;

        public bool Selected
        {
            get { return selected; }
            set {
                if (selected != value)
                {
              
                    selected = value;
                    OnPropertyChanged("ButtonBrush");
                }
            
            }
        }

        public Brush ButtonBrush
        {
            get
            {
                if (selected)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }

            }
            set
            {
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

    public class UserAttributeGroup : INotifyPropertyChanged
    {
        public static string SortType = "名前順";

        public string GroupName { get; set; }

        int count = 0;
        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public void UnSelected()
        {
            foreach (var item in dic)
            {
                item.Value.Selected = false;
            }
        }

        bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    //        OnPropertyChanged("Visible");
                    OnPropertyChanged("Visibility");
                }
            }
        }

        public Visibility Visibility
        {
            get
            {
                if (visible) return Visibility.Visible;
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }



        Dictionary<string, UserAttributeData> dic = new Dictionary<string, UserAttributeData>();
        public IEnumerable<UserAttributeData> Data
        {
            get
            {
                if (SortType == "名前順")
                {
                    foreach (var item in dic.Values.Where(n => n.AttributeName != "不明").OrderBy(n => n.AttributeName))
                    {
                        yield return item;
                    }
                }
                else
                {
                    foreach (var item in dic.Values.Where(n => n.AttributeName != "不明").OrderByDescending(n => n.Count).ThenBy(n => n.AttributeName))
                    {
                        yield return item;
                    }
                }
                if (dic.Values.Where(n => n.AttributeName == "不明").Any())
                {
                    yield return dic.Values.Where(n => n.AttributeName == "不明").First();
                }
            }
        }
        public void Add(string attribute)
        {
            if (attribute.Length > 0)
            {
                count++;
                if (dic.ContainsKey(attribute))
                {
                    dic[attribute].Count++;
                }
                else
                {
                    dic.Add(attribute, new UserAttributeData() { AttributeName = attribute, Count = 1, Parent = this });
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

}
