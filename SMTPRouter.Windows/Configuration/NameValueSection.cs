using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Windows.Configuration
{
    /// <summary>
    /// Represents a <see cref="ConfigurationSection"/> that contains a Name and Value
    /// </summary>
    public sealed class NameValueSection: ConfigurationSection
    {
        /// <summary>
        /// The Names and Values inside the <see cref="NameValueSection"/>
        /// </summary>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public NameValueConfigurationCollection Settings
        {
            get
            {
                return (NameValueConfigurationCollection)base[""];
            }
        }
    }
}
