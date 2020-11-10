// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;

namespace AuthCenter
{
    public class Program
    {
        public static int Main( string[] args )
        {
            Log.Logger = new LoggerConfiguration( )
                .MinimumLevel.Debug( )
                .MinimumLevel.Override( "Microsoft", LogEventLevel.Warning )
                .MinimumLevel.Override( "Microsoft.Hosting.Lifetime", LogEventLevel.Information )
                .MinimumLevel.Override( "System", LogEventLevel.Warning )
                .MinimumLevel.Override( "Microsoft.AspNetCore.Authentication", LogEventLevel.Information )
                .Enrich.FromLogContext( )
                // uncomment to write to Azure diagnostics stream
                //.WriteTo.File(
                //    @"D:\home\LogFiles\Application\identityserver.txt",
                //    fileSizeLimitBytes: 1_000_000,
                //    rollOnFileSizeLimit: true,
                //    shared: true,
                //    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.Console( outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code )
                .CreateLogger( );

            try
            {
                var host = CreateHostBuilder( args ).Build( );

                var logger = host.Services.GetRequiredService<ILogger<Program>>( );
                var configuration = host.Services.GetRequiredService<IConfiguration>( );

                using ( var scope = host.Services.CreateScope( ) )
                {
                    var cmd = configuration["db"];
                    if ( cmd != null )
                    {
                        var db = scope.ServiceProvider.GetRequiredService<DAL.AuthContext>( );

                        switch ( cmd.ToLower( ) )
                        {
                            case "compare":
                                {
                                    DAL.DbHelper.Compare( db );
                                    return 0;
                                }
                            case "update":
                                {
                                    DAL.DbHelper.Update( db );
                                    return 0;
                                }
                            default: throw new ArgumentException( $"无效的指令: {cmd}" );
                        }
                    }

                    cmd = configuration["seed"];
                    if ( cmd != null )
                    {
                        var db = scope.ServiceProvider.GetRequiredService<DAL.AuthContext>( );

                        switch ( cmd.ToLower( ) )
                        {
                            case "add":
                                {
                                    DAL.SeedData.AddSeedDataAsync( db ).Wait( );
                                    return 0;
                                }
                            case "clear":
                                {
                                    DAL.SeedData.ClearDataAsync( db ).Wait( );
                                    return 0;
                                }
                            default: throw new ArgumentException( $"无效的指令: {cmd}" );
                        }
                    }
                }

                Log.Information( "Starting host..." );
                host.Run( );
                return 0;
            }
            catch ( Exception ex )
            {
                Log.Fatal( ex, "Host terminated unexpectedly." );
                return 1;
            }
            finally
            {
                Log.CloseAndFlush( );
            }
        }

        public static IHostBuilder CreateHostBuilder( string[] args ) =>
            Host.CreateDefaultBuilder( args )
                .UseSerilog( )
                .ConfigureWebHostDefaults( webBuilder =>
                 {
                     webBuilder.ConfigureAppConfiguration( ( context, builder ) =>
                     {
                         builder.AddCommandLine( args );
                     } );

                     webBuilder.UseStartup<Startup>( );
                 } );
    }
}