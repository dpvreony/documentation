// Copyright (c) 2020 DHGMS Solutions and Contributors. All rights reserved.
// DHGMS Solutions and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whipstaff.AspNetCore.Features.ApplicationStartup;

namespace DPVreony.Documentation.WebApp
{
    /// <summary>
    /// Web application startup class for the ITK web app.
    /// </summary>
    public class WebAppStartUp : IWhipstaffWebAppStartup
    {
        /// <inheritdoc/>
        public void ConfigureAspireServiceDefaults(IHostApplicationBuilder builder)
        {
        }

        /// <inheritdoc/>
        public void ConfigureLogging(ILoggingBuilder loggingBuilder, ConfigurationManager configuration, IWebHostEnvironment environment)
        {
        }

        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment environment)
        {
        }

        /// <inheritdoc/>
        public void ConfigureWebApplication(WebApplication applicationBuilder)
        {
            applicationBuilder.UseDefaultFiles();
            applicationBuilder.UseStaticFiles();
        }
    }
}
