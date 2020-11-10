using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SampleApi
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    public class Startup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices( IServiceCollection services )
        {
            services.AddControllers( );

            services.AddAuthorization( );

            // nuget: IdentityServer4.AccessTokenValidation
            services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme )
                    .AddIdentityServerAuthentication( options =>
                    {
                        options.Authority = "http://localhost:5000";    // ע: ͨ��localhost��ȡ��token, ��Ҫ��Ӧ�����localhost. �����ͨ��127.0.0.1��ȡ��token, �������ip����127.0.0.1(����ᱨ��: Bearer was challenged.)
                        options.RequireHttpsMetadata = false;
                        options.ApiName = "api";
                        options.ApiSecret = "api.secret";
                    } );
        }

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if ( env.IsDevelopment( ) )
            {
                app.UseDeveloperExceptionPage( );
            }

            app.UseHttpsRedirection( );

            app.UseRouting( );
            app.UseAuthentication( );
            app.UseAuthorization( );

            app.UseEndpoints( endpoints =>
            {
                endpoints.MapControllers( );
            } );
        }
    }
}
