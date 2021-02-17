using ResTB.Translation;
using ResTB.Translation.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ResTB.DB.Models
{
    /// <summary>
    /// hazard map for a certain NatHazard and of a certain hazard index
    /// </summary>
    public class HazardMap
    {
        //[ReadOnly(true)]
        [Browsable(false)]
        public int ID { get; set; }
        //[ReadOnly(true)]
        [Browsable(false)]
        public Project Project { get; set; }
        [LocalizedDisplayName(nameof(Resources.NatHazard), typeof(Resources))]
        [ReadOnly(true)]
        [Display(Order = 1)]
        public NatHazard NatHazard { get; set; }
        /// <summary>
        /// hazard index, range 1..9
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.HazardIndex), typeof(Resources))]
        [Range(1, 9)]
        [Display(Order = 3)]
        public int Index { get; set; }
        /// <summary>
        /// before mitigation measure?
        /// </summary>
        [Browsable(false)]
        [ReadOnly(true)]
        public bool BeforeAction { get; set; }
        /// <summary>
        /// not in use: DamageExtents are linked to intensities, not to hazard maps
        /// </summary>
        [Browsable(false)]
        public virtual List<DamageExtent> DamageExtents { get; set; }

        public override string ToString()
        {
            //TODO: Translation?
            return $"Hazard Map: {NatHazard} / before:{BeforeAction} / index:{Index}";
        }
    }
}
