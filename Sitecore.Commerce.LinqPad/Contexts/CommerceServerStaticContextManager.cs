//-----------------------------------------------------------------------
// <copyright file="CommerceServerStaticContextManager.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2014
// </copyright>
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

    /// <summary>
    /// 
    /// </summary>
    public class CommerceServerStaticContextManager : ICommerceServerContextManager
    {
        private static bool _catalogCacheEnabled = false;
        private static CatalogContext _catalogContext;

        private ProfileContext _profileContext;
        private OrderContext _orderContext;
        private PipelineCollection _pipelineCollection;
        private OrderManagementContext _orderManagementContext;

        public bool IsCommerceServerInitialized
        {
            get { return true; }
        }

        public string SiteName
        {
            get { return "SolutionStorefrontSite"; }
        }

        public static bool CatalogCacheEnabled
        {
            get
            {
                return _catalogCacheEnabled;
            }
            set
            {
                if (_catalogCacheEnabled != value)
                {
                    _catalogCacheEnabled = value;
                    _catalogContext = null;
                }
            }
        }

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

        public CommerceCacheCollection Caches
        {
            get { return new CommerceServer.Core.Runtime.Caching.CommerceCacheCollection(); }
        }

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

        private OrderContext InitializeOrderContext()
        {
            return OrderContext.Create(this.SiteName);
        }

        private PipelineCollection InitializePipelines()
        {
            PipelineCollection coll = new PipelineCollection();

            PipelineBase basketPipeline = new OrderPipeline("basket", "pipelines/basket.pcf", false, "pipelines/basket.log", false);
            PipelineBase checkoutPipeline = new OrderPipeline("checkout", "pipelines/checkout.pcf", false, "pipelines/checkout.log", false);
            PipelineBase totalPipeline = new OrderPipeline("total", "pipelines/total.pcf", false, "pipelines/total.log", false);
            PipelineBase creditcardPipeline = new OrderPipeline("creditcard", "pipelines/creditcard.pcf", false, "pipelines/creditcard.log", false);

            coll.Add("basket", basketPipeline);
            coll.Add("checkout", checkoutPipeline);
            coll.Add("total", totalPipeline);
            coll.Add("creditcard", creditcardPipeline);

            return coll;
        }
    }
}
