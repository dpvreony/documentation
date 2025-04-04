// Copyright (c) 2020 DHGMS Solutions and Contributors. All rights reserved.
// DHGMS Solutions and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Whipstaff.AspNetCore.Features.ApplicationStartup;

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

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = "F:\\github\\dpvreony\\documentation\\src\\docfx_project\\_site",
                WebRootPath = "F:\\github\\dpvreony\\documentation\\src\\docfx_project\\_site"
            });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Run();
        }
    }
}
