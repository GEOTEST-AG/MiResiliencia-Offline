using GeoAPI.Geometries;
using ResTB_API.Helpers;
using ResTB_API.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ResTB_API.Models.Database.Domain
{
    public class ProtectionMeasure
    {
        [TableIgnore]
        public virtual int ID { get; set; }
        [LocalizedDisplayName(nameof(ResModel.PM_Description), typeof(ResModel))]
        public virtual string Description { get; set; }
        [LocalizedDisplayName(nameof(ResModel.PM_Costs), typeof(ResModel))]
        //[DisplayFormat(DataFormatString = "{0:C}")]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public virtual int Costs { get; set; }
        [LocalizedDisplayName(nameof(ResModel.PM_LifeSpan), typeof(ResModel))]
        public virtual int LifeSpan { get; set; }
        [LocalizedDisplayName(nameof(ResModel.PM_OperatingCosts), typeof(ResModel))]
        //[DisplayFormat(DataFormatString = "{0:C}/a")]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_CurrencyPerYear), typeof(ResFormat))]
        public virtual int OperatingCosts { get; set; }
        [LocalizedDisplayName(nameof(ResModel.PM_MaintenanceCosts), typeof(ResModel))]
        //[DisplayFormat(DataFormatString = "{0:C}/a")]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_CurrencyPerYear), typeof(ResFormat))]
        public virtual int MaintenanceCosts { get; set; }
        [LocalizedDisplayName(nameof(ResModel.PM_RateOfReturn), typeof(ResModel))]
        //[DisplayFormat(DataFormatString = "{0:F3}%")]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_PercentF3), typeof(ResFormat))]
        public virtual double RateOfReturn { get; set; }
        [LocalizedDisplayName(nameof(ResModel.PM_ValueAddedTax), typeof(ResModel))]
        //[DisplayFormat(DataFormatString = "{0:F3}%")]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_PercentF3), typeof(ResFormat))]
        [TableIgnore]
        public virtual double ValueAddedTax { get; set; }
        //[TableIgnore]
        //public virtual IMultiPolygon geometry { get; set; }
        [TableIgnore]
        public virtual Project Project { get; set; }

        //not in db
        [LocalizedDisplayName(nameof(ResModel.PM_YearlyCosts), typeof(ResModel))]
        //[DisplayFormat(DataFormatString = "{0:C}/a")]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_CurrencyPerYear), typeof(ResFormat))]
        public virtual double YearlyCosts
        {
            get
            {
                if (LifeSpan < 1)
                {
                    return 0.0d;
                }

                double _result = (1.0d + ValueAddedTax / 100.0d) * (
                    (double)OperatingCosts +
                    (double)MaintenanceCosts +
                    ((double)Costs - 0.0d) / (double)LifeSpan +
                    ((double)Costs + 0.0d) * RateOfReturn / 2.0d / 100.0d
                    );

                return _result;
            }
        }

        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.PM_LogYearlyCosts), typeof(ResModel))]
        public virtual string LogYearlyCosts
        {
            get
            {
                string _result =
                    $"YearlyCosts = (1 + ValueAddedTax / 100) * [" +
                    $"OperatingCosts + " +
                    $"MaintenanceCosts + " +
                    $"(ConstructionCosts - 0) / LifeSpan + " +
                    $"(ConstructionCosts + 0) * RateOfReturn / 2 / 100 ] ; \n";

                _result +=
                    $"YearlyCosts = (1 + {ValueAddedTax:F3} / 100) * [" +
                    $"{(double)OperatingCosts:F0} + " +
                    $"{(double)MaintenanceCosts:F0} + " +
                    $"({(double)Costs:F0} - 0) / {(double)LifeSpan:F0} + " +
                    $"({(double)Costs:F0} + 0) * {RateOfReturn:F3} / 2 / 100 ] ";
                ;

                if (LifeSpan < 1)
                {
                    _result += $"\nERROR: LifeSpan = {LifeSpan} years";
                }

                return _result;
            }
        }
    }
}