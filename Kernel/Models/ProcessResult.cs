using ResTB_API.Helpers;
using ResTB_API.Models.Database.Domain;
using ResTB_API.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ResTB_API.Models
{
    public class ProcessResult
    {
        //[DisplayName("Natural Hazard")]
        [LocalizedDisplayName(nameof(ResResult.PR_NatHazard), typeof(ResResult))]
        public NatHazard NatHazard { get; set; }
        [TableIgnore]
        //[DisplayName("Before Measure")]
        [LocalizedDisplayName(nameof(ResResult.PR_BeforeAction), typeof(ResResult))]
        public bool BeforeAction { get; set; }
        [TableIgnore]
        public string BeforeActionString => BeforeAction ? ResResult.PR_beforeActionString : ResResult.PR_afterActionString;
        [TableIgnore]
        public List<ScenarioResult> ScenarioResults { get; set; } = new List<ScenarioResult>();
        //[DisplayName("Person Damage Extent")]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResResult.PR_DamageExtentPerson), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public double DamageExtentPerson => ScenarioResults.Sum(sr => sr.DamageExtentPerson);
        //[DisplayName("Deaths")]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResResult.PR_DamageExtentDeaths), typeof(ResResult))]
        public double DamageExtentDeaths => ScenarioResults.Sum(sr => sr.DamageExtentDeaths);
        //[DisplayName("Property Damage Extent")]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResResult.PR_DamageExtentProperty), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public double DamageExtentProperty => ScenarioResults.Sum(sr => sr.DamageExtentProperty);
        //[DisplayName("Indirect Damage Extent")]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResResult.PR_DamageExtentIndirect), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public double DamageExtentIndirect => ScenarioResults.Sum(sr => sr.DamageExtentIndirect);
        //[DisplayName("Total Damage Extent")]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResResult.PR_DamageExtentTotal), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public double DamageExtentTotal => DamageExtentPerson + DamageExtentProperty + DamageExtentIndirect;

        [TableIgnore]
        //[DisplayName("Return Periods")]
        [LocalizedDisplayName(nameof(ResResult.PR_ReturnPeriods), typeof(ResResult))]
        public List<IKClasses> ReturnPeriods => ScenarioResults.Select(s => s.IkClass).OrderBy(s => s.Value).ToList();

        //[DisplayName("Collective Risk - Total")]
        [LocalizedDisplayName(nameof(ResResult.PR_CollectiveRiskTotal), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_CurrencyPerYear), typeof(ResFormat))]
        public Tuple<double, string> CollectiveRiskTotal => collectiveRisk(DamageTypeEnum.Total);
        //[DisplayName("Collective Risk - Person")]
        [LocalizedDisplayName(nameof(ResResult.PR_CollectiveRiskPerson), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_CurrencyPerYear), typeof(ResFormat))]
        public Tuple<double, string> CollectiveRiskPerson => collectiveRisk(DamageTypeEnum.Person);
        //[DisplayName("Collective Risk - Property")]
        [LocalizedDisplayName(nameof(ResResult.PR_CollectiveRiskProperty), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_CurrencyPerYear), typeof(ResFormat))]
        public Tuple<double, string> CollectiveRiskProperty => collectiveRisk(DamageTypeEnum.Property);
        /// <summary>
        /// resilient indirect risk
        /// </summary>
        //[DisplayName("Collective Risk - Indirect")]
        [LocalizedDisplayName(nameof(ResResult.PR_CollectiveRiskIndirect), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_CurrencyPerYear), typeof(ResFormat))]
        public Tuple<double, string> CollectiveRiskIndirect => collectiveRisk(DamageTypeEnum.Indirect);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>collectiveRisk, calculation log</returns>
        public Tuple<double, string> collectiveRisk(DamageTypeEnum type)
        {
            List<string> _logString1 = new List<string>();
            List<string> _logString2 = new List<string>();

            double _sumRisk = 0;

            if (ReturnPeriods == null)
                return new Tuple<double, string>(0.0, "no return periods found");

            int _count = ReturnPeriods.Count();
            if (_count < 1)
                return new Tuple<double, string>(0.0, "no return periods found");

            List<double> _damageValues = new List<double>();
            switch (type)
            {
                case DamageTypeEnum.Total:
                    _damageValues = ScenarioResults.Select(s => s.DamageExtentTotal).ToList();
                    break;
                case DamageTypeEnum.Person:
                    _damageValues = ScenarioResults.Select(s => s.DamageExtentPerson).ToList();
                    break;
                case DamageTypeEnum.Property:
                    _damageValues = ScenarioResults.Select(s => s.DamageExtentProperty).ToList();
                    break;
                case DamageTypeEnum.Indirect:
                    _damageValues = ScenarioResults.Select(s => s.DamageExtentIndirect).ToList();
                    break;
            }

            for (int i = 0; i < _count - 1; i++)
            {
                double _T1 = ReturnPeriods.ElementAt(i).Value;
                double _T2 = ReturnPeriods.ElementAt(i + 1).Value;

                _sumRisk += _damageValues.ElementAt(i) * (1.0 / _T1 - 1.0 / _T2);

                _logString1.Add($" DamageExtent{_T1} * (1 / T{_T1} - 1 / T{_T2}) ");
                _logString2.Add($" {_damageValues.ElementAt(i):C} * (1 / {_T1} - 1 / {_T2}) ");
            }

            //last element
            double _T = ReturnPeriods.Last().Value;
            _sumRisk += _damageValues.ElementAt(_count - 1) * (1.0 / _T);

            _logString1.Add($" DamageExtent{_T} * (1 / T{_T}) ");
            _logString2.Add($" {_damageValues.Last():C} * (1 / {_T}) ");

            string _logCollectiveRisk = $"Collective Risk of Type {type.ToString()} = " + String.Join("+", _logString1) + "\n = " + String.Join("+", _logString2);

            return new Tuple<double, string>(_sumRisk, _logCollectiveRisk);

        }

    }
}