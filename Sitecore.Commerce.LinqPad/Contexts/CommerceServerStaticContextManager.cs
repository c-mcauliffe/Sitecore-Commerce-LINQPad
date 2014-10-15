//-----------------------------------------------------------------------
// <summary>Defines the CommerceServerStaticContextManager class.</summary>
//-----------------------------------------------------------------------
namespace Sitecore.Commerce.LINQPad.Contexts
{
    using CommerceServer.Core.Runtime.Caching;
    using global::CommerceServer.Core.Catalog;
    using global::CommerceServer.Core.Orders;
    using global::CommerceServer.Core.Runtime.Configuration;
    using global::CommerceServer.Core.Runtime.Diagnostics;
    using global::CommerceServer.Core.Runtime.Orders;
    using global::CommerceServer.Core.Runtime.Pipelines;
    using global::CommerceServer.Core.Runtime.Profiles;
    using Sitecore.Commerce.Connect.CommerceServer;
    using System;
    using System.Xml.Linq;
    using System.Linq;

    /// <summary>
    /// Creates Commerce Server contexts without using Http Modules
    /// </summary>
    public class CommerceServerStaticContextManager : ICommerceServerContextManager, IDisposable
    {
        private static bool _catalogCacheEnabled = false;
        private static CatalogContext _catalogContext;
        private static string _siteName;

        private ProfileContext _profileContext;
        private OrderContext _orderContext;
        private PipelineCollection _pipelineCollection;
        private OrderManagementContext _orderManagementContext;

        ~CommerceServerStaticContextManager()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not Commerce Server has been initialized
        /// </summary>
        public bool IsCommerceServerInitialized
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets a value indicating the name of the current Commerce Server site
        /// </summary>
        public string SiteName
        {
            get
            {
                if (!string.IsNullOrEmpty(_siteName))
                {
                    return _siteName;
                }

                var xDoc = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                _siteName = xDoc.Descendants("CommerceServer").First().Descendants("application").First().Attribute("siteName").Value;

                return _siteName;
            }
        }

        /// <summary>
        /// Gets the Catalog Context
        /// </summary>
        public CatalogContext CatalogContext
        {
            get
            {
                if (_catalogContext == null)
                {
                    _catalogContext = this.InitializeCatalogContext();
                }

                return _catalogContext;
            }
        }

        /// <summary>
        /// Gets the Profile Context
        /// </summary>
        public ProfileContext ProfileContext
        {
            get
            {
                if (this._profileContext == null)
                {
                    var resources = new CommerceResourceCollection(this.SiteName);
                    var profilesResource = resources["Biz Data Service"];

                    if (profilesResource != null)
                    {
                        string profileConnectionString = profilesResource["s_ProfileServiceConnectionString"].ToString();
                        string providerConnectionString = profilesResource["s_CommerceProviderConnectionString"].ToString();
                        string BDAOConnectionString = profilesResource["s_BizDataStoreConnectionString"].ToString();

                        if (!string.IsNullOrWhiteSpace(profileConnectionString) && !string.IsNullOrWhiteSpace(providerConnectionString) && !string.IsNullOrWhiteSpace(BDAOConnectionString))
                        {
                            this._profileContext = new ProfileContext(profileConnectionString, providerConnectionString, BDAOConnectionString, new ConsoleDebugContext(DebugMode.Debug));
                        }
                    }
                }

                return this._profileContext;
            }
        }

        /// <summary>
        /// Gets the Order Context
        /// </summary>
        public OrderContext OrderContext
        {
            get
            {
                if (this._orderContext == null)
                {
                    this._orderContext = this.InitializeOrderContext();
                }

                return this._orderContext;
            }
        }

        /// <summary>
        /// Gets the Order Management Context
        /// </summary>
        public OrderManagementContext OrderManagementContext
        {
            get
            {
                if (_orderManagementContext == null)
                {
                    OrderSiteAgent agent = new OrderSiteAgent(this.SiteName);
                    this._orderManagementContext = OrderManagementContext.Create(agent);
                }

                return this._orderManagementContext;
            }
        }

        /// <summary>
        /// Gets the Caches
        /// </summary>
        public CommerceCacheCollection Caches
        {
            get { return new CommerceServer.Core.Runtime.Caching.CommerceCacheCollection(); }
        }

        /// <summary>
        /// Gets the Collection of pipelines in the site
        /// </summary>
        public PipelineCollection Pipelines
        {
            get
            {
                if (this._pipelineCollection == null)
                {
                    this._pipelineCollection = this.InitializePipelines();
                }

                return this._pipelineCollection;
            }
        }

        /// <summary>
        /// Dispose of the class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Overridable method for disposing of objects in the class
        /// </summary>
        /// <param name="disposing">Whether or not the class is already disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (this._profileContext != null)
                {
                    this._profileContext.Dispose();
                    this._profileContext = null;
                }
            }
        }

        /// <summary>
        /// Creates an instance of a Catalog Context
        /// </summary>
        /// <returns>Commerce Server Catalog Context</returns>
        private CatalogContext InitializeCatalogContext()
        {
            var catalogSiteAgent = new CatalogSiteAgent();

            catalogSiteAgent.AutoUpdateFullTextTables = false;
            catalogSiteAgent.SiteName = this.SiteName;

            var cacheConfiguration = new CommerceServer.Core.Catalog.CacheConfiguration();
            cacheConfiguration.CacheEnabled = _catalogCacheEnabled;

            CatalogContext catalogContext = CatalogContext.Create(catalogSiteAgent, cacheConfiguration);

            return catalogContext;
        }

        /// <summary>
        /// Creates an instance of a Order Context
        /// </summary>
        /// <returns>Commerce Server Order Context</returns>
        private OrderContext InitializeOrderContext()
        {
            return OrderContext.Create(this.SiteName);
        }

        /// <summary>
        /// Returns a list of all of the order pipelines in the site
        /// </summary>
        /// <returns>A collection of pipelines</returns>
        private PipelineCollection InitializePipelines()
        {
            PipelineCollection coll = new PipelineCollection();

            var xDoc = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            var orderPipelineConfigs = xDoc.Descendants("CommerceServer").First().Descendants("pipelines").First().Descendants("pipeline").Where(n => n.Attribute("type").Value == "OrderPipeline");

            foreach (var orderPipelineConfig in orderPipelineConfigs)
            {
                var name = orderPipelineConfig.Attribute("name").Value;
                var path = orderPipelineConfig.Attribute("path").Value.Replace(@"\", "/");

                var orderPipeline = new OrderPipeline(name, path, false, "pipelines/" + name + ".log", false);
                coll.Add(name, orderPipeline);
            }

            return coll;
        }
    }
}
