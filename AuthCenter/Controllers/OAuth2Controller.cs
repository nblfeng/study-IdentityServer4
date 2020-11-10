using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AuthCenter.Controllers
{
    using AuthCenter.DAL;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Events;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using IdentityServerHost.Quickstart.UI;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;

    public partial class OAuth2Controller : Controller
    {
        private readonly AuthContext _db;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IClientStore _clientStore;
        private readonly IEventService _eventService;

        public OAuth2Controller( DAL.AuthContext db,
            IIdentityServerInteractionService interaction,
            IAuthenticationSchemeProvider schemeProvider,
            IClientStore clientStore,
            IEventService eventService )
        {
            _db = db;
            _interaction = interaction;
            _schemeProvider = schemeProvider;
            _clientStore = clientStore;
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> Authorize( string returnUrl )
        {
            var vm = await BuildLoginViewModelAsync( returnUrl );

            if ( vm.IsExternalLoginOnly )
            {
                return RedirectToAction( "Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl } );
            }

            return View( vm );
        }

        [HttpPost]
        public async Task<IActionResult> Authorize( LoginInputModel model, string button )
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync( model.ReturnUrl );

            // the user clicked the "cancel" button
            if ( button != "login" )
            {
                if ( context != null )
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.DenyAuthorizationAsync( context, AuthorizationError.AccessDenied );

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if ( context.IsNativeClient( ) )
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage( "Redirect", model.ReturnUrl );
                    }

                    return Redirect( model.ReturnUrl );
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect( "~/" );
                }
            }

            if ( ModelState.IsValid )
            {
                var user = await _db.T_Users.Where( i => i.Account == model.Username && i.Password == model.Password ).ToOneAsync( );
                if ( user != null )
                {
                    await _eventService.RaiseAsync( new UserLoginSuccessEvent( user.Account, user.Id, user.Account, clientId: context?.Client.ClientId ) );

                    AuthenticationProperties properties = null;
                    if ( model.RememberLogin )
                    {
                        properties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add( TimeSpan.FromDays( 7 ) )
                        };
                    }

                    // issue authentication cookie with subject ID and username
                    var issuer = new IdentityServerUser( user.Id )
                    {
                        DisplayName = user.Name,
                        AdditionalClaims = new Claim[]
                        {
                            new Claim( "account", user.Account ),
                            new Claim( "authority", user.Authority ?? "" ),
                            new Claim( JwtClaimTypes.Role, user.IsAdmin ? "admin": "user" ),
                        }
                    };
                    await HttpContext.SignInAsync( issuer, properties );

                    // 页面跳转
                    if ( context != null )
                    {
                        if ( context.IsNativeClient( ) )
                        {
                            // The client is native, so this change in how to
                            // return the response is for better UX for the end user.
                            return this.LoadingPage( "Redirect", model.ReturnUrl );
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect( model.ReturnUrl );
                    }

                    // request for a local page
                    if ( Url.IsLocalUrl( model.ReturnUrl ) )
                    {
                        return Redirect( model.ReturnUrl );
                    }
                    else if ( string.IsNullOrWhiteSpace( model.ReturnUrl ) )
                    {
                        return Redirect( "~/" );
                    }
                    else
                    {
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception( "invalid return URL" );
                    }
                }

                await _eventService.RaiseAsync( new UserLoginFailureEvent( model.Username, "invalid credentials", clientId: context?.Client.ClientId ) );
                ModelState.AddModelError( string.Empty, AccountOptions.InvalidCredentialsErrorMessage );
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync( model );
            return View( vm );
        }
    }

    partial class OAuth2Controller
    {
        private async Task<LoginViewModel> BuildLoginViewModelAsync( string returnUrl )
        {
            var context = await _interaction.GetAuthorizationContextAsync( returnUrl );
            if ( context?.IdP != null && await _schemeProvider.GetSchemeAsync( context.IdP ) != null )
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if ( !local )
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync( );

            var providers = schemes
                .Where( x => x.DisplayName != null )
                .Select( x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                } ).ToList( );

            var allowLocal = true;
            if ( context?.Client.ClientId != null )
            {
                var client = await _clientStore.FindEnabledClientByIdAsync( context.Client.ClientId );
                if ( client != null )
                {
                    allowLocal = client.EnableLocalLogin;

                    if ( client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any( ) )
                    {
                        providers = providers.Where( provider => client.IdentityProviderRestrictions.Contains( provider.AuthenticationScheme ) ).ToList( );
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray( )
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync( LoginInputModel model )
        {
            var vm = await BuildLoginViewModelAsync( model.ReturnUrl );
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }
    }
}
