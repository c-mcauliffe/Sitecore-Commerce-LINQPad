//-----------------------------------------------------------------------
// <copyright file="CommerceServerConfigReader.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2014
// </copyright>
// <summary>Defines the CommerceServerConfigReader class.</summary>
//-----------------------------------------------------------------------
namespace Sitecore.Commerce.LinqPad.Config
{
    using Sitecore.Commerce.LinqPad.Contexts;
    using Sitecore.Linqpad.Config;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    public class CommerceServerConfigReader : Sitecore7AppConfigReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="driverConfig"></param>
        protected override void Transform(System.Xml.Linq.XDocument config, Sitecore.Linqpad.Driver.ISitecoreDriverSettings driverConfig)
        {
            base.Transform(config, driverConfig);
            var contextConfig = config.Descendants("commerceServer").First()
                .Descendants("types").First()
                .Descendants("type")
                .Where(t => t.Attribute("name").Value == "ICommerceServerContextManager")
                .FirstOrDefault();

            contextConfig.Attribute("type").Value = typeof(CommerceServerStaticContextManager).AssemblyQualifiedName;
        }
    }
}
