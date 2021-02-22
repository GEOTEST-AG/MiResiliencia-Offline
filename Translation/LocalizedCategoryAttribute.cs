using System;
using System.ComponentModel;
using System.Resources;

namespace ResTB.Translation
{
    //SOURCE:
    // https://brianlagunas.com/localize-property-names-descriptions-and-categories-for-the-xampropertygrid/

    /// <summary>
    /// Localization for CategoryAttributes
    /// </summary>
    public class LocalizedCategoryAttribute : CategoryAttribute
    {
        readonly ResourceManager _resourceManager;
        readonly string _resourceKey;

        /// <summary>
        /// [LocalizedCategory(nameof(Resources.Entry), typeof(Resources))]
        /// </summary>
        public LocalizedCategoryAttribute(string resourceKey, Type resourceType)
        {
            _resourceManager = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        protected override string GetLocalizedString(string value)
        {
            string category = _resourceManager.GetString(_resourceKey);
            return string.IsNullOrWhiteSpace(category) ? string.Format("[[{0}]]", _resourceKey) : category;
        }
    }
}
