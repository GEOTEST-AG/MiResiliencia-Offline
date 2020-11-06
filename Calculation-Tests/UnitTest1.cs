using System;
using ResTB.Calculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResTB.DB;
using ResTB.DB.Models;
using System.Diagnostics;
using System.IO;

namespace ResTB.Calculation.Tests
{
    [TestClass]
    public class KernelTest
    {
        [TestMethod]
        public void CalculationTest1()
        {
            //Project project;
            //using (ResTBContext db = new ResTBContext())
            //{
            //    //project = db.Projects.Find(235);
            //    project = db.Projects.Find(353);    //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< set project id
            //}

            var calc = new Calc(353);

            calc.CreateIntensityMaps();

            //string returnvalue = calc.RunCalculation();

            //string returnvalue = 
            calc.RunCalculationAsync().Wait();

            var blub = "Stop";
        }
    }
}
