using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Security.Claims;
using System.Threading.Tasks;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Security;

/// <summary>
/// A class to implement OpenID Connect single-sign on with Umbraco 8's back office.
/// 
/// Make sure you install the NuGet package Microsoft.Owin.Security.OpenIDConnect v4.0.1 or better
/// </summary>
public static class OpenIDAuthConnectExtension
{
    /// <summary>
    /// Configures Umbraco to use a customized OpenID Connect authentication. This example is meant to work with https://github.com/auroris/OpenIddict-WindowsAuth
    /// which provides Windows Integrated Authentication single-sign on for an active directory domain. Active Directory groups are sent in
    /// via a role claims collection and can allow a user to be automatically assigned to equivalent Umbraco groups.
    /// </summary>
    /// <param name="app">OWIN Middleware pipeline constructor</param>
    /// <param name="authority">The location of your OpenID Connect server</param>
    /// <param name="redirectUri">The location of the Umbraco back office; where OpenID Connect will redirect to when login and logout actions are performed</param>
    /// <param name="authErrorUri">The location of the authentication error page</param>
    /// <param name="caption">Customize the text shown on the back office login screen; it will be in the format of "Login with " + caption</param>
    /// <param name="style">Umbraco button icon style</param>
    /// <param name="icon">Umbraco button icon</param>
    public static void ConfigureBackOfficeOpenIDConnectAuth(this IAppBuilder app,
        string authority, string redirectUri, string authErrorUri,
        string caption = "OpenId Connect", string style = "btn-microsoft", string icon = "fa-windows")
    {
        var identityOptions = new OpenIdConnectAuthenticationOptions
        {
            ClientId = "u-client-bo",
            SignInAsAuthenticationType = Umbraco.Core.Constants.Security.BackOfficeExternalAuthenticationType,
            Authority = authority,
            RedirectUri = redirectUri,
            PostLogoutRedirectUri = redirectUri,
            ResponseType = "code id_token token",
            Scope = "openid profile email roles",
            RequireHttpsMetadata = false,
        };
        identityOptions.ForUmbracoBackOffice(style, icon);
        identityOptions.Caption = caption;
        identityOptions.AuthenticationType = authority;

        // Auto-linking options
        var autoLinkOptions = new ExternalSignInAutoLinkOptions(
            autoLinkExternalAccount: true,
            defaultUserGroups: new string[]{ }, // declare an empty array so no groups are auto added
            defaultCulture: null);

        // This callback will occur if a user is being created and automatically linked to the
        // OpenID Connect identity. The user has not been created in Umbraco's back office
        // yet, so changes made to the user object will be persisted to the database.
        autoLinkOptions.OnAutoLinking += (BackOfficeIdentityUser user, ExternalLoginInfo info) =>
        {
            // Add user to groups as specified by OpenID Connect in the role collection
            AddGroups(user, info);

            // Specify the username; useful if you set "usernameIsEmail" to false over in umbracoSettings.config
            // Username format is expected as DOMAIN\username
            user.UserName = info.ExternalIdentity.FindFirst(ClaimTypes.WindowsAccountName).Value.Split('\\')[1];
        };

        // A callback executed every time a user authenticates using OpenID Connect.
        // Returns a boolean indicating if sign in should continue or not. Called on subsequent 
        // logins, but not on a login where a user has been created (ie, via auto-linking).
        autoLinkOptions.OnExternalLogin += (BackOfficeIdentityUser user, ExternalLoginInfo info) =>
        {
            // Ensure changes to existing user gets persisted
            user.EnableChangeTracking();

            // Add user to groups as specified by OpenID Connect in the role collection
            AddGroups(user, info);

            // Update the user's username and/or display name; useful if a person gets a name change
            // (ex, gets married, gets a promotion, etc)
            // Username format is expected as DOMAIN\username
            user.UserName = info.ExternalIdentity.FindFirst(ClaimTypes.WindowsAccountName).Value.Split('\\')[1];
            user.Name = info.ExternalIdentity.FindFirst(ClaimTypes.Name).Value;

            // Carry on with login
            return true;
        };

        // Events that can occur during the OpenID Connect protocol steps. I don't use all of them,
        // but they're here if you want to check them out.
        identityOptions.Notifications = new OpenIdConnectAuthenticationNotifications
        {
            RedirectToIdentityProvider = (notification) =>
            {
                return Task.FromResult(0);
            },
            MessageReceived = (notification) =>
            {
                return Task.FromResult(0);
            },
            SecurityTokenReceived = (notification) =>
            {
                return Task.FromResult(0);
            },
            SecurityTokenValidated = (notification) =>
            {
                return Task.FromResult(0);
            },
            AuthorizationCodeReceived = (notification) =>
            {
                // My identity server will only attach role claims if the user belongs to the active directory groups
                // I'm looking for. If there are no roles, then the user's not authorized to log into back office.
                if (notification.AuthenticationTicket.Identity.FindFirst(ClaimTypes.Role) == null)
                {
                    notification.OwinContext.Response.Redirect(authErrorUri);
                    notification.HandleResponse();
                }

                return Task.FromResult(0);
            },
            AuthenticationFailed = (notification) =>
            {
                notification.OwinContext.Response.Redirect(authErrorUri);
                notification.HandleResponse();

                return Task.FromResult(0);
            },
        };

        // Attach the auto-linking configuration to the identity server configuration
        identityOptions.SetExternalSignInAutoLinkOptions(autoLinkOptions);

        // Attach the OpeNID Connect identity server configuration to the OWIN middleware stack
        app.UseOpenIdConnectAuthentication(identityOptions);
    }

    /// <summary>
    /// Adds user to Umbraco groups assuming their name matches exactly the names
    /// present in the role collection
    /// </summary>
    /// <param name="user">Umbraco back office user to modify</param>
    /// <param name="info">External login information</param>
    private static void AddGroups(BackOfficeIdentityUser user, ExternalLoginInfo info)
    {
        // List of all groups in Umbraco
        foreach (IUserGroup group in Umbraco.Core.Composing.Current.Services.UserService.GetAllUserGroups())
        {
            // List of all active directory groups given by the OpenID Connect server as a collection
            // of role claims
            foreach (Claim role in info.ExternalIdentity.FindAll(ClaimTypes.Role))
            {
                // If the name of the active directory group is the same as an umbraco group add
                // the user to it
                if (group.Name.Equals(role.Value))
                {
                    user.AddRole(group.Alias);
                }
            }
        }
    }
}