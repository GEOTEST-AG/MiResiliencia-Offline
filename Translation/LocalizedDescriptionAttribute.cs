using System;
using System.ComponentModel;
using System.Resources;

namespace ResTB.Translation
{
    //SOURCE:
    // https://brianlagunas.com/localize-property-names-descriptions-and-categories-for-the-xampropertygrid/

    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        readonly ResourceManager _resourceManager;
        readonly string _resourceKey;

        /// <summary>
        /// usage: [LocalizedDescription(nameof(Resources.Entry), typeof(Resources))]
        /// </summary>
        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        {
            _resourceManager = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string description = _resourceManager.GetString(_resourceKey);
                return string.IsNullOrWhiteSpace(description) ? string.Format("[[{0}]]", _resourceKey) : description;
            }
        }
    }

}