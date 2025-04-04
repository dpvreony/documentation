// Copyright (c) 2020 DHGMS Solutions and Contributors. All rights reserved.
// DHGMS Solutions and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;

namespace DPVreony.Documentation.WebApp
{
    /// <summary>
    /// Program host.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            // WebApplicationFactory.GetHostApplicationBuilder<WebAppStartUp>(args, null).Run();

            // TODO: check where we are running from, or allow Aspire app host to inject the setting.
            var path = Path.GetFullPath("../docfx_project/_site");

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = path,
                WebRootPath = path
            });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Run();
        }
    }
}
