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

        public void Create(System.IO.StreamReader sr)
        {
            foreach (var item in TSVFile.ReadLines(sr))
            {
                User u = new User()
                {
                    Id = item.GetIntValue("User_Id")
                };
                //Community_Id
                foreach (var d in item.GetValue("属性",string.Empty).Split(','))
                {
                    var dd = d.Split(':');
                    if (dd.Length > 1)
                    {
                        u.AddAttribute(dd[0], dd[1]);
                    }
                }
                userList.Add(u);
            }
        }

    }

     public class UserAttributeData
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
             get {
                 return (double)this.Count*100 / (double)Parent.Count;
             }
         }
         public UserAttributeGroup Parent { get; set; }
     }

     public class UserAttributeGroup
     {
         public string GroupName{get;set;}

         int count = 0;
         public int Count
         {
             get { return count; }
             set { count = value; }
         }
         Dictionary<string, UserAttributeData> dic = new Dictionary<string, UserAttributeData>();
         public IEnumerable<UserAttributeData> Data
         {
             get {
                 foreach (var item in dic.Values.Where(n=>n.AttributeName != "不明").OrderByDescending(n=>n.Count).ThenBy(n=>n.AttributeName))
                 {
                     yield return item;
                 }
                 if(dic.Values.Where(n=>n.AttributeName == "不明").Any())
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
     }

}
