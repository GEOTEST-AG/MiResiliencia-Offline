using ResTB.DB;
using ResTB.DB.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ResTB.GUI.Helpers
{
    public static class MappedObjectManager
    {
        private static TEntity ShallowCopyEntity<TEntity>(TEntity source) where TEntity : class, new()
        {

            // Get properties from EF that are read/write and not marked witht he NotMappedAttribute
            var sourceProperties = typeof(TEntity)
                                    .GetProperties()
                                    .Where(p => p.CanRead && p.CanWrite &&
                                                p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute), true).Length == 0);
            var newObj = new TEntity();

            foreach (var property in sourceProperties)
            {

                // Copy value
                property.SetValue(newObj, property.GetValue(source, null), null);

            }

            return newObj;

        }

        /// <summary>
        /// Get the merged Objectparameter for a MappedObject
        /// </summary>
        public static Objectparameter GetMergedObjectparameter(int mapObjID)
        {
            MappedObject mapObj = null;

            using (ResTBContext db = new ResTBContext())
            {
                mapObj = db.MappedObjects.AsNoTracking()
                   .Include(m => m.FreeFillParameter)
                   .Include(m => m.Objectparameter.HasProperties)
                   .Include(m => m.Objectparameter.MotherOtbjectparameter.HasProperties)
                   .Include(m => m.Objectparameter.MotherOtbjectparameter.ObjectparameterPerProcesses)
                   .Include(m => m.Objectparameter.MotherOtbjectparameter.ResilienceFactors.Select(r => r.ResilienceWeights))
                   .Include(m => m.Objectparameter.ResilienceFactors.Select(r => r.ResilienceWeights))
                   .Include(m => m.ResilienceValues.Select(rv => rv.ResilienceWeight).Select(rw => rw.ResilienceFactor))
                   .Where(m => m.ID == mapObjID)
                   .FirstOrDefault();

                if (mapObj == null)
                    return null;

                // default take everything from original
                Objectparameter mergedObjParam = ShallowCopyEntity<Objectparameter>(mapObj.Objectparameter);

                // look for free fill properties
                if (mapObj.Objectparameter.MotherOtbjectparameter != null)
                {
                    mergedObjParam.HasProperties = mapObj.Objectparameter.MotherOtbjectparameter.HasProperties;
                    mergedObjParam.ObjectparameterPerProcesses = mapObj.Objectparameter.MotherOtbjectparameter.ObjectparameterPerProcesses;
                    mergedObjParam.FeatureType = mapObj.Objectparameter.MotherOtbjectparameter.FeatureType;
                    mergedObjParam.ResilienceFactors = mapObj.Objectparameter.MotherOtbjectparameter.ResilienceFactors;
                }
                else
                {
                    mergedObjParam.HasProperties = mapObj.Objectparameter.HasProperties;
                    mergedObjParam.ObjectparameterPerProcesses = mapObj.Objectparameter.ObjectparameterPerProcesses;
                    mergedObjParam.FeatureType = mapObj.Objectparameter.FeatureType;
                    mergedObjParam.ResilienceFactors = mapObj.Objectparameter.ResilienceFactors;
                }
                mergedObjParam.ResilienceFactors = mergedObjParam.ResilienceFactors.OrderBy(r => r.ID).ToList();

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

                //Add all ResilienceFactors to merged object
                foreach (ResilienceFactor resFac in mergedObjParam.ResilienceFactors)
                {
                    foreach (ResilienceWeight resWeight in resFac.ResilienceWeights)
                    {
                        var resValue = new ResilienceValues()
                        {
                            MappedObject = mapObj,
                            ResilienceWeight = resWeight,
                            OverwrittenWeight = resWeight.Weight,
                            Value = 0
                        };

                        if (mapObj.ResilienceValues.Any(rv => rv.ResilienceWeight.ID == resWeight.ID))
                        {
                            var mappedResValue = mapObj.ResilienceValues.Where(rv => rv.ResilienceWeight.ID == resWeight.ID).Single();

                            resValue = mappedResValue;
                        }

                        mergedObjParam.ResilienceValuesMerged.Add(resValue);
                    }

                }

                return mergedObjParam;
            }

        }

        /// <summary>
        /// Transfer resilience values between mapped objects
        /// </summary>
        /// <param name="fromID"></param>
        /// <param name="toID"></param>
        public static async Task<bool> CopyResilience(int fromID, int toID, bool onlyAfterAction = false)
        {
            using (ResTBContext db = new ResTBContext())
            {
                var fromMapObj = await db.MappedObjects.AsNoTracking()
                    .Include(m => m.FreeFillParameter)
                    .Include(m => m.ResilienceValues.Select(rv => rv.ResilienceWeight).Select(rw => rw.ResilienceFactor))
                    .Where(m => m.ID == fromID)
                    .FirstOrDefaultAsync();

                var toMapObj = await db.MappedObjects
                    .Include(m => m.FreeFillParameter)
                    .Include(m => m.ResilienceValues.Select(rv => rv.ResilienceWeight).Select(rw => rw.ResilienceFactor))
                    .Where(m => m.ID == toID)
                    .FirstOrDefaultAsync();

                if (fromMapObj != null && toMapObj != null)
                {
                    //toMapObj.ResilienceValues.Clear();  // only reference of mappedObject is deleted in db !!!!

                    if (onlyAfterAction)
                    {
                        db.ResilienceValues.RemoveRange(toMapObj.ResilienceValues.Where(r => r.ResilienceWeight.BeforeAction == false)); //delete only afterAction
                    }
                    else
                    {
                        db.ResilienceValues.RemoveRange(toMapObj.ResilienceValues); //delete all
                    }
                    await db.SaveChangesAsync();

                    var resWeightIDsfromMapObj = fromMapObj.ResilienceValues.Select(rv => rv.ResilienceWeight.ID);

                    var resWeightsFromMapObj = db.ResilienceWeights.Where(r => resWeightIDsfromMapObj.Contains(r.ID));

                    foreach (ResilienceValues resVal in fromMapObj.ResilienceValues)
                    {
                        ResilienceWeight resWeight = await resWeightsFromMapObj.SingleAsync(r => r.ID == resVal.ResilienceWeight.ID);

                        if (onlyAfterAction)
                        {
                            if ((bool)resWeight.BeforeAction)
                                continue;
                        }

                        toMapObj.ResilienceValues.Add(
                            new ResilienceValues()
                            {
                                OverwrittenWeight = resVal.OverwrittenWeight,
                                Value = resVal.Value,
                                ResilienceWeight = resWeight
                            }
                        );
                    }

                    await db.SaveChangesAsync();
                }
            }
            return true;
        }
    }
}
