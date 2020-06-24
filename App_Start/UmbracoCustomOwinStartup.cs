using Microsoft.Owin;
using Owin;
using Umbraco.Web;

[assembly: OwinStartup("UmbracoCustomOwinStartup", typeof(UmbracoCustomOwinStartup))]

/// <summary>
/// A class to insert our own custom authentication routine into Umbraco's back-office authentication
/// 
/// To use this startup class, open web.config, locate the key "Configuration\appSetings\owin:appStartup"
/// change value from UmbracoDefaultOwinStartup to UmbracoCustomOwinStartup
/// 
/// If you get an Owin startup error, check to make sure the build action for this file is
/// 'compile' and not 'content'
/// </summary>
public class UmbracoCustomOwinStartup : UmbracoDefaultOwinStartup
{
    protected override void ConfigureUmbracoAuthentication(IAppBuilder app)
    {
        base.ConfigureUmbracoAuthentication(app);
        app.ConfigureBackOfficeOpenIDConnectAuth(
            "http://localhost:5000/", // Location of the OpenIDConnect server
            "http://localhost:61299/umbraco/", // Location of the back office
            "/AuthenticationError", // Location of the error page
            "Forces" // Login caption text
            );
    }
}