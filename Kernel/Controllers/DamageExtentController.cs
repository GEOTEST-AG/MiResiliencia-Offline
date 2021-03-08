using NHibernate;
using ResTB_API.Helpers;
using ResTB_API.Models;
using ResTB_API.Models.Database.Domain;
using ResTB_API.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace ResTB_API.Controllers
{
    public class DamageExtentController
    {

        public static ISession ActiveSession
        {
            get
            {
                return DBManager.ActiveSession;
            }
        }

        private static double _willingnessToPay = -1;
        public static double WillingnessToPay
        {
            get
            {
                if (_willingnessToPay == -1)
                {
                    WillingnessToPay willingnessToPay = DBManager.ActiveSession.Load<WillingnessToPay>(1);
                    _willingnessToPay = willingnessToPay.Value;
                }
                return _willingnessToPay;
            }
        }

        /// <summary>
        /// Computation of all Damage Extent of a Damage Potential in a given Intensity
        /// </summary>
        /// <param name="mapObj">Damage Potential</param>
        /// <param name="intensity"></param>
        /// <returns></returns>
        public static DamageExtent computeDamageExtent(MappedObject mapObj, Intensity intensity)//, List<MappedObject> clippedObjects = null)
        {
            var _damageExtent = new DamageExtent()
            {
                Intensity = (Intensity)intensity.Clone(),        //make sure shallow copy is used
                MappedObject = (MappedObject)mapObj.Clone(),     //make sure shallow copy is used
                Clipped = mapObj.IsClipped,
            };

            if (mapObj.point != null) _damageExtent.geometry = mapObj.point;
            if (mapObj.line != null) _damageExtent.geometry = mapObj.line;
            if (mapObj.polygon != null) _damageExtent.geometry = mapObj.polygon;

            int _intensityDegree = intensity.IntensityDegree; //0=high, 1=med, 2=low

            //Merge Objectparameter with Freefillparamter
            var _mergedObjParam = MappedObjectController.getMergedObjectParameter(mapObj);

            //get Objectparameters for NatHazard (Vulnerability, Mortality, indirect costs)
            int _motherObjectID = _mergedObjParam.MotherObjectparameter != null
                                    ? _mergedObjParam.MotherObjectparameter.ID
                                    : _mergedObjParam.ID;

            ObjectparameterPerProcess _objectParamProcess;
            _objectParamProcess = _mergedObjParam.ProcessParameters
                                     .Where(pp => pp.NatHazard.ID == intensity.NatHazard.ID &&
                                                  pp.Objectparameter.ID == _motherObjectID)
                                     .SingleOrDefault();

            if (_objectParamProcess == null)
            {
                _damageExtent.Log += $"ERROR: NO PROCESS PARAMETER, count: {_mergedObjParam.ProcessParameters.Count} \n";
                return _damageExtent;
            }

            //get pra for intensity
            var _intensityController = new IntensityController();
            PrA _prA = _intensityController.getPrA(intensity);

            if (_prA == null)
            {
                _damageExtent.Log += $"ERROR: NO PrA VALUES FOUND FOR PROCESS {intensity.NatHazard.Name.ToUpper()} \n";
                return _damageExtent;
            }


            // BUILDINGS and SPECIAL BUILDINGS
            if (_mergedObjParam.ObjectClass.ID <= 2)
            {

                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // PERSON DAMAGE

                double _deaths = _prA.Value * _mergedObjParam.Personcount * _mergedObjParam.Presence / 24.0;
                string _logDeaths1 = $"Deaths = prA * PersonCount * Presence/24";
                string _logDeaths2 = $"Deaths = {_prA.Value:F3} * {_mergedObjParam.Personcount} * {_mergedObjParam.Presence:F1} / 24";

                double _deathProbability = _mergedObjParam.Personcount > 0 ? 1.0d / _mergedObjParam.Personcount : 0;
                string _logDProb1 = _mergedObjParam.Personcount > 0 ? $"IndividualDeathRisk = 1 / PersonCount" : "ERROR: 1 / PersonCount";
                string _logDProb2 = _mergedObjParam.Personcount > 0 ? $"IndividualDeathRisk =  1 / {_mergedObjParam.Personcount}" : $"ERROR: 1 / {_mergedObjParam.Personcount}";
                if (_mergedObjParam.Personcount < 1)
                    _damageExtent.Log = $"{ResModel.DE_PersonCount} = {_mergedObjParam.Personcount} \n";

                //switching on intensity degree 
                switch (_intensityDegree)
                {
                    case 0:
                        _deaths *= _objectParamProcess.MortalityHigh;
                        _logDeaths1 += $" * MortalityHigh";
                        _logDeaths2 += $" * {_objectParamProcess.MortalityHigh:F3}";
                        break;
                    case 1:
                        _deaths *= _objectParamProcess.MortalityMedium;
                        _logDeaths1 += $" * MortalityMedium";
                        _logDeaths2 += $" * {_objectParamProcess.MortalityMedium:F3}";
                        break;
                    case 2:
                        _deaths *= _objectParamProcess.MortalityLow;
                        _logDeaths1 += $" * MortalityLow";
                        _logDeaths2 += $" * {_objectParamProcess.MortalityLow:F3}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_intensityDegree), _intensityDegree, "out of range");
                }

                //looking for floors count, if available for this object type
                if (_mergedObjParam.HasProperties.Where(m => m.Property == nameof(_mergedObjParam.Floors)).Any())
                {
                    _deaths *= _mergedObjParam.Floors;
                    _logDeaths1 += $" * Floors";
                    _logDeaths2 += $" * {_mergedObjParam.Floors}";

                    if (_mergedObjParam.Floors > 0)
                    {
                        _deathProbability /= _mergedObjParam.Floors;
                        _logDProb1 += $" / Floors";
                        _logDProb2 += $" / {_mergedObjParam.Floors}";
                    }
                    else
                        _damageExtent.Log += $"{ResModel.DE_Floors} = {_mergedObjParam.Floors} \n";
                }

                _damageExtent.Deaths = _deaths;
                _damageExtent.LogDeaths = _logDeaths1 + ";\n" + _logDeaths2;

                _deathProbability *= _deaths;
                _logDProb1 += $" * Deaths";
                _logDProb2 += $" * {_deaths:F6}";
                _damageExtent.DeathProbability = _deathProbability;
                _damageExtent.LogDeathProbability = _logDProb1 + ";\n" + _logDProb2;

                _damageExtent.PersonDamage = _deaths * WillingnessToPay;
                _damageExtent.LogPersonDamage = $"PersonDamage = Deaths * WillingnessToPay;\n";
                _damageExtent.LogPersonDamage += $"PersonDamage = {_deaths:F6} * {WillingnessToPay:C}";


                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // PROPERTY DAMAGE

                double _propertyDamage = _prA.Value * _mergedObjParam.Value;
                string _logPropertyDamage1 = $"PropertyDamage = prA * Value";
                string _logPropertyDamage2 = $"PropertyDamage = {_prA.Value:F3} * {_mergedObjParam.Value:C}";

                switch (_mergedObjParam.FeatureType)
                {
                    case 0: //POINT BASED OBJECT(like communication tower)
                        _damageExtent.Piece = 1;
                        _logPropertyDamage1 += $" * Piece";
                        _logPropertyDamage2 += $" * 1";
                        if (_damageExtent.Clipped)
                        {
                            _damageExtent.Part = 1.0d;
                        }

                        break;

                    case 1: //LINE BASED OBJECT (like Aduccion)
                        _damageExtent.Length = mapObj.line.Length;
                        if (_damageExtent.Clipped)
                        {
                            var rawMapObject = MappedObjectController.ActiveSession.Load<MappedObject>(mapObj.ID);
                            _damageExtent.Part = mapObj.line.Length / rawMapObject.line.Length;
                        }

                        _propertyDamage *= mapObj.line.Length;
                        _logPropertyDamage1 += $" * Length";
                        _logPropertyDamage2 += $" * {mapObj.line.Length:F3}";
                        break;

                    case 2: //POLYGON BASED OBJECT
                        _damageExtent.Area = mapObj.polygon.Area;
                        if (_damageExtent.Clipped)
                        {
                            var rawMapObject = MappedObjectController.ActiveSession.Load<MappedObject>(mapObj.ID);
                            _damageExtent.Part = mapObj.polygon.Area / rawMapObject.polygon.Area;
                        }

                        _propertyDamage *= mapObj.polygon.Area;
                        _logPropertyDamage1 += $" * Area";
                        _logPropertyDamage2 += $" * {mapObj.polygon.Area:F3}";
                        break;

                    default:
                        _damageExtent.Log += $"ERROR: BUILDING, FEATURETYPE = {_mergedObjParam.ObjectClass.ID}, {_mergedObjParam.FeatureType} \n";
                        return _damageExtent;

                }

                //looking for floors count, if available for this object type
                if (_mergedObjParam.HasProperties.Where(m => m.Property == nameof(_mergedObjParam.Floors)).Any())
                {
                    _propertyDamage *= _mergedObjParam.Floors;
                    _logPropertyDamage1 += $" * Floors";
                    _logPropertyDamage2 += $" * {_mergedObjParam.Floors}";
                }

                switch (_intensityDegree)
                {
                    case 0:
                        _propertyDamage *= _objectParamProcess.VulnerabilityHigh;
                        _logPropertyDamage1 += $" * VulnerabilityHigh";
                        _logPropertyDamage2 += $" * {_objectParamProcess.VulnerabilityHigh:F3}";

                        break;
                    case 1:
                        _propertyDamage *= _objectParamProcess.VulnerabilityMedium;
                        _logPropertyDamage1 += $" * VulnerabilityMedium";
                        _logPropertyDamage2 += $" * {_objectParamProcess.VulnerabilityMedium:F3}";
                        break;
                    case 2:
                        _propertyDamage *= _objectParamProcess.VulnerabilityLow;
                        _logPropertyDamage1 += $" * VulnerabilityLow";
                        _logPropertyDamage2 += $" * {_objectParamProcess.VulnerabilityLow:F3}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_intensityDegree), _intensityDegree, "out of range");
                }

                _damageExtent.PropertyDamage = _propertyDamage;
                _damageExtent.LogPropertyDamage = _logPropertyDamage1 + ";\n" + _logPropertyDamage2;

                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            }
            // INFRASTRUCTURE
            else if (_mergedObjParam.ObjectClass.ID == 3)
            {
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // PERSON DAMAGE

                // STREETS / BRIDGES
                // LINE OBJECT
                if (_mergedObjParam.FeatureType == 1)
                {
                    if (_mergedObjParam.Personcount == 0)
                    {
                        _damageExtent.Deaths = 0;
                        _damageExtent.LogDeaths = "no person damage";

                        _damageExtent.DeathProbability = 0;
                        _damageExtent.LogDeathProbability = "no person damage";

                        _damageExtent.PersonDamage = 0;
                        _damageExtent.LogPersonDamage = "no person damage";
                    }
                    else
                    {

                        _damageExtent.Length = mapObj.line.Length;

                        double _deaths = _prA.Value * _mergedObjParam.Personcount *
                                        (double)_mergedObjParam.NumberOfVehicles * _damageExtent.Length / (double)_mergedObjParam.Velocity / 24000.0d;

                        string _logDeaths1 = $"Deaths = prA * PersonCount * NumberOfVehicles * Length / Velocity / 24000";
                        string _logDeaths2 = $"Deaths = {_prA.Value:F3} * {_mergedObjParam.Personcount} * {_mergedObjParam.NumberOfVehicles} * {_damageExtent.Length:F3} / {_mergedObjParam.Velocity} / 24000";

                        switch (_intensityDegree)
                        {
                            case 0:
                                _deaths *= _objectParamProcess.MortalityHigh;
                                _logDeaths1 += $" * MortalityHigh";
                                _logDeaths2 += $" * {_objectParamProcess.MortalityHigh:F3}";
                                break;
                            case 1:
                                _deaths *= _objectParamProcess.MortalityMedium;
                                _logDeaths1 += $" * MortalityMedium";
                                _logDeaths2 += $" * {_objectParamProcess.MortalityMedium:F3}";
                                break;
                            case 2:
                                _deaths *= _objectParamProcess.MortalityLow;
                                _logDeaths1 += $" * MortalityLow";
                                _logDeaths2 += $" * {_objectParamProcess.MortalityLow:F3}";
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(_intensityDegree), _intensityDegree, "out of range");
                        }

                        _damageExtent.Deaths = _deaths;
                        _damageExtent.LogDeaths = _logDeaths1 + ";\n" + _logDeaths2;

                        int _passagesSamePerson = 4; //TODO: HARD CODED
                        double _deathProbability = _deaths * _passagesSamePerson / _mergedObjParam.NumberOfVehicles / _mergedObjParam.Personcount;
                        string _logDProb1 = $"IndivudualDeathRisk = Deaths * PassagesSamePerson / NumberOfVehicles / PersonCount";
                        string _logDProb2 = $"IndivudualDeathRisk = {_deaths:F6} * {_passagesSamePerson} / {_mergedObjParam.NumberOfVehicles} / {_mergedObjParam.Personcount}";
                        _damageExtent.DeathProbability = _deathProbability;
                        _damageExtent.LogDeathProbability = _logDProb1 + ";\n" + _logDProb2;

                        _damageExtent.PersonDamage = _deaths * WillingnessToPay;
                        _damageExtent.LogPersonDamage = $"PersonDamage = Deaths * WillingnessToPay;\n";
                        _damageExtent.LogPersonDamage += $"PersonDamage = {_deaths:F6} * {WillingnessToPay:C}";
                    }
                }
                // TOWERS
                // POINT OBJECT
                else if (_mergedObjParam.FeatureType == 0)
                {
                    _damageExtent.Deaths = 0;
                    _damageExtent.LogDeaths = "no person damage";

                    _damageExtent.DeathProbability = 0;
                    _damageExtent.LogDeathProbability = "no person damage";

                    _damageExtent.PersonDamage = 0;
                    _damageExtent.LogPersonDamage = "no person damage";
                }
                // POLYGON NOT IMPLEMENTED
                else
                {
                    _damageExtent.Log += $"ERROR: Feature type not implemented";
                    return _damageExtent;
                }


                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // PROPERTY DAMAGE

                double _propertyDamage = _prA.Value * _mergedObjParam.Value;
                string _logPropertyDamage1 = $"PropertyDamage = prA * Value";
                string _logPropertyDamage2 = $"PropertyDamage = {_prA.Value:F3} * {_mergedObjParam.Value:C}";

                switch (_mergedObjParam.FeatureType)
                {
                    case 0: //POINT BASED OBJECT (like towers)
                        _damageExtent.Piece = 1;
                        _logPropertyDamage1 += $" * Piece";
                        _logPropertyDamage2 += $" * 1";
                        break;

                    case 1: //LINE BASED OBJECT (like streets)
                        _damageExtent.Length = mapObj.line.Length;

                        _propertyDamage *= mapObj.line.Length;
                        _logPropertyDamage1 += $" * Length";
                        _logPropertyDamage2 += $" * {mapObj.line.Length:F3}";
                        break;

                    default:
                        _damageExtent.Log += $"ERROR: Infrastructure, FEATURETYPE = {_mergedObjParam.FeatureType} \n";
                        return _damageExtent;
                }

                switch (_intensityDegree)
                {
                    case 0:
                        _propertyDamage *= _objectParamProcess.VulnerabilityHigh;
                        _logPropertyDamage1 += $" * VulnerabilityHigh";
                        _logPropertyDamage2 += $" * {_objectParamProcess.VulnerabilityHigh:F3}";
                        break;
                    case 1:
                        _propertyDamage *= _objectParamProcess.VulnerabilityMedium;
                        _logPropertyDamage1 += $" * VulnerabilityMedium";
                        _logPropertyDamage2 += $" * {_objectParamProcess.VulnerabilityMedium:F3}";
                        break;
                    case 2:
                        _propertyDamage *= _objectParamProcess.VulnerabilityLow;
                        _logPropertyDamage1 += $" * VulnerabilityLow";
                        _logPropertyDamage2 += $" * {_objectParamProcess.VulnerabilityLow:F3}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_intensityDegree), _intensityDegree, "out of range");
                }

                _damageExtent.PropertyDamage = _propertyDamage;
                _damageExtent.LogPropertyDamage = _logPropertyDamage1 + ";\n" + _logPropertyDamage2;

                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            }
            // AGRICULTURE
            else if (_mergedObjParam.ObjectClass.ID == 4)
            {
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // PERSON DAMAGE

                _damageExtent.Deaths = 0;
                _damageExtent.LogDeaths = "no person damage";

                _damageExtent.DeathProbability = 0;
                _damageExtent.LogDeathProbability = "no person damage";

                _damageExtent.PersonDamage = 0;
                _damageExtent.LogPersonDamage = "no person damage";

                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                // PROPERTY DAMAGE

                double _propertyDamage = _prA.Value * _mergedObjParam.Value; //value per hectare!!!
                string _logPropertyDamage1 = $"PropertyDamage = prA * Value";
                string _logPropertyDamage2 = $"PropertyDamage = {_prA.Value:F3} * {_mergedObjParam.Value:C}";

                switch (_mergedObjParam.FeatureType)
                {
                    case 2: //POLYGON BASED OBJECT
                        _damageExtent.Area = mapObj.polygon.Area;
                        _propertyDamage *= _damageExtent.Area / 10000.0;  //in hectare!
                        _logPropertyDamage1 += $" * Area / 10000";
                        _logPropertyDamage2 += $" * {(_damageExtent.Area / 10000.0):F3}";
                        break;

                    default:
                        _damageExtent.Log += $"ERROR: Agriculture, FEATURETYPE = {_mergedObjParam.FeatureType} \n";
                        return _damageExtent;
                }

                switch (_intensityDegree)
                {
                    case 0:
                        _propertyDamage *= _objectParamProcess.VulnerabilityHigh;
                        _logPropertyDamage1 += $" * VulnerabilityHigh";
                        _logPropertyDamage2 += $" * {_objectParamProcess.VulnerabilityHigh:F3}";
                        break;
                    case 1:
                        _propertyDamage *= _objectParamProcess.VulnerabilityMedium;
                        _logPropertyDamage1 += $" * VulnerabilityMedium";
                        _logPropertyDamage2 += $" * {_objectParamProcess.VulnerabilityMedium:F3}";
                        break;
                    case 2:
                        _propertyDamage *= _objectParamProcess.VulnerabilityLow;
                        _logPropertyDamage1 += $" * VulnerabilityLow";
                        _logPropertyDamage2 += $" * {_objectParamProcess.VulnerabilityLow:F3}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_intensityDegree), _intensityDegree, "out of range");
                }

                _damageExtent.PropertyDamage = _propertyDamage;
                _damageExtent.LogPropertyDamage = _logPropertyDamage1 + ";\n" + _logPropertyDamage2;

                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            }

            else
            {
                _damageExtent.Log += $"ERROR: OBJECT CLASS = {_mergedObjParam.ObjectClass.ID} \n";
                return _damageExtent;
            }

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            // RESILIENCE FACTOR
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            if (mapObj.ResilienceValues != null && mapObj.ResilienceValues.Any())
            {
                Tuple<double, string> _resilience = MappedObjectController.computeResilienceFactor(mapObj.ResilienceValues.ToList(), intensity);

                _damageExtent.ResilienceFactor = _resilience.Item1;
                _damageExtent.LogResilienceFactor = _resilience.Item2;
            }
            else
            {
                _damageExtent.ResilienceFactor = 0;
                _damageExtent.LogResilienceFactor = "no resilience available";
            }

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            // INDIRECT DAMAGE
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            double _indirectDamage = 0;
            string _logIndirectDamage1 = "";
            string _logIndirectDamage2 = "";
            if (_mergedObjParam.ObjectClass.ID <= 3)    //Value = 1$
            {
                _indirectDamage = _prA.Value;
                _logIndirectDamage1 = $"IndirectDamage = prA";
                _logIndirectDamage2 = $"IndirectDamage = {_prA.Value:F3}";
            }
            else //Value according to DB
            {
                _indirectDamage = _prA.Value * _objectParamProcess.Value;
                _logIndirectDamage1 = $"IndirectDamage = prA * Value";
                _logIndirectDamage2 = $"IndirectDamage = {_prA.Value:F3} * {_objectParamProcess.Value:C}";
            }

            //if (_mergedObjParam.ObjectClass.ID != 3) //not available for infrastructure
            {
                switch (_intensityDegree) //0=high, 1=med, 2=low
                {
                    case 0:
                        _indirectDamage *= _objectParamProcess.DurationHigh;
                        _logIndirectDamage1 += $" * DurationHigh";
                        _logIndirectDamage2 += $" * {_objectParamProcess.DurationHigh:F0}";
                        break;
                    case 1:
                        _indirectDamage *= _objectParamProcess.DurationMedium;
                        _logIndirectDamage1 += $" * DurationMedium";
                        _logIndirectDamage2 += $" * {_objectParamProcess.DurationMedium:F0}";
                        break;
                    case 2:
                        _indirectDamage *= _objectParamProcess.DurationLow;
                        _logIndirectDamage1 += $" * DurationLow";
                        _logIndirectDamage2 += $" * {_objectParamProcess.DurationLow:F0}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_intensityDegree), _intensityDegree, "out of range");
                }

                //Building and Special Objects
                if (_mergedObjParam.ObjectClass.ID <= 2)
                {
                    // staff property indicates if indirect damage is available
                    if (_mergedObjParam.HasProperties.Where(m => m.Property == nameof(_mergedObjParam.Staff)).Any())
                    {
                        _indirectDamage *= (double)_mergedObjParam.Staff;
                        _logIndirectDamage1 += $" * Loss/day";
                        _logIndirectDamage2 += $" * {_mergedObjParam.Staff:F0}";

                        if (_damageExtent.Clipped)
                        {
                            _indirectDamage *= _damageExtent.Part;
                            _logIndirectDamage1 += $" * PartOfLoss";
                            _logIndirectDamage2 += $" * {_damageExtent.Part:F2}";
                        }

                        if (_mergedObjParam.Staff <= 0)
                        {
                            _damageExtent.Log += $"{ResModel.DE_Staff} = {_mergedObjParam.Staff} \n";
                        }
                    }
                    else // Buildings without indirect damage
                    {
                        _indirectDamage = 0;
                        _logIndirectDamage1 = $"no indirect damage";
                        _logIndirectDamage2 = "";
                    }
                }
                //Agriculture
                else if (_mergedObjParam.ObjectClass.ID == 4)
                {
                    _indirectDamage *= _damageExtent.Area / 10000.0;   //in hectare!
                    _logIndirectDamage1 += $" * Area / 10000";
                    _logIndirectDamage2 += $" * {_damageExtent.Area / 10000.0:F3}";
                }
                //Infrastructure
                else if (_mergedObjParam.ObjectClass.ID == 3)
                {
                    // duration indicates if indirect damage is available
                    if ((_objectParamProcess.DurationHigh + _objectParamProcess.DurationMedium + _objectParamProcess.DurationLow) > 0)
                    {
                        _indirectDamage *= (double)_mergedObjParam.Staff;
                        _logIndirectDamage1 += $" * Loss/day";
                        _logIndirectDamage2 += $" * {_mergedObjParam.Staff:F0}";

                        //TODO

                    }
                    else // Infrastructure without indirect damage
                    {
                        _indirectDamage = 0;
                        _logIndirectDamage1 = $"no indirect damage";
                        _logIndirectDamage2 = "";
                    }
                }

            }
            //else //Infrastructure
            //{
            //    _indirectDamage = 0;
            //    _logIndirectDamage1 = $"no indirect damage";
            //    _logIndirectDamage2 = "";
            //}

            _damageExtent.IndirectDamage = _indirectDamage;
            _damageExtent.LogIndirectDamage = _logIndirectDamage1 + ";\n" + _logIndirectDamage2;

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            // if no errors reported
            if (String.IsNullOrWhiteSpace(_damageExtent.Log))
                _damageExtent.Log = "OK \n";

            return _damageExtent;
        }

        /// <summary>
        /// Delete all damage extents of project in the database
        /// </summary>
        /// <param name="projectId"></param>
        public void deleteDamageExtentsFromDB(int projectId)
        {
            if (projectId < 1)
                return;

            string _deleteQueryString =
                $"delete " +
                $"from \"DamageExtent\" " +
                $"where \"DamageExtent\".\"MappedObjectId\" in " +
                $"(select damageextent.\"MappedObjectId\" " +
                $"from \"DamageExtent\" as damageextent, \"Intensity\" as intensity " +
                $"where intensity.\"Project_Id\" = {projectId} " +
                $"and damageextent.\"IntensityId\" = intensity.\"ID\") ";

            using (var transaction = DBManager.ActiveSession.BeginTransaction())
            {
                ISQLQuery query = ActiveSession.CreateSQLQuery(_deleteQueryString);
                query.ExecuteUpdate();
                DBManager.ActiveSession.Flush();
                transaction.Commit();
            }

            //Change the project to state "Started"
            var _resultController = new ResultController();
            _resultController.setProjectStatus(projectId, 1);


        }

        /// <summary>
        /// Save damage extents to db
        /// </summary>
        /// <param name="damageExtents"></param>
        public void saveDamageExtentToDB(List<DamageExtent> damageExtents)
        {
            if (damageExtents == null || damageExtents.Count() == 0)
                return;

            Stopwatch _saveWatch = new Stopwatch();
            _saveWatch.Start();

            Logging.warn($" + start: elapsed time = " + _saveWatch.Elapsed.ToString());
            _saveWatch.Restart();

            using (var transaction = DBManager.ActiveSession.BeginTransaction())
            {
                Logging.warn($" + transaction start: elapsed time = " + _saveWatch.Elapsed.ToString());
                _saveWatch.Restart();

                foreach (DamageExtent item in damageExtents)
                {
                    DBManager.ActiveSession.Save(item);
                }

                Logging.warn($" + save: elapsed time = " + _saveWatch.Elapsed.ToString());
                _saveWatch.Restart();

                DBManager.ActiveSession.Flush();

                Logging.warn($" + flush: elapsed time = " + _saveWatch.Elapsed.ToString());
                _saveWatch.Restart();

                transaction.Commit();

                Logging.warn($" + commit: elapsed time = " + _saveWatch.Elapsed.ToString());
                _saveWatch.Restart();
            }
            Logging.warn($" + end: elapsed time = " + _saveWatch.Elapsed.ToString());
            _saveWatch.Stop();
        }

        /// <summary>
        /// Compute the project summary, no computation perfomed
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public ProjectResult computeProjectResult(int projectId, bool details = false)
        {
            //if (damageExtentsIn == null) //bac session
            //    DBManager.NewSession();

            // Check if project exists
            bool _projectExist = ActiveSession.QueryOver<Project>().Where(x => x.Id == projectId).RowCount() > 0;
            if (!_projectExist)
            {
                return new ProjectResult
                {
                    Project = ActiveSession.Get<Project>(projectId) ?? new Project() { Id = projectId, Name = ResResult.PRJ_NoProject },
                    Message = String.Format(ResResult.PRJ_ProjectNotFound, projectId),
                };
            }

            List<DamageExtent> _damageExtents;
            _damageExtents = ActiveSession
                                .QueryOver<DamageExtent>()
                                .JoinQueryOver(lm => lm.MappedObject).Where(d => d.Project.Id == projectId)
                                .List()
                                //.OrderBy(de=>de.MappedObject.Objectparameter.ObjectClass.ID)
                                //.OrderBy(de=>de.Intensity.ID)
                                .ToList();

            if (!_damageExtents.Any())
            {
                return new ProjectResult
                {
                    Project = ActiveSession.Get<Project>(projectId) ?? new Project() { Id = projectId, Name = ResResult.PRJ_NoProject },
                    //Message = $"\n{ResResult.PRJ_NoDamageExtent}",
                };
            }

            Project _project = _damageExtents.First().MappedObject.Project;

            var _response = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
            };

            //List<NatHazard> hazards = _damageExtents.Select(de => de.Intensity.NatHazard).Distinct().OrderBy(n => n.ID).ToList();

            IList<Intensity> _intensityListRaw = ActiveSession
                .QueryOver<Intensity>()
                .Where(i => i.Project.Id == projectId)
                .List<Intensity>();

            List<NatHazard> _hazards = _intensityListRaw.Select(i => i.NatHazard).Distinct().OrderBy(n => n.ID).ToList();

            var _projectResult = new ProjectResult()
            {
                Project = _project,
                NatHazards = _hazards,
                ShowDetails = details,
            };

            // loop over natural hazards
            foreach (NatHazard hazard in _hazards)
            {
                List<bool> beforeActions = _intensityListRaw
                    .Where(i => i.NatHazard.ID == hazard.ID)
                    .Select(i => i.BeforeAction)
                    .Distinct()
                    .OrderByDescending(a => a).ToList();

                foreach (bool beforeMeasure in beforeActions)
                {
                    List<IKClasses> ikClasses = _intensityListRaw
                        .Where(i => i.NatHazard.ID == hazard.ID)
                        .Where(i => i.BeforeAction == beforeMeasure)
                        .Select(i => i.IKClasses)
                        .Distinct()
                        .OrderBy(p => p.Value).ToList();

                    var _processResult = new ProcessResult()
                    {
                        NatHazard = hazard,
                        BeforeAction = beforeMeasure,
                    };
                    _projectResult.ProcessResults.Add(_processResult);

                    foreach (IKClasses period in ikClasses)
                    {
                        List<DamageExtent> _scenarioDamageExtents = _damageExtents
                            .Where(de => de.Intensity.BeforeAction == beforeMeasure && de.Intensity.NatHazard == hazard && de.Intensity.IKClasses == period)
                            .OrderBy(de => de.MappedObject.Objectparameter.ObjectClass.ID)
                            .ThenBy(de => de.MappedObject.ID)
                            .ToList();

                        var _scenarioResult = new ScenarioResult()
                        {
                            DamageExtents = _scenarioDamageExtents,
                            NatHazard = hazard,
                            BeforeAction = beforeMeasure,
                            IkClass = period,
                        };

                        _processResult.ScenarioResults.Add(_scenarioResult);
                    }
                }
            }

            //////////////////////////////////////////////////////////////////////////////////

            // Protection Measure
            var _protectionMeasure = _project.ProtectionMeasure;
            if (_protectionMeasure != null)
            {
                _projectResult.ProtectionMeasure = _protectionMeasure;
                if (_projectResult.ProtectionMeasure.LifeSpan < 1)
                {
                    _projectResult.Message += $"\n{String.Format(ResResult.PRJ_LifeSpanError, _projectResult.ProtectionMeasure.LifeSpan)}";
                }
            }

            // Project Summary

            // Errors Summary
            var _damageExtentErrors = _damageExtents
                .Where(de => de.Log.Trim().ToUpper() != "OK")
                .OrderBy(de => de.MappedObject.ID)
                .ThenByDescending(de => de.Intensity.BeforeAction)
                .ThenBy(de => de.Intensity.NatHazard.ID)
                .ThenBy(de => de.Intensity.IKClasses.Value)
                .ThenBy(de => de.Intensity.IntensityDegree)
                .Select(de => new DamageExtentError
                {
                    MappedObject = de.MappedObject,
                    Intensity = de.Intensity,
                    Issue = de.Log
                }
                );
            if (_damageExtentErrors.Any())
            {
                _projectResult.DamageExtentErrors = _damageExtentErrors.ToList();
            }

            return _projectResult;
        }

    }
}
