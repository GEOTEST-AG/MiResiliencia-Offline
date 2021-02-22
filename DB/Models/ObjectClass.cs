using ResTB.Translation;
using ResTB.Translation.Properties;
using System.ComponentModel;

namespace ResTB.DB.Models
{
    /// <summary>
    /// geometry feature type
    /// </summary>
    public enum FeatureType
    {
        [LocalizedDescription(nameof(Resources.Point), typeof(Resources))]
        Point = 0,
        [LocalizedDescription(nameof(Resources.Line), typeof(Resources))]
        Line = 1,
        [LocalizedDescription(nameof(Resources.Polygon), typeof(Resources))]
        Polygon = 2,
        Any = 3
    }

    /// <summary>
    /// object class, language dependent!
    /// </summary>
    public class ObjectClass
    {
        [ReadOnly(true)]
        public int ID { get; set; }
        [ReadOnly(true)]
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
