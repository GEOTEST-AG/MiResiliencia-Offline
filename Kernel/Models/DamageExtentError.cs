using ResTB_API.Helpers;
using ResTB_API.Models.Database.Domain;
using ResTB_API.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Models
{
    public class DamageExtentError
    {
        [LocalizedDisplayName(nameof(ResModel.DE_MappedObject), typeof(ResModel))]
        public MappedObject MappedObject { get; set; }
        [LocalizedDisplayName(nameof(ResModel.DE_Intensity), typeof(ResModel))]
        public Intensity Intensity { get; set; }
        [LocalizedDisplayName(nameof(ResModel.DE_ProblemDescription), typeof(ResModel))]
        public string Issue { get; set; }
    }
}