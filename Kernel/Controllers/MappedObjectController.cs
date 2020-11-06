using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NHibernate;
using NHibernate.Spatial.Criterion.Lambda;
using ResTB_API.Helpers;
using ResTB_API.Models.Database.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ResTB_API.Controllers
{
    public class MappedObjectController
    {
        public static ISession ActiveSession
        {
            get
            {
                return DBManager.ActiveSession;
            }
        }

        public static Tuple<double, string> computeResilienceFactor(List<ResilienceValues> allResilienceList, Intensity intensity)
        {
            NatHazard natHazard = intensity.NatHazard;
            bool beforeAction = intensity.BeforeAction; //--> to distinguish the before/after resilience

            int _resilienceHazardID;
            if (natHazard?.ID == 1) // Sequia
            {
                _resilienceHazardID = 1;
            }
            else //other nathazards
            {
                _resilienceHazardID = 2;
            }

            // use only resilience values of given nathazard
            List<ResilienceValues> _resilienceList = allResilienceList
                .Where(r => r.ResilienceWeight.NatHazard.ID == _resilienceHazardID && r.ResilienceWeight.BeforeAction == beforeAction).ToList();

            List<string> _logString1 = new List<string>();
            List<string> _logString2 = new List<string>();
            List<string> _logIDStrings = new List<string>();

            //if list is empty: factor = 0
            if (_resilienceList == null || !_resilienceList.Any())
            {
                return new Tuple<double, string>(0.0d, "no resiliece values found");
            }

            double _sumWeight = 0;
            double _sumValueWeight = 0;

            foreach (ResilienceValues item in _resilienceList)
            {
                _sumValueWeight += item.Value * item.Weight;
                var string1 = $" ({item.Value} * {item.Weight}) ";
                _logString1.Add(string1);

                _sumWeight += item.Weight;
                var string2 = $" {item.Weight} ";
                _logString2.Add(string2);

                _logIDStrings.Add(item.ResilienceWeight.ResilienceFactor_ID + $" V{item.Value:F2} W{item.Weight:F1}");
            }

            string _logResilienceFactor = $"ResilienceFactor (c{_resilienceHazardID}) = " +
                String.Join("+", _logString1) + " / (" + String.Join("+", _logString2) + ")" +
                ";\n         ResilienceValues: " + String.Join("; \n", _logIDStrings); ;

            if (_sumWeight == 0) //avoid division by 0
            {
                return new Tuple<double, string>(0.0d, _logResilienceFactor);
            }
            else
            {
                double _resilienceFactor = _sumValueWeight / _sumWeight;

                var _result = new Tuple<double, string>(_resilienceFactor, _logResilienceFactor);
                return _result;
            }
        }

        /// <summary>
        /// Get damagepotentials crossing the projects perimeter
        /// </summary>
        /// <param name="projectID"></param>
        /// <returns></returns>
        //[Route("DamagePotential/{projectID}/Crossing")]
        public IEnumerable<int> GetCrossingsByProjectID(int projectID)
        {
            try
            {
                ISQLQuery query = ActiveSession.CreateSQLQuery(
                    $"select \"ID\" " +
                    $"from \"MappedObject\" as mapobj " +
                    $"where ST_Intersects((select geometry from \"Project\" where \"Id\" = {projectID}), mapobj.geometry) and " +
                    $"  NOT ST_Contains((select geometry from \"Project\" where \"Id\" = {projectID}), mapObj.Geometry) and " +
                    $"  \"Project_Id\" = {projectID}; ");

                var results = query.List<int>();

                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// all DamagePotentials having geometry in project.geometry, including crossing project perimeter
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public List<MappedObject> getAllDamagePotentials(int projectId)
        {
            var project = DBManager.ActiveSession.Load<Project>(projectId);

            IList<MappedObject> dpList = ActiveSession.QueryOver<MappedObject>()
               .Where(m => m.Project.Id == projectId)
               .List<MappedObject>();

            var perimeter = DBManager.ActiveSession.QueryOver<Perimeter>().Where(p => p.Project.Id == projectId).List<Perimeter>().FirstOrDefault();
            if (perimeter == null || perimeter.geometry == null)
                throw new NullReferenceException(nameof(perimeter) + " " + nameof(perimeter.geometry));

            //all DamagePotentials having geometry in project.geometry, including crossing project perimeter
            List<MappedObject> dpListFiltered = new List<MappedObject>();
            dpListFiltered.AddRange(dpList.Where(m => m.point != null && perimeter.geometry.Intersects(m.point)));
            dpListFiltered.AddRange(dpList.Where(m => m.line != null && perimeter.geometry.Intersects(m.line)));
            dpListFiltered.AddRange(dpList.Where(m => m.polygon != null && perimeter.geometry.Intersects(m.polygon)));


            return dpListFiltered.ToList();
        }

        /// <summary>
        /// DamagePotential within intensity.geometry
        /// </summary>
        /// <param name="intensity"></param>
        /// <param name="dpList"></param>
        /// <returns></returns>
        public List<MappedObject> getDamagePotentialsWithin(Intensity intensity, IList<MappedObject> dpList)
        {
            var project = DBManager.ActiveSession.Load<Project>(intensity.Project.Id);

            var dpListFiltered = new List<MappedObject>();
            dpListFiltered.AddRange(dpList.Where(m => m.point != null && m.point.Within(intensity.geometry)));
            dpListFiltered.AddRange(dpList.Where(m => m.line != null && m.line.Within(intensity.geometry)));
            dpListFiltered.AddRange(dpList.Where(m => m.polygon != null && m.polygon.Within(intensity.geometry)));

            List<MappedObject> withinList = new List<MappedObject>();

            foreach (var damagePotential in dpListFiltered)
            {
                var copyDamagePotential = (MappedObject)damagePotential.Clone();

                copyDamagePotential.IsClipped = false;
                copyDamagePotential.Intensity = (Intensity)intensity.Clone(); //remember the intensity the mappedObject is in

                withinList.Add(copyDamagePotential);
            }

            return withinList;
        }

        /// <summary>
        /// like getDamagePotentialsWithin, but with spatial query on db
        /// </summary>
        /// <param name="intensity"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public List<MappedObject> getDamagePotentialsWithin2(Intensity intensity, int projectId)
        {
            //Stopwatch _timer = new Stopwatch();
            //_timer.Start();

            //long ts1 = _timer.ElapsedMilliseconds; //////////////////////////////
            //_timer.Restart();

            IList<MappedObject> dpListFiltered = ActiveSession.QueryOver<MappedObject>()
                .Where(m => m.Project.Id == projectId)
                .WhereSpatialRestrictionOn(m => m.point).Within(intensity.geometry)
                .WhereSpatialRestrictionOn(m => m.line).Within(intensity.geometry)
                .WhereSpatialRestrictionOn(m => m.polygon).Within(intensity.geometry)
                .List<MappedObject>();

            //long ts2 = _timer.ElapsedMilliseconds; //////////////////////////////
            //_timer.Restart();

            List<MappedObject> clippedList = new List<MappedObject>();

            foreach (var damagePotential in dpListFiltered)
            {
                //damagePotential can't be outside of intensity
                IGeometry clippedDamagePotentialGeometry = null;
                if (damagePotential.point != null)
                    clippedDamagePotentialGeometry = intensity.geometry.Intersection(damagePotential.point);
                else if (damagePotential.line != null)
                    clippedDamagePotentialGeometry = intensity.geometry.Intersection(damagePotential.line);
                else if (damagePotential.polygon != null)
                    clippedDamagePotentialGeometry = intensity.geometry.Intersection(damagePotential.point);

                //assign new geometry to new MappedObject
                MappedObject clippedDamagePotential = new MappedObject()
                {
                    FreeFillParameter = damagePotential.FreeFillParameter,
                    ID = damagePotential.ID,
                    Objectparameter = damagePotential.Objectparameter,
                    Project = damagePotential.Project,
                    //geometry = clippedDamagePotentialGeometry,            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    IsClipped = true,                                     //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    Intensity = (Intensity)intensity.Clone(),             //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    ResilienceValues = damagePotential.ResilienceValues,
                };
                if (damagePotential.point != null)
                    clippedDamagePotential.point = clippedDamagePotentialGeometry;
                else if (damagePotential.line != null)
                    clippedDamagePotential.line = clippedDamagePotentialGeometry;
                else if (damagePotential.polygon != null)
                    clippedDamagePotential.polygon = clippedDamagePotentialGeometry;

                clippedList.Add(clippedDamagePotential);
            }

            //long ts3 = _timer.ElapsedMilliseconds; //////////////////////////////
            //_timer.Restart();

            //Logging.warn($"    222     {ts1:F2} - {ts2:F2} - {ts3:F3}");
            //_timer.Stop();

            return clippedList;
        }


        /// <summary>
        /// like getDamagePotentialCrossing, but with spatial query on db
        /// </summary>
        /// <param name="intensity"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        //public List<MappedObject> getDamagePotentialCrossing2(Intensity intensity, int projectId)
        //{
        //    //Stopwatch _timer = new Stopwatch();
        //    //_timer.Start();

        //    //long ts1 = _timer.ElapsedMilliseconds; //////////////////////////////
        //    //_timer.Restart();

        //    IList<MappedObject> dpListFiltered = ActiveSession.QueryOver<MappedObject>()
        //        .Where(m => m.Project.Id == projectId)
        //        //.Where(m => m.Project.geometry.Intersects(m.geometry))

        //        .WhereSpatialRestrictionOn(m => m.geometry).Not.Within(intensity.geometry)
        //        .WhereSpatialRestrictionOn(m => m.geometry).Intersects(intensity.geometry)

        //        //.Where(m => !m.geometry.Within(intensity.geometry))
        //        //.Where(m => m.geometry.Intersects(intensity.geometry))
        //        .List<MappedObject>();

        //    //long ts2 = _timer.ElapsedMilliseconds; //////////////////////////////
        //    //_timer.Restart();

        //    List<MappedObject> clippedList = new List<MappedObject>();

        //    foreach (var damagePotential in dpListFiltered)
        //    {
        //        //damagePotential can't be outside of intensity
        //        IGeometry clippedDamagePotentialGeometry = intensity.geometry.Intersection(damagePotential.geometry);

        //        //assign new geometry to new MappedObject
        //        MappedObject clippedDamagePotential = new MappedObject()
        //        {
        //            FreeFillParameter = damagePotential.FreeFillParameter,
        //            ID = damagePotential.ID,
        //            Objectparameter = damagePotential.Objectparameter,
        //            Project = damagePotential.Project,
        //            geometry = clippedDamagePotentialGeometry,            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        //            IsClipped = true,                                     //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        //            Intensity = (Intensity)intensity.Clone(),             //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        //            ResilienceValues = damagePotential.ResilienceValues,
        //        };

        //        clippedList.Add(clippedDamagePotential);
        //    }

        //    //long ts3 = _timer.ElapsedMilliseconds; //////////////////////////////
        //    //_timer.Restart();

        //    //Logging.warn($"    222     {ts1:F2} - {ts2:F2} - {ts3:F3}");
        //    //_timer.Stop();

        //    return clippedList;
        //}

        /// <summary>
        /// DamagePotential crossing intensity.geometry
        /// </summary>
        /// <param name="intensity"></param>
        /// <param name="dpList"></param>
        /// <returns></returns>
        public List<MappedObject> getDamagePotentialCrossing(Intensity intensity, IList<MappedObject> dpList)
        {
            //Stopwatch _timer = new Stopwatch();
            //_timer.Start();

            //            select "ID"
            //from "MappedObject" as mapObj
            //where ST_Intersects((select geometry from "Intensity" where "ID" = 117), mapObj.Geometry) AND
            //NOT ST_Contains((select geometry from "Intensity" where "ID" = 117), mapObj.Geometry);

            var project = DBManager.ActiveSession.Load<Project>(intensity.Project.Id);

            //long ts1 = _timer.ElapsedMilliseconds; //////////////////////////////
            //_timer.Restart();

            List<MappedObject> dpListFiltered = new List<MappedObject>();

            dpListFiltered.AddRange(dpList
                .Where(m => m.point != null &&
                            !m.point.Within(intensity.geometry) &&
                            m.point.Intersects(intensity.geometry)
                            )
                .ToList()
                );
            dpListFiltered.AddRange(dpList
                .Where(m => m.line != null &&
                            !m.line.Within(intensity.geometry) &&
                            m.line.Intersects(intensity.geometry)
                            )
                .ToList()
                );
            dpListFiltered.AddRange(dpList
                .Where(m => m.polygon != null &&
                            !m.polygon.Within(intensity.geometry) &&
                            m.polygon.Intersects(intensity.geometry)
                            )
                .ToList()
                );

            //long ts2 = _timer.ElapsedMilliseconds; //////////////////////////////
            //_timer.Restart();

            List<MappedObject> clippedList = new List<MappedObject>();

            foreach (var damagePotential in dpListFiltered)
            {
                //damagePotential can't be outside of intensity
                IGeometry clippedDamagePotentialGeometry = null;
                if (damagePotential.point != null)
                    clippedDamagePotentialGeometry = intensity.geometry.Intersection(damagePotential.point);
                else if (damagePotential.line != null)
                    clippedDamagePotentialGeometry = intensity.geometry.Intersection(damagePotential.line);
                else if (damagePotential.polygon != null)
                    clippedDamagePotentialGeometry = intensity.geometry.Intersection(damagePotential.polygon);

                //#if DEBUG
                //                //area difference due to intersection
                //                double areaDifference = damagePotential.geometry.Area - clippedDamagePotentialGeometry.Area;
                //                Debug.WriteLine($"DAMAGEPOTENTIAL CLIPPING: DamagePotential {damagePotential.ID}: area diff = {areaDifference:F2}, new area = {clippedDamagePotentialGeometry.Area:F2}, type: {damagePotential.geometry.GeometryType} -> {clippedDamagePotentialGeometry.GeometryType}");
                //#endif

                //assign new geometry to new MappedObject
                MappedObject clippedDamagePotential = new MappedObject()
                {
                    FreeFillParameter = damagePotential.FreeFillParameter,
                    ID = damagePotential.ID,
                    Objectparameter = damagePotential.Objectparameter,
                    Project = damagePotential.Project,
                    //geometry = clippedDamagePotentialGeometry,            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    IsClipped = true,                                     //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    Intensity = (Intensity)intensity.Clone(),             //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    ResilienceValues = damagePotential.ResilienceValues,
                };
                if (damagePotential.point != null)
                    clippedDamagePotential.point = clippedDamagePotentialGeometry;
                else if (damagePotential.line != null)
                    clippedDamagePotential.line = clippedDamagePotentialGeometry;
                else if (damagePotential.polygon != null)
                    clippedDamagePotential.polygon = clippedDamagePotentialGeometry;

                clippedList.Add(clippedDamagePotential);
            }

            //long ts3 = _timer.ElapsedMilliseconds; //////////////////////////////
            //_timer.Restart();

            //Logging.warn($"    111     {ts1:F2} - {ts2:F2} - {ts3:F3}");
            //_timer.Stop();

            return clippedList;
        }

        /// <summary>
        /// Get the merged Objectparameter for a MappedObject
        /// </summary>
        /// <param name="mapObj"></param>
        /// <returns></returns>
        public static Objectparameter getMergedObjectParameter(MappedObject mapObj)
        {
            // default take everything from original
            Objectparameter mergedObjParam = (Objectparameter)mapObj.Objectparameter.Clone();

            // look for free fill properties
            if (mapObj.Objectparameter.MotherObjectparameter != null)
            {
                mergedObjParam.HasProperties = mapObj.Objectparameter.MotherObjectparameter.HasProperties;
                mergedObjParam.ProcessParameters = mapObj.Objectparameter.MotherObjectparameter.ProcessParameters;
                mergedObjParam.FeatureType = mapObj.Objectparameter.MotherObjectparameter.FeatureType;
            }
            else
            {
                mergedObjParam.HasProperties = mapObj.Objectparameter.HasProperties;
                mergedObjParam.FeatureType = mapObj.Objectparameter.FeatureType;
            }

            // copy all free fill properties to merged parameter, wenn free fill has a value
            foreach (ObjectparameterHasProperties ohp in mergedObjParam.HasProperties.Where(m => m.isOptional == true))
            {
                if (mapObj.FreeFillParameter != null)
                {
                    // Get the Property from the Free Fill Object
                    PropertyInfo FreeFillProperty = ohp.Property.Split('.').Select(s => mapObj.FreeFillParameter.GetType().GetProperty(s)).FirstOrDefault();
                    // Get the Property from the Merged Object
                    PropertyInfo MergedProperty = ohp.Property.Split('.').Select(s => mergedObjParam.GetType().GetProperty(s)).FirstOrDefault();

                    // Assign FreeFill PropertyValue to Merged PropertyValue
                    if (FreeFillProperty.GetValue(mapObj.FreeFillParameter) != null)
                    {
                        MergedProperty.SetValue(mergedObjParam, FreeFillProperty.GetValue(mapObj.FreeFillParameter));
                    }
                }
            }

            return mergedObjParam;
        }

        /// <summary>
        /// Delete all damage extents of this project in database and recreate them
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public void createDamageExtent(int projectId)
        {
            Stopwatch _stopWatch = new Stopwatch();
            _stopWatch.Start();

            //DBManager.NewSession(); //bac session

            //ConcurrentBag<DamageExtent> _saveDamageExtents = new ConcurrentBag<DamageExtent>();
            List<DamageExtent> _saveDamageExtents = new List<DamageExtent>();

            var _damageExtentController = new DamageExtentController();
            _damageExtentController.deleteDamageExtentsFromDB(projectId);  //DELETE

            var _hazards = ActiveSession.QueryOver<NatHazard>().List<NatHazard>();
            var _ikClasses = ActiveSession.QueryOver<IKClasses>().List<IKClasses>();
            List<bool> _beforeActions = new List<bool>() { true, false };

            Logging.warn($"querying: elapsed time = " + _stopWatch.Elapsed.ToString());
            _stopWatch.Restart();

            var _damagePotentialController = new MappedObjectController();
            var _allAffectedDamagePotentials = _damagePotentialController.getAllDamagePotentials(projectId); //unprocessed Damage Potentials in the project perimeter

            Logging.warn($"getAllDamagePotentials: elapsed time = " + _stopWatch.Elapsed.ToString() + $", count= {_allAffectedDamagePotentials.Count()}");
            _stopWatch.Restart();

            foreach (var hazard in _hazards)
            {
                foreach (var period in _ikClasses)
                {
                    foreach (var beforeAction in _beforeActions)
                    {
                        _stopWatch.Restart();

                        var _controller = new IntensityController();

                        List<Intensity> _intensities = _controller.getIntensityMap(projectId, hazard.ID, period.ID, beforeAction);
                        List<MappedObject> _allProcessedDamagePotentials = new List<MappedObject>();

                        Logging.warn($"getIntensityMap: elapsed time = " + _stopWatch.Elapsed.ToString());
                        _stopWatch.Restart();

                        if (_intensities == null || _intensities.Count() == 0)
                            continue;
                        Stopwatch _damageWatch = new Stopwatch();
                        _damageWatch.Start();

                        // gather all processed DamagePotentials
                        foreach (var intensity in _intensities)
                        {
                            _damageWatch.Restart();

                            IList<MappedObject> dpListWithin = _damagePotentialController.getDamagePotentialsWithin(intensity, _allAffectedDamagePotentials);
                            List<MappedObject> outlist = dpListWithin.ToList();

                            Logging.warn($">getDamagePotentialsWithin: elapsed time = " + _damageWatch.Elapsed.ToString());
                            _damageWatch.Restart();

                            //IList<MappedObject> dpListWithin2 = _damagePotentialController.getDamagePotentialsWithin2(intensity, projectId);
                            //List<MappedObject> outlist = dpListWithin2.ToList();
                            //Logging.warn($">getDamagePotentialsWithin2: elapsed time = " + _damageWatch.Elapsed.ToString());
                            //_damageWatch.Restart();

                            IList<MappedObject> dpListCrossing = _damagePotentialController.getDamagePotentialCrossing(intensity, _allAffectedDamagePotentials);
                            outlist.AddRange(dpListCrossing); //Merge Within and Crossing for Intensity

                            Logging.warn($">getDamagePotentialCrossing: elapsed time = " + _damageWatch.Elapsed.ToString());
                            _damageWatch.Restart();

                            //IList<MappedObject> dpListCrossing2 = _damagePotentialController.getDamagePotentialCrossing2(intensity, projectId);
                            //outlist.AddRange(dpListCrossing2); //Merge Within and Crossing for Intensity
                            //Logging.warn($">getDamagePotentialCrossing2: elapsed time = " + _damageWatch.Elapsed.ToString());
                            //_damageWatch.Restart();

                            _allProcessedDamagePotentials.AddRange(outlist); //collect all processed DamagePotentials
                        }
                        _damageWatch.Stop();

                        Logging.warn($"getDamagePotential: elapsed time = " + _stopWatch.Elapsed.ToString());
                        _stopWatch.Restart();

                        //Parallel.ForEach(_intensities,
                        //    (intensity) =>
                        //    {

                        foreach (var intensity in _intensities)
                        {

                            var outlist = _allProcessedDamagePotentials.Where(o => o.Intensity.ID == intensity.ID).ToList();

                            List<DamageExtent> outDamageExtents = new List<DamageExtent>();
                            //ConcurrentBag<DamageExtent> outDamageExtents = new ConcurrentBag<DamageExtent>();

                            //Parallel.ForEach(outlist, (damagePotential) =>
                            //{

                            foreach (MappedObject damagePotential in outlist)
                            {
                                DamageExtent damageExtent = null;

                                damageExtent = DamageExtentController.computeDamageExtent(damagePotential, intensity);

                                if (damageExtent != null)
                                    outDamageExtents.Add(damageExtent);
                            }
                            //});

                            _saveDamageExtents.AddRange(outDamageExtents);
                            //_saveDamageExtents.AddRange<DamageExtent>(outDamageExtents);

                            //double _sumPersonDamage = outDamageExtents.Sum(x => x.PersonDamage);
                            //double _sumDeaths = outDamageExtents.Sum(x => x.Deaths);
                            //double _sumProptertyDamage = outDamageExtents.Sum(x => x.PropertyDamage);

                            //Logging.warn($"  #DamageExtent: {outlist.Count}");

                        } //loop over intensities
                          //});

                        Logging.warn($"computeDamageExtent: elapsed time = " + _stopWatch.Elapsed.ToString());
                        _stopWatch.Restart();

                    }//loop over actions
                }//loop over period

            }//loop over hazard

            Logging.warn($"inbetween: elapsed time = " + _stopWatch.Elapsed.ToString());
            _stopWatch.Restart();

            //one time saving to db <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            _damageExtentController.saveDamageExtentToDB(_saveDamageExtents.ToList()); //SAVE

            Logging.warn($"saveDamageExtentToDB: elapsed time = " + _stopWatch.Elapsed.ToString());
            _stopWatch.Restart();

            _stopWatch.Stop();


            //Change the project to state "Calculated"
            var _resultController = new ResultController();
            _resultController.setProjectStatus(projectId, 2);

        }


    }
}
