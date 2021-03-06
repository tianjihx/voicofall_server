﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;

namespace voicofall_server.ResponsePages
{
    /// <summary>
    /// ResponseServerTime 的摘要说明
    /// </summary>
    public class ResponseServerTime : IHttpHandler
    {
        public static readonly string connStr1 = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/tickets.mdb");
        OleDbConnection conn;
        OleDbDataAdapter Adapter2;
        DataSet dataSet2;
        string strSQL;

        int bookyear;     //arr[6]
        int bookmonth;    //arr[7]
        int bookday;      //arr[8]
        int bookhour;     //arr[9]
        int bookmin;      //arr[10]
        int booksecond;   //arr[11]
        string party;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;
            int hour = DateTime.Now.Hour;
            int min = DateTime.Now.Minute;
            int second = DateTime.Now.Second;

            GetBookTimeFromDB(context);

            if (party == "no")
            {
                context.Response.Write("noparty");
                return;
            }

            context.Response.Write(year.ToString() + "&" +
                                   month.ToString() + "&" +
                                   day.ToString() + "&" +
                                   hour.ToString() + "&" +
                                   min.ToString() + "&" +
                                   second.ToString() + "&" +
                                   bookyear.ToString() + "&" +
                                   bookmonth.ToString() + "&" +
                                   bookday.ToString() + "&" +
                                   bookhour.ToString() + "&" +
                                   bookmin.ToString() + "&" +
                                   booksecond.ToString());
        }       

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void GetBookTimeFromDB(HttpContext context)
        {
            conn = new OleDbConnection(connStr1);
            try
            {
                conn.Open();
                strSQL = "select * from ticketsStateTable";
                Adapter2 = new OleDbDataAdapter(strSQL, conn);
                //加了这句话就不会出现：当传递具有已修改行的 DataRow 集合时，更新要求有效的 UpdateCommand。
                OleDbCommandBuilder sqlBulider2 = new OleDbCommandBuilder(Adapter2);
                dataSet2 = new DataSet();
                Adapter2.Fill(dataSet2, "ticketsStateTable");
                DataTable ticketsStateTable = dataSet2.Tables["ticketsStateTable"];

                //检验是否还有空余票
                ticketsStateTable.PrimaryKey = new DataColumn[] { ticketsStateTable.Columns["state"] };
                //读取下次订票时间
                DataRow tempRow = ticketsStateTable.Rows.Find("nextBookTime");
                string temp = (string)tempRow["scontent"];
                Regex reg;
                reg = new Regex(@"^\d{4}");
                bookyear = Convert.ToInt32(reg.Match(temp).Value);
                reg = new Regex(@"(?<=-)\d+(?=-)");
                bookmonth = Convert.ToInt32(reg.Match(temp).Value);
                reg = new Regex(@"(?<=-)\d+(?=\s)");
                bookday = Convert.ToInt32(reg.Match(temp).Value);
                reg = new Regex(@"\d+(?=:)");
                bookhour = Convert.ToInt32(reg.Match(temp).Value);
                reg = new Regex(@"\d+$");
                bookmin = Convert.ToInt32(reg.Match(temp).Value);
                booksecond = 0;

                party = ticketsStateTable.Rows.Find("party")["scontent"] as string;
                
                conn.Close();
            }
            catch (Exception ee)
            {
                context.Response.Write("state=no&wrongcode=3"); //初始化数据库出错！
            }
        }
    }
}