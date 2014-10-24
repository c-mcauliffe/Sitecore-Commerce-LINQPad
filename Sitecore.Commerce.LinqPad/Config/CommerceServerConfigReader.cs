//-----------------------------------------------------------------------
// <summary>Defines the CommerceServerConfigReader class.</summary>
//-----------------------------------------------------------------------
namespace Sitecore.Commerce.LINQPad.Config
{
    using Sitecore.Commerce.LINQPad.Contexts;
    using Sitecore.Linqpad.AppConfigReaders;
    using Sitecore.Linqpad.Models;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Extends the defult Sitecore LINQPad Driver config reader and updates some Commerce Specific settings
    /// </summary>
    public class CommerceServerConfigReader : Sitecore7AppConfigReader
    {
        /// <summary>
        /// Called when the Sitecore config is being transformed.
        /// </summary>
        /// <param name="config">The Sitecore config xml</param>
        /// <param name="driverConfig">The current driver configurations</param>
        protected override void Transform(XDocument document, SitecoreDriverSettings driverSettings)
        {
            base.Transform(document, driverSettings);

            // update the Commerce Server context config to use and instance that can create contexts without using Http Modules
            var contextConfig = document.Descendants("commerceServer").First()
                .Descendants("types").First()
                .Descendants("type")
                .Where(t => t.Attribute("name").Value == "ICommerceServerContextManager")
                .FirstOrDefault();

            contextConfig.Attribute("type").Value = typeof(CommerceServerStaticContextManager).AssemblyQualifiedName;
        }
    }
}
