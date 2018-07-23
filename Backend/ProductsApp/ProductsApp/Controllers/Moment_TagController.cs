﻿using System;
using ProductsApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using System.Data;
//using System.Data.OracleClient;
using Oracle.ManagedDataAccess.Client;

namespace ProductsApp.Controllers
{
    public class Moment_TagController : ApiController
    {
        DBAccess Access = new DBAccess();
        /// <summary>
        /// 通过Tag的内容得到与之相关的所有动态
        /// </summary>
        /// <param name="Page"></param>
        /// <param name="PageSize"></param>
        /// <param name="content"></param>
        /// <returns></returns>
       [HttpGet]
        public Tuple<List<Moment>,int,bool> Followers(int Page, int PageSize, string TagContent , string Email)
        {
            bool FollowState = false;
            string sql = Access.Select("ID", "USERS", "EMAIL='" + Email + "'").ToString();
            OracleDataReader r = Access.GetDataReader(sql);
            r.Read();
            string UserId = r["ID"].ToString();
            if (Access.GetRecordCount(Access.Select("*", "FOLLOW_TAG", "USER_ID='" + UserId + "' and TAG='" + TagContent + "'")) == 0)
            {
                FollowState = false;  //用户未关注
            }
            else
                FollowState = true;   //用户已关注

            //动态ID的数据集
            DataSet MIDSet = new DataSet();
            //动态的数据集
            //DataSet MSet = new DataSet();
            string select = Access.Select("MOMENT_ID", "MOMENT_TAG", "TAG = '" + TagContent + "'");
            MIDSet = Access.GetDataSet(select, "MOMENT_TAG", PageSize, Page);
            //动态的数组
            List<Moment> moments = new List<Moment>();
            foreach (DataRow row in MIDSet.Tables[0].Rows)
            {
                select = Access.Select("*", "MOMENT", "ID = '" + row[0] + "'");
                OracleDataReader rd = Access.GetDataReader(select);
                while(rd.Read())
                {
                    string id = rd["ID"].ToString();
                    string sender_id = rd["SENDER_ID"].ToString();
                    string content = rd["CONTENT"].ToString();
                    int likes = int.Parse(rd["LIKE_NUM"].ToString());
                    int forwards = int.Parse(rd["FORWARD_NUM"].ToString());
                    int collects = int.Parse(rd["COLLECT_NUM"].ToString());
                    int comments = int.Parse(rd["COMMENT_NUM"].ToString());
                    string time = rd["TIME"].ToString().Replace('T', ' ');
                    moments.Add(new Moment(id, sender_id, content, likes, forwards, collects, comments, time));
                }
            }
            select = Access.Select("*", "FOLLOW_TAG", "TAG = '" + TagContent + "'");
            int Flowers = Access.GetRecordCount(select);

            Tuple<List<Moment>, int, bool> result = new Tuple<List<Moment>, int, bool>(null, 0, false);
            if (moments.Count != 0)
            {
                result = new Tuple<List<Moment>, int, bool>(moments, Flowers, FollowState);
            }
            else if (moments.Count == 0)
                result = new Tuple<List<Moment>, int, bool>(null, 0, FollowState);

            return result;
        }
    }
}
