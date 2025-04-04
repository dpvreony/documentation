// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using System.Threading.Tasks;
using Aspire.Hosting;

namespace DPVreony.Documentation.AspireAppHost
{
    /// <summary>
    /// Program entry point for the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var app = GetApplication(args);
            await app.RunAsync();
        }

        /// <summary>
        /// Gets the distributed application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Instance of the application.</returns>
        public static DistributedApplication GetApplication(string[] args)
        {
            var builder = GetBuilder(args);
            var app = builder.Build();
            return app;
        }

        /// <summary>
        /// Gets the builder for the distributed application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Instance of application builder.</returns>
        public static IDistributedApplicationBuilder GetBuilder(string[] args)
        {
            var builder = DistributedApplication.CreateBuilder(args);

            var docFxProject = new Projects.docfx_project();

            var docfxConfigPath = Path.Combine(
                Path.GetDirectoryName(docFxProject.ProjectPath)!,
                "docfx.json");

            builder.AddProject<Projects.docfx_project>("docfx-builder")
                .WithArgs(
                [
                    "--docfx-config-path",
                    docfxConfigPath
                ]);

            builder.AddProject<Projects.DPVreony_Documentation_WebApp>("documentation-website");

            return builder;
        }
    }
}
