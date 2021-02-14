using ResTB.DB;
using ResTB.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ResTB.Calculation
{
    /// <summary>
    /// Wrapper for the calculation kernel
    /// </summary>
    public class Calc
    {
        static Dictionary<int, IntensityDegree> Ik05Map = new Dictionary<int, IntensityDegree>()    //TODO: change name from 5 to "often"
                {
                    { 9, IntensityDegree.alta },
                    { 8, IntensityDegree.media  },
                    { 7, IntensityDegree.zero  },
                    { 6, IntensityDegree.media },
                    { 5, IntensityDegree.baja },
                    { 4, IntensityDegree.zero },
                    { 3, IntensityDegree.baja },
                    { 2, IntensityDegree.zero },
                    { 1, IntensityDegree.zero },
                };

        static Dictionary<int, IntensityDegree> Ik20Map = new Dictionary<int, IntensityDegree>()    //TODO: change name from 20 to "sometimes"
                {
                    { 9, IntensityDegree.alta },
                    { 8, IntensityDegree.alta  },
                    { 7, IntensityDegree.media  },
                    { 6, IntensityDegree.media },
                    { 5, IntensityDegree.media },
                    { 4, IntensityDegree.baja },
                    { 3, IntensityDegree.baja },
                    { 2, IntensityDegree.baja },
                    { 1, IntensityDegree.zero },
                };

        static Dictionary<int, IntensityDegree> Ik50Map = new Dictionary<int, IntensityDegree>()    //TODO: change name from 50 to "rare"
                {
                    { 9, IntensityDegree.alta },
                    { 8, IntensityDegree.alta  },
                    { 7, IntensityDegree.alta  },
                    { 6, IntensityDegree.media },
                    { 5, IntensityDegree.media },
                    { 4, IntensityDegree.media },
                    { 3, IntensityDegree.baja },
                    { 2, IntensityDegree.baja },
                    { 1, IntensityDegree.baja },
                };

        private static List<IKClasses> IkClasses { get; set; } = new List<IKClasses>();

        private Project CurrentProject { get; set; }

        /// <summary>
        /// create a calc instance for a given project id
        /// </summary>
        /// <param name="projectId">project id</param>
        public Calc(int projectId)
        {
            if (projectId < 1)
                throw new ArgumentOutOfRangeException(nameof(projectId));

            Project project = new Project() { Id = projectId };
            this.CurrentProject = project;

            using (ResTBContext db = new ResTBContext())
            {
                IkClasses = db.IntensitaetsKlassen.AsNoTracking().OrderBy(i => i.Value).ToList();
                if (IkClasses.Count != 3)
                    throw new InvalidOperationException($"GenerateIntensityMaps: IKClasses count = {IkClasses.Count}. Expected: 3");
            }
        }

        /// <summary>
        /// not in use
        /// </summary>
        /// <param name="project"></param>
        public Calc(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project), "Kernel ctor");

            this.CurrentProject = project;

            using (ResTBContext db = new ResTBContext())
            {
                IkClasses = db.IntensitaetsKlassen.AsNoTracking().OrderBy(i => i.Value).ToList();
                if (IkClasses.Count != 3)
                    throw new InvalidOperationException($"GenerateIntensityMaps: IKClasses count = {IkClasses.Count}. Expected: 3");
            }

        }

        #region MappedObjects
        /// <summary>
        /// not in use
        /// </summary>
        public List<int> GetAllMappedObjects()
        {
            var project = CurrentProject;
            IList<int> dpListFiltered = null;
            using (ResTBContext db = new ResTBContext())
            {
                string query =
                    $"select mo.* " +
                    $"from \"MappedObject\" mo, \"Perimeter\" p " +
                    $"where mo.\"Project_Id\" = {project.Id} " +
                    $"and mo.\"Project_Id\" = p.project_fk " +
                    $"and ( " +
                    $"ST_Intersects(p.geometry, mo.point) or " +
                    $"ST_Intersects(p.geometry, mo.line ) or " +
                    $"ST_Intersects(p.geometry, mo.polygon ) " +
                    $") ";

                dpListFiltered = db.MappedObjects.SqlQuery(query).Select(m => m.ID).ToList();

                //dpList = db.MappedObjects
                //        .Include(m => m.Project)
                //        .Where(m => m.Project.Id == project.Id)
                //        .ToList();

                //all DamagePotentials having geometry in project.geometry, including crossing project perimeter
            }
            //var dpListFiltered = dpList;//.Where(m => m.Project.geometry.Intersects(m.geometry));

            return dpListFiltered.ToList();
        }

        #endregion //MappedObjects

        /// <summary>
        /// Delete all damage extents of project in the database
        /// </summary>
        /// <param name="projectId"></param>
        private void DeleteDamageExtentsFromDB()
        {
            int projectId = CurrentProject.Id;

            if (projectId < 1)
                return;

            string query =
                $"delete " +
                $"from \"DamageExtent\" " +
                $"where \"DamageExtent\".\"MappedObjectId\" in " +
                $"(select damageextent.\"MappedObjectId\" " +
                $"from \"DamageExtent\" as damageextent, \"Intensity\" as intensity " +
                $"where intensity.\"Project_Id\" = {projectId} " +
                $"and damageextent.\"IntensityId\" = intensity.\"ID\") ";

            using (ResTBContext db = new ResTBContext())
            {
                var result = db.Database.ExecuteSqlCommand(query);
            }

        }

        /// <summary>
        /// STEP 1
        /// </summary>
        public void CreateIntensityMaps()
        {
            this.DeleteDamageExtentsFromDB(); //Cleanup

            this.GenerateAllIntensityMaps();  //Intensity Maps Generator


        }

        //STEP 2
        //public string RunCalculation(bool onlySummary = false)
        //{
        //    string returnvalue = string.Empty;
        //    //string fileName = @"C:\VS2019\ResTBDesktop\Kernel\bin\Debug\Kernel.exe";
        //    string fileName = @"Kernel\ResTBKernel.exe";
        //    string args = $" -d true -p {CurrentProject.Id} ";
        //    if (onlySummary)
        //        args += " -s true";
        //    args += $" -c {Thread.CurrentThread.CurrentCulture}";

        //    ProcessStartInfo info = new ProcessStartInfo(fileName);
        //    info.UseShellExecute = false;          //
        //    info.Arguments = args;
        //    info.RedirectStandardInput = true;     //
        //    info.RedirectStandardOutput = true;    //
        //    info.CreateNoWindow = true;            //

        //    using (Process process = Process.Start(info))
        //    {
        //        StreamReader sr = process.StandardOutput;
        //        returnvalue = sr.ReadToEnd();
        //    }

        //    return returnvalue;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onlySummary">true: only summary is computed; false: everything is recalculated </param>
        /// <param name="details">detailed result with calculation formulas</param>
        /// <param name="isOnline">Where is the DB located?</param>
        /// <returns></returns>
        public async Task<string> RunCalculationAsync(bool onlySummary = false, bool details = false, bool isOnline = true)
        {
            string fileName = @"Kernel\ResTBKernel.exe";
            string args = $" -d {details} -p {CurrentProject.Id} -o {isOnline}";
            if (onlySummary)
                args += " -s true";
            args += $" -c {Thread.CurrentThread.CurrentCulture}";

            int timeout = 1000 * 60 * 2;// ;        //ms

            var result = await ProcessAsyncHelper.ExecuteShellCommand(fileName, args, timeout);

            if (result.Completed)
                return result.Output;
            else
                return "Error: Task not completed";

        }


        #region IntensityMaps

        /// <summary>
        /// Re-Generate all intensity maps for current project
        /// </summary>
        private void GenerateAllIntensityMaps()
        {
            List<HazardMap> allHazardMaps = new List<HazardMap>();

            using (ResTBContext db = new ResTBContext())
            {
                //remove ALL intensities
                var intensitiesToDelete = db.Intensities
                    .Include(i => i.Project)
                    .Where(i => i.Project.Id == CurrentProject.Id);
                if (intensitiesToDelete.Any())
                {
                    db.Intensities.RemoveRange(intensitiesToDelete);
                    db.SaveChanges();
                }

                allHazardMaps = db.HazardMaps
                    .Include(m => m.NatHazard)
                    .Where(m => m.Project.Id == CurrentProject.Id).ToList();
            }

            List<NatHazard> allNatHazards = allHazardMaps
                .Select(h => h.NatHazard)
                .Distinct()
                .OrderBy(n => n.ID).ToList();

            foreach (NatHazard natHazard in allNatHazards)              //foreach NatHazard
            {
                List<bool> beforeActions = allHazardMaps
                    .Where(i => i.NatHazard.ID == natHazard.ID)
                    .Select(i => i.BeforeAction)
                    .Distinct()
                    .OrderByDescending(a => a).ToList();

                foreach (bool beforeMeasure in beforeActions)           //foreach BeforeAction/AfterAction
                {
                    GenerateIntensityMaps(beforeMeasure, natHazard.ID);
                }
            }
        }

        /// <summary>
        /// Generate intensity maps for current project, beforeAction & natHazard
        /// </summary>
        /// <param name="beforeAction"></param>
        /// <param name="natHazardId"></param>
        private void GenerateIntensityMaps(bool beforeAction, int natHazardId)
        {
            int projectId = CurrentProject.Id;

            List<HazardMap> hazardMaps = new List<HazardMap>();

            using (ResTBContext db = new ResTBContext())
            {
                ////remove all intensities for that hazardmap
                //var intensitiesToDelete = db.Intensities
                //    .Include(i=>i.Project)
                //    .Where(i => i.Project.Id == projectId && 
                //                i.BeforeAction == beforeAction && 
                //                i.NatHazard.ID == natHazardId);
                //if (intensitiesToDelete.Any())
                //{
                //    db.Intensities.RemoveRange(intensitiesToDelete);
                //    db.SaveChanges();
                //}

                //Load hazard maps
                hazardMaps = db.HazardMaps
                    .Include(m => m.Project.Intesities)
                    .Where(m => m.Project.Id == projectId && m.BeforeAction == beforeAction && m.NatHazard.ID == natHazardId)
                    .ToList();

                if (!hazardMaps.Any())
                {
                    return;
                }

                foreach (HazardMap hazardMap in hazardMaps)
                {
                    int hazardIndex = hazardMap.Index;

                    Dictionary<IKClasses, IntensityDegree> IkIntensityMap = new Dictionary<IKClasses, IntensityDegree>();

                    IkIntensityMap.Add(IkClasses[0], Ik05Map[hazardIndex]);
                    IkIntensityMap.Add(IkClasses[1], Ik20Map[hazardIndex]);
                    IkIntensityMap.Add(IkClasses[2], Ik50Map[hazardIndex]);

                    foreach (KeyValuePair<IKClasses, IntensityDegree> kvp in IkIntensityMap)
                    {
                        if (kvp.Value == IntensityDegree.zero)  //skip zero intensities
                            continue;

                        string query =
                            $"insert into \"Intensity\" (\"BeforeAction\" , \"IntensityDegree\" , \"NatHazard_ID\" , \"Project_Id\" ,\"IKClasses_ID\", geometry ) " +
                            $"select hm.\"BeforeAction\" , {(int)kvp.Value}, hm.\"NatHazard_ID\" , hm.\"Project_Id\" , {kvp.Key.ID} , ST_Multi(hm.geometry) " +
                            $"from \"HazardMap\" hm " +
                            $"where hm.\"Project_Id\" = {projectId} and hm.\"BeforeAction\" = {beforeAction} and hm.\"NatHazard_ID\" = {natHazardId} and hm.\"Index\" = {hazardIndex} ";
                        var result = db.Database.ExecuteSqlCommand(query);


                    }
                }

                //Merging
                foreach (var ikClass in IkClasses)
                {
                    foreach (int intDegree in new List<int>() { 0, 1, 2 })
                    {
                        var intensities = db.Intensities.Where(i => i.IKClasses.ID == ikClass.ID &&
                                             (int)i.IntensityDegree == intDegree &&
                                             i.BeforeAction == beforeAction &&
                                             i.Project.Id == projectId &&
                                             i.NatHazard.ID == natHazardId);

                        if (intensities.Any() && intensities.Count() > 1)
                        {
                            string ids = string.Join(", ", intensities.Select(i => i.ID.ToString()));
                            string query =
                                $"insert into \"Intensity\" (\"BeforeAction\" , \"IntensityDegree\" , \"NatHazard_ID\" , \"Project_Id\" ,\"IKClasses_ID\", geometry ) " +
                                $"select \"BeforeAction\" , \"IntensityDegree\"  , \"NatHazard_ID\", \"Project_Id\", \"IKClasses_ID\" , ST_Multi(ST_Union(geometry)) " +
                                $"from \"Intensity\" i " +
                                $"where i.\"ID\" in ({ids}) " +
                                $"group by \"BeforeAction\", \"NatHazard_ID\", \"Project_Id\", \"IntensityDegree\", \"IKClasses_ID\" ";
                            var result = db.Database.ExecuteSqlCommand(query);

                            //delete the origins
                            query = $"delete from \"Intensity\" i where i.\"ID\" in ({ids}) ";
                            result = db.Database.ExecuteSqlCommand(query);

                        }
                    }
                }

            } //dbContext
        }

        #endregion //IntensityMaps



    }
}
