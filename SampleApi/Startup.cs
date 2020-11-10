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
                        options.Authority = "http://localhost:5000";    // 注: 通过localhost获取的token, 需要对应这里的localhost. 如果是通过127.0.0.1获取的token, 则这里的ip就是127.0.0.1(否则会报错: Bearer was challenged.)
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
