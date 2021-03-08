using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NHibernate;
using ResTB_API.Helpers;
using ResTB_API.Models.Database.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ResTB_API.Controllers
{
    public class IntensityController
    {
        public IntensityController() { }

        public static ISession ActiveSession
        {
            get
            {
                return DBManager.ActiveSession;
            }
        }

        /// <summary>
        /// get prA from intensity
        /// </summary>
        /// <param name="intensity"></param>
        /// <returns></returns>
        public PrA getPrA(Intensity intensity)
        {
            PrA pra = intensity.Project.PrAs.Where(p => p.IKClass.ID == intensity.IKClasses.ID && p.NatHazard.ID == intensity.NatHazard.ID).SingleOrDefault();

            return pra;
        }

        /// <summary>
        /// Clip intensity to project perimeter, fix geometry for intersection & difference
        /// Attention: "side effect" = geometry of intensity changes
        /// </summary>
        /// <param name="intens"></param>
        public void clipToProject(Intensity intens)
        {
            // trick to make all geometries valid for Intersection / Difference: Buffer(0.001)  <<<<<<<<<<<<<<<<<<<<<<
            intens.geometry = GeometryTools.Polygon2Multipolygon(intens.geometry.Buffer(0.001));

            //intensity can't be outside of perimeter

            var perimeter = DBManager.ActiveSession.QueryOver<Perimeter>().Where(p => p.Project.Id == intens.Project.Id).List<Perimeter>().FirstOrDefault();
            if (perimeter == null || perimeter.geometry == null)
                throw new NullReferenceException(nameof(perimeter) + " " + nameof(perimeter.geometry));

            IGeometry clippedIntens = perimeter.geometry.Intersection(intens.geometry);

            //keep multipolygon geometry
            clippedIntens = GeometryTools.Polygon2Multipolygon(clippedIntens.Buffer(0.001));

            //area difference due to intersection
            double areaDifference = intens.geometry.Area - clippedIntens.Area;

            Debug.WriteLine($"PROJECT CLIPPING: Intensity {intens.ID}: area diff = {areaDifference:F2}, new area = {clippedIntens.Area:F2}, type: {intens.geometry.GeometryType} -> {clippedIntens.GeometryType}");

            //assign new geometry
            intens.geometry = (MultiPolygon)clippedIntens;

        }

        /// <summary>
        /// Get Insitiy Maps with clipped intensities (without overlaps)
        /// </summary>
        /// <param name="projectId">project id</param>
        /// <param name="hazardId">hazard id</param>
        /// <param name="periodId">period id</param>
        /// <param name="beforeAction">beforeAction boolean</param>
        /// <returns></returns>
        public List<Intensity> getIntensityMap(int projectId, int hazardId, int periodId, bool beforeAction)
        {
            try
            {
                IList<Intensity> intensityListRaw = ActiveSession.QueryOver<Intensity>()
                    .Where(i => i.Project.Id == projectId &&
                           i.NatHazard.ID == hazardId &&
                           i.BeforeAction == beforeAction &&
                           i.IKClasses.ID == periodId &&
                           i.IntensityDegree <= 2)
                    .List<Intensity>();

                if (intensityListRaw == null || intensityListRaw.Count == 0)
                {
                    //Debug.WriteLine($"Warning @ {nameof(getIntensityMap)}: No Intensities found for project id {projectId}, hazard {hazardId}, period {periodId}, beforeAction {beforeAction}");
                    return null;
                }

                //avoiding side effects: copying intensities
                IList<Intensity> intensityList = new List<Intensity>();

                //Merging Intensities with identical intensityDegrees
                foreach (int intDegree in new List<int>() { 0, 1, 2 })
                {
                    var intMapsOfDegree = intensityListRaw.Where(i => i.IntensityDegree == intDegree);

                    if (intMapsOfDegree.Any()) //clipping
                    {
                        IGeometry multigeometry = intMapsOfDegree.First().geometry;
                        foreach (var item in intMapsOfDegree.Skip(1))
                        {
                            multigeometry.Union(item.geometry);
                        }

                        var intensCopy = (Intensity)intMapsOfDegree.First().Clone();
                        intensCopy.geometry = multigeometry;

                        intensityList.Add(intensCopy);

                    }
                }

                //clip to project and fix geometries
                foreach (Intensity intens in intensityList)
                {
                    clipToProject(intens);
                }

                if (intensityList.Count > 1) //2 or more intensities available
                {
                    if (intensityList.Count > 3)
                        throw new ArgumentOutOfRangeException(nameof(intensityList));

                    // intensityDegree: 0=high, 1=medium, 2=low
                    Intensity high = intensityList.OrderBy(i => i.IntensityDegree).First();
                    Intensity medium = intensityList.OrderBy(i => i.IntensityDegree).Skip(1).First();

                    // compute geometry of "medium" that is not in "high"
                    var clip1 = medium.geometry.Difference(high.geometry);

                    ////area difference due to difference
                    //double areaDifference = medium.geometry.Area - clip1.Area;
                    //Debug.WriteLine($"INTENSITY CLIPPING: Intensity {medium.ID}: area diff = {areaDifference:F2}, new area = {clip1.Area:F2}");

                    // save geometry back to "medium"
                    medium.geometry = GeometryTools.Polygon2Multipolygon(clip1.Buffer(0.001));

                    if (intensityList.Count > 2)
                    {
                        Intensity low = intensityList.OrderBy(i => i.IntensityDegree).Skip(2).First();

                        var clip2 = low.geometry.Difference(medium.geometry).Difference(high.geometry);

                        low.geometry = GeometryTools.Polygon2Multipolygon(clip2.Buffer(0.001));
                    }
                }

                return intensityList.OrderBy(i => i.IntensityDegree).ToList<Intensity>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
