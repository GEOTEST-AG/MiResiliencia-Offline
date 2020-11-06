using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using ResTB.DB.Models;
using System.Data;
using System.Linq;

namespace ResTB.DB.Tests
{
    [TestClass]
    public class DBStartEditStop
    {
        //[TestMethod]
        //public void StartDB()
        //{
        //    ResTB.DB.DBUtils dbutils = ResTB.DB.DBUtils.Instance;
        //    dbutils.StartLocalDB();

        //    try
        //    {

        //        NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["ResTBLocalDB"].ConnectionString);
        //        conn.Open();

        //        Assert.AreEqual(System.Data.ConnectionState.Open, conn.State);
        //    }
        //    catch (Exception e)
        //    {
        //        // localDB not yet startet
        //        Assert.Fail(e.Message);
        //    }
        //}


        //[TestMethod]
        //public void AddAndDeleteProject()
        //{
        //    try
        //    {

        //        NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["ResTBLocalDB"].ConnectionString);
        //        conn.Open();
        //    }
        //    catch (Exception e)
        //    {
        //        // localDB not yet startet
        //        ResTB.DB.DBUtils dbutils = ResTB.DB.DBUtils.Instance;
        //        dbutils.StartLocalDB();

        //    }

        //    ResTB.DB.ResTBContext db = new ResTB.DB.ResTBContext();

        //    Project p = new Project();
        //    p.Name = "Testprojekt1";
        //    p.Number = "1";
        //    p.Description = "Neues Projekt";
        //    db.Projects.Add(p);
        //    db.SaveChanges();

        //    Project p2 = db.Projects.Where(m => m.Name == "Testprojekt").First();
        //    Assert.IsNotNull(p2);

        //    db.Projects.Remove(p2);
        //    db.SaveChanges();
            
        //}

        //[TestMethod]
        //public void StopDB()
        //{
        //    ResTB.DB.DBUtils dbutils = ResTB.DB.DBUtils.Instance;
        //    dbutils.StopLocalDB();
        //}
    }
}
