using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Translation
{
    //SOURCE:
    // https://brianlagunas.com/localize-property-names-descriptions-and-categories-for-the-xampropertygrid/

    /// <summary>
    /// Localization for DisplayNameyAttributes
    /// </summary>
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        readonly ResourceManager _resourceManager;
        readonly string _resourceKey;

        /// <summary>
        /// usage: [LocalizedDisplayName(nameof(Resources.Entry), typeof(Resources))]
        /// </summary>
        public LocalizedDisplayNameAttribute(string resourceKey, Type resourceType)
        {
            _resourceManager = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        public override string DisplayName
        {
            get
            {
                string displayName = _resourceManager.GetString(_resourceKey);
                return string.IsNullOrWhiteSpace(displayName) ? string.Format("[[{0}]]", _resourceKey) : displayName;
            }
        }
    }
}

