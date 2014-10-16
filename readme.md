# Sitecore Commerce LINQPad
---------------------------------------------

The [Sitecore LINQPad Driver from Adam Conn](http://www.sitecore.net/Learn/Blogs/Search.aspx?q=linqpad) makes it really easy to use LINQPad to query against Sitecore. The goal of Sitecore Commerce LINQPad is to add some extensions to this driver to make sure Sitecore Commerce data works as expected.

#### Functionality

CRUD on Commerce Server Items

| Without extension  | With extension  |
|---|---|
| ![Fail](/doc/images/CS_Fail.png)  | ![Success](/doc/images/CS_Success.png)  |

On a regular Sitecore Commerce Server installation all of the Commerce Server sub system contexts are created by HTTP modules. When you run queries from LINQPad the HTTP modules are never run so all of the Commerce Server contexts remain null and nothing works. The **CommerceServerConfigReader** class fixes this by updating the Sitecore configuration with a custom implementation of **ICommerceServerContextManager** which creates the Commerce Server sub system contexts statically so that everything will work when calls are made out side of the web.

#### Setup
##### Get
Sitecore.Commerce.LINQPad.dll is distributed via a [NuGet package](https://www.nuget.org/packages/Sitecore.Commerce.LINQPad/) or a Sitecore package which you can get from the [Releases tab](https://github.com/c-mcauliffe/Sitecore-Commerce-LINQPad/releases) or the [Sitecore Marketplace](https://marketplace.sitecore.net/Modules/Sitecore_Commerce_LINQPad.aspx)

##### Install
If you choose the NuGet package route then you can install it via Sitecore Rocks, see the links below if you are unsure how to do this. If you choose the Sitecore package route then you can just use the regular Sitecore Install Wizard on the site you want to target with LINQPad.

##### Configure
1. Setup a connection with the Sitecore LINQPad Driver ![Step 1](/doc/images/Step01.png)
2. Right click on the connection and then click **Properties** ![Step 2](/doc/images/Step02.png)
3. Click on the **Advanced** tab, then click on the **Browse** link to the right of the **App.config reader** label ![Step 3](/doc/images/Step03.png)
4. Browse and select the **Sitecore.Commerce.LINQPad.dll** from your site, select the **CommerceServerConfigReader** class, then click **OK** ![Step 4](/doc/images/Step04.png)
5. Make sure the **App.config** text box is populated with the class, then click **OK** ![Step 5](/doc/images/Step05.png)


#### Useful Links

[Sitecore LINQPad Driver from Adam Conn](http://www.sitecore.net/Learn/Blogs/Search.aspx?q=linqpad)

[Installing and Using LINQPad Driver for Sitecore](https://www.youtube.com/watch?v=ucifA0eGzEA)

[Sitecore.NuGet](http://vsplugins.sitecore.net/Sitecore-NuGet.ashx)

[Create and Deploy Sitecore Modules with NuGet]
(http://www.velir.com/blog/index.php/2012/12/04/create-and-deploy-sitecore-modules-with-nuget/)
