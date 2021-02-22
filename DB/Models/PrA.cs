using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ResTB.DB.Models
{
    /// <summary>
    /// spatial probability of occurrence (räumliche Auftretenswahrscheinlichkeit), project dependent
    /// <para/>
    /// so far, only project related copies of Standard_PrA
    /// </summary>
    public class PrA
    {
        [Key, Column(Order = 0)]
        public int NatHazardId { get; set; }
        [Key, Column(Order = 1)]
        public int IKClassesId { get; set; }
        [Key, Column(Order = 2)]
        public int ProjectId { get; set; }
        public NatHazard NatHazard { get; set; }
        public IKClasses IKClasses { get; set; }
        public Project Project { get; set; }
        public double Value { get; set; }

        public object Clone()
        {
            PrA pra = new PrA();
            pra.NatHazardId = NatHazardId;
            pra.NatHazard = NatHazard;
            pra.IKClassesId = IKClassesId;
            pra.IKClasses = IKClasses;
            pra.ProjectId = ProjectId;
            pra.Project = Project;
            pra.Value = Value;
            return pra;
        }

    }
}