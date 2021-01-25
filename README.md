#  Umbraco 8 Back Office Single Sign-On using OpenID Connect

An example customization of Umbraco 8 using OpenID Connect to implement single-sign on for the back office. In this example, the OpenID Connect server is assumed to be https://github.com/auroris/OpenIddict-WindowsAuth.

## Rationale

I wanted to have active directory single sign-on without having to set up a service account or use active directory membership providers. This example is geared for my OpenID Connect server, but can be easily generalized for any OpenID Connect server.

## Customizations to the default Umbraco 8 installation

The following files were added to `App_Start`:

* OpenIDAuthConnectExtension.cs
* UmbracoCustomOwinStartup.cs 

[Web.config](https://github.com/auroris/Umbraco8SSO/blob/master/Web.config) was modified as follows:

* appSetting\owin:appStartup was modified from `UmbracoDefaultOwinStartup` to `UmbracoCustomOwinStartup`.
