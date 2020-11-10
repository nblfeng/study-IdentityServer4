// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace AuthCenter
{
    using AuthCenter.DAL;

    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        private readonly IFreeSql _fsql;

        public Startup( IWebHostEnvironment environment, IConfiguration configuration )
        {
            Environment = environment;
            Configuration = configuration;

            _fsql = new FreeSql.FreeSqlBuilder( )
                               .UseConnectionString( FreeSql.DataType.Sqlite, Configuration.GetConnectionString( "AuthContext" ) )
                               .UseAutoSyncStructure( false )
                               .UseMonitorCommand( cmd => Console.WriteLine( $"{System.Environment.NewLine}{cmd.CommandText}" ) )
                               .Build( );
        }

        public void ConfigureServices( IServiceCollection services )
        {
            services.AddSingleton( _fsql );
            services.AddFreeDbContext<AuthContext>( options => options.UseFreeSql( _fsql ) );

            services.AddControllersWithViews( );

            var builder = services
                            .AddIdentityServer( options =>
                            {
                                options.Events.RaiseErrorEvents = true;
                                options.Events.RaiseInformationEvents = true;
                                options.Events.RaiseFailureEvents = true;
                                options.Events.RaiseSuccessEvents = true;

                                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                                options.EmitStaticAudienceClaim = true;

                                options.UserInteraction.LoginUrl = "/oauth2/authorize";
                            } )
                            .AddResourceOwnerValidator<Ids4.ResourceOwnerPasswordValidator>( );
            //.AddTestUsers( TestUsers.Users );

            // in-memory, code config
            builder.AddInMemoryIdentityResources( Config.IdentityResources );
            builder.AddInMemoryApiScopes( Config.ApiScopes );
            builder.AddInMemoryApiResources( Config.ApiResources );
            builder.AddInMemoryClients( Config.Clients );

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential( );

            services.AddAuthentication( )
                .AddGoogle( options =>
                 {
                     options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                     // register your IdentityServer with Google at https://console.developers.google.com
                     // enable the Google+ API
                     // set the redirect URI to https://localhost:5001/signin-google
                     options.ClientId = "copy client ID from Google here";
                     options.ClientSecret = "copy client secret from Google here";
                 } );

            services.Configure<CookiePolicyOptions>( options =>
            {
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            } );
        }

        public void Configure( IApplicationBuilder app )
        {
            if ( Environment.IsDevelopment( ) )
            {
                app.UseDeveloperExceptionPage( );
            }

            app.UseStaticFiles( );

            app.UseCookiePolicy( );
            app.UseRouting( );
            app.UseIdentityServer( );
            app.UseAuthorization( );
            app.UseEndpoints( endpoints =>
             {
                 endpoints.MapDefaultControllerRoute( );
             } );
        }
    }
}