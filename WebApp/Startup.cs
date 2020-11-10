using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApp
{
    using IdentityModel;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using System.IdentityModel.Tokens.Jwt;

    public class Startup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices( IServiceCollection services )
        {
            services.AddControllersWithViews( );

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication( options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    } )
                    .AddCookie( CookieAuthenticationDefaults.AuthenticationScheme )
                    .AddOpenIdConnect( OpenIdConnectDefaults.AuthenticationScheme, options =>
                    {
                        options.Authority = "http://localhost:5000";
                        options.RequireHttpsMetadata = false;

                        options.ClientId = "web.client";
                        options.ClientSecret = "web.client.secret";
                        options.ResponseType = OpenIdConnectResponseType.Code;
                        options.SaveTokens = true;

                        options.Scope.Clear( );
                        options.Scope.Add( OidcConstants.StandardScopes.OpenId );
                        options.Scope.Add( OidcConstants.StandardScopes.Profile );
                        options.Scope.Add( "account" );
                        options.Scope.Add( "authority" );
                        options.Scope.Add( "role" );

                        options.Scope.Add( "scope1" );
                    } );

            services.Configure<CookiePolicyOptions>( options =>
            {
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            } );
        }

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if ( env.IsDevelopment( ) )
            {
                app.UseDeveloperExceptionPage( );
            }
            else
            {
                app.UseExceptionHandler( "/Home/Error" );
            }
            app.UseStaticFiles( );

            app.UseCookiePolicy( );
            app.UseRouting( );
            app.UseAuthentication( );
            app.UseAuthorization( );

            app.UseEndpoints( endpoints =>
             {
                 endpoints.MapControllerRoute(
                     name: "default",
                     pattern: "{controller=Home}/{action=Index}/{id?}" );
             } );
        }
    }
}
