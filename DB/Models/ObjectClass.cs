using ResTB.Translation;
using ResTB.Translation.Properties;
using System.ComponentModel;

namespace ResTB.DB.Models
{
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
