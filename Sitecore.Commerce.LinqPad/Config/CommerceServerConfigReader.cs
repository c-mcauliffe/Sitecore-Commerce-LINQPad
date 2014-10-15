//-----------------------------------------------------------------------
// <summary>Defines the CommerceServerConfigReader class.</summary>
//-----------------------------------------------------------------------
namespace Sitecore.Commerce.LINQPad.Config
{
    using Sitecore.Commerce.LINQPad.Contexts;
    using Sitecore.Linqpad.Config;
    using System.Linq;

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
        protected override void Transform(System.Xml.Linq.XDocument config, Sitecore.Linqpad.Driver.ISitecoreDriverSettings driverConfig)
        {
            base.Transform(config, driverConfig);

            // update the Commerce Server context config to use and instance that can create contexts without using Http Modules
            var contextConfig = config.Descendants("commerceServer").First()
                .Descendants("types").First()
                .Descendants("type")
                .Where(t => t.Attribute("name").Value == "ICommerceServerContextManager")
                .FirstOrDefault();

            contextConfig.Attribute("type").Value = typeof(CommerceServerStaticContextManager).AssemblyQualifiedName;
        }
    }
}
