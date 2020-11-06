using ResTB.Translation;
using ResTB.Translation.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB.Models
{
    public enum CoordinateSystem
    {
        WGS84 = 4326
    }

    [LocalizedDisplayName(nameof(Resources.Project), typeof(Resources))]
    public class Project : IComparable
    {
        [Browsable(false)]
        public int Id { get; set; }
        [LocalizedDisplayName(nameof(Resources.Project_Name), typeof(Resources))]
        [Display(Order = 1)]
        [Required]
        [Index(IsUnique = true)]
        public string Name { get; set; }
        [LocalizedDisplayName(nameof(Resources.Project_Number), typeof(Resources))]
        [Display(Order = 2)]
        [Required]
        public string Number { get; set; }
        [LocalizedDisplayName(nameof(Resources.Description), typeof(Resources))]
        [Display(Order = 3)]
        public string Description { get; set; }

        [DefaultValue(CoordinateSystem.WGS84)]
        [Browsable(false)]
        public CoordinateSystem CoordinateSystem { get; set; }

        [Browsable(false)]
        public virtual List<Intensity> Intesities { get; set; }
        [Browsable(false)]
        public virtual List<MappedObject> MappedObjects { get; set; }
        [Browsable(false)]
        public virtual List<PrA> PrAs { get; set; } = new List<PrA>();

        [Browsable(false)]
        public virtual ProtectionMeasure ProtectionMeasure { get; set; }

        [Browsable(false)]
        public virtual ProjectState ProjectState { get; set; }

        public int CompareTo(object obj)
        {
            if (typeof(Project) == obj.GetType())
            {
                return Name.CompareTo(((Project)obj).Name);
            }
            else return 1;
        }

        public override string ToString()
        {
            //TODO: Translation?
            return $"Project: {Name} / {Description} (ID: {Id})";
        }
    }
}
