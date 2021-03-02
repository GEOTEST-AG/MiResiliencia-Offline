using Fclp;
using RazorEngine;
using RazorEngine.Templating; // Dont forget to include this.
using ResTB_API.Controllers;
using ResTB_API.Models;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kernel
{
    public static class Globals
    {
        public static bool ISONLINE = false;
    }

    class Program
    {

        static void Main(string[] args)
        {
            ////////////////////////////////////////////////////
            string localData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!System.IO.Directory.Exists(localData + "\\ResTBDesktop"))
                System.IO.Directory.CreateDirectory(localData + "\\ResTBDesktop");
            //string scriptPath = localData + "\\ResTBDesktop\\Script";
            //if (!System.IO.Directory.Exists(scriptPath))
            //    System.IO.Directory.CreateDirectory(scriptPath);
            string htmlPath = localData + "\\ResTBDesktop\\result.html";


            //Console.WriteLine($"output html: {htmlPath}");
            ////////////////////////////////////////////////////

            int projectId = -1;
            bool showDetails = false;
            bool onlySummary = false;
            bool isOnline = false;

            string cultureString = "en";
            var p = new FluentCommandLineParser();

            p.Setup<int>('p', "project")
             .Callback(record => projectId = record)
             .WithDescription("Project ID (int)")
             .Required();

            p.Setup<bool>('d', "detail")
             .Callback(record => showDetails = record)
             .WithDescription("Show Details in Summary")
             .SetDefault(false);

            p.Setup<bool>('s', "summaryonly")
                .Callback(record => onlySummary = record)
                .WithDescription("Show Summary Only")
                .SetDefault(false);

            p.Setup<string>('c', "culture")
                .Callback(record => cultureString = record)
                .WithDescription("Language Setting")
                .SetDefault("es-HN");

            p.Setup<bool>('o', "online")
                .Callback(record => isOnline = record)
                .WithDescription("DB is online")
                .SetDefault(false);

            var parsedArgs = p.Parse(args);

            if (parsedArgs.HasErrors == true)
            {
                Console.WriteLine(parsedArgs.ErrorText);
                return;
            }

            Globals.ISONLINE = isOnline;    //set for Connection string

            CultureInfo culture;
            try
            {
                //CultureInfo culture = CultureInfo.CreateSpecificCulture(cultureString);
                culture = new CultureInfo(cultureString);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                //CultureInfo.DefaultThreadCurrentUICulture = culture;
                //CultureInfo.DefaultThreadCurrentCulture = culture;
            }
            catch (Exception)
            {
                Console.WriteLine($"ERROR: culture invalid. Provided value: {cultureString}");
                return;
            }

            Console.WriteLine("Kernel started...");
            Console.WriteLine($"\tProject Id = {projectId}, showDetails = {showDetails}, " +
                              $"summaryOnly = {onlySummary}, culture = {cultureString}, isOnline = {isOnline}");
            ////////////////////////////////////////////////////
            // CALCULATION

            if (!onlySummary)
            {
                try
                {
                    ResultWrapper.CreateDamageExtents(projectId);               //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    Console.WriteLine("\n\tCreate Damage Extents finished.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError while creating damage extents.\n\n" + ex.ToString());
                    return;
                }
            }

            ProjectResult projectResult = ResultWrapper.ComputeResult(projectId, showDetails);

            Console.WriteLine("\n\tCompute Project Result finished.");

            string fullFileName = @"Kernel/Views/Summary.cshtml";        //TODO: Copy CSHTML to output dir

            DynamicViewBag viewBag = new DynamicViewBag();
            viewBag.AddValue("attachCss", true);
            viewBag.AddValue("details", showDetails);
            viewBag.AddValue("print", true);

            var templateSource = new LoadedTemplateSource(File.ReadAllText(fullFileName), fullFileName);
            string result = "";

            var t = Task.Run(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                Console.WriteLine("Task thread ID: {0}", Thread.CurrentThread.ManagedThreadId);
                result =
                    Engine.Razor.RunCompile(templateSource, "templateKey", null, model: projectResult, viewBag: viewBag);   //RENDER HTML with RAZOR ENGINE

                Console.WriteLine($"\tTask: culture = {culture.TwoLetterISOLanguageName} / {culture.Name}");
            });
            t.Wait();

            File.WriteAllText(htmlPath, result);               //TODO: Save HTML to output dir

            Console.WriteLine("\nKernel finished.\n");

        }
    }
}
