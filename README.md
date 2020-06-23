#  Umbraco 8 Back Office Single Sign-On using OpenID Connect

An example customization of Umbraco 8 using OpenID Connect to implement single-sign on for the back office. In this example, the OpenID Connect server is assumed to be https://github.com/auroris/OpenIddict-WindowsAuth.

## Rationale

I wanted to have active directory single sign-on without having to set up a service account or use active directory membership providers. This example is geared for my OpenID Connect server, but can be easily generalized for any OpenID Connect server.

## Customizations to the default Umbraco 8 installation

The App_Start folder was created and two new files were added: [IdentityServerAuthExtension.cs](https://github.com/auroris/Umbraco8SSO/blob/master/App_Start/IdentityServerAuthExtension.cs) and [UmbracoCustomOwinStartup.cs](https://github.com/auroris/Umbraco8SSO/blob/master/App_Start/UmbracoCustomOwinStartup.cs). In addition, `owin:appStartup` in [Web.config](https://github.com/auroris/Umbraco8SSO/blob/master/Web.config#L48) was modified from `UmbracoDefaultOwinStartup` to `UmbracoCustomOwinStartup`.

## Quick Start Guide

Note: This example is intended for Active Directory domain-joined computers, but should work fine if you're not.

1. Clone or download [OpenIddict-WindowsAuth](https://github.com/auroris/OpenIddict-WindowsAuth) and open it in Visual Studio 2019. Then run it. Make sure the server is working correctly by checking out the contents of the web browser it spawned.

2. If you're not on a domain-joined machine, modify Startup.cs as follows to output at least one example group name:

   Above [line 154](https://github.com/auroris/OpenIddict-WindowsAuth/blob/master/Startup.cs#L154), add something like the following:

   ```csharp 
   identity.AddClaim(ClaimTypes.Role, "TEST GROUP", Destinations.IdentityToken);
   ```

3. Clone or download this repository. Run it in Visual Studio 2019 and go through the Umbraco setup process.

4. Create a group named "TEST GROUP" in Umbraco back office and then log out.

3. When attempting to use the Umbraco back office single sign-on using Firefox you will get a username and password box requesting you log in using your Windows account credentials. If you are using Chrome, Internet Explorer or Edge, you should get past this requirement straight away without trouble.
