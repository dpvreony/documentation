// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting;
using Dhgms.NetContrib.Playground.Features.Nuget.DependencyGraph;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

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
            var app = await GetApplication(args).ConfigureAwait(false);
            await app.RunAsync();
        }

        /// <summary>
        /// Gets the distributed application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Instance of the application.</returns>
        public static async Task<DistributedApplication> GetApplication(string[] args)
        {
            var builder = await GetBuilder(args).ConfigureAwait(false);
            var app = builder.Build();
            return app;
        }

        /// <summary>
        /// Gets the builder for the distributed application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Instance of application builder.</returns>
        public static async Task<IDistributedApplicationBuilder> GetBuilder(string[] args)
        {
            var builder = DistributedApplication.CreateBuilder(args);

            var docFxProject = new Projects.docfx_project();

            var docFxProjectFolder = Path.GetDirectoryName(docFxProject.ProjectPath)!;
            var docfxConfigPath = Path.Combine(
                docFxProjectFolder,
                "docfx.json");

            var gripeMarkdownOutputPath = Path.Combine(
                docFxProjectFolder,
                "obj\\gripe");

            var wrapper = MsBuildLocatorWrapper.RegisterLatestVersionOfVisualStudio();
            var gripeAnalyzerPath = await GetGripeAssemblyPath().ConfigureAwait(false);

            var roslynMarkDownGeneration = builder.AddProject<Projects.DPVreony_Documentation_RoslynAnalzyersToMarkdown>("gripe-markdown-builder")
                .WithArgs(
                [
                    "-a",
                    gripeAnalyzerPath,
                    "-o",
                    gripeMarkdownOutputPath,
                    "--output-file-per-analyzer"
                ]);

            var docFxBuilder = builder.AddProject<Projects.docfx_project>("docfx-builder")
                .WithArgs(
                [
                    "--docfx-config-path",
                    docfxConfigPath
                ])
                .WaitForCompletion(roslynMarkDownGeneration);

            builder.AddProject<Projects.DPVreony_Documentation_WebApp>("documentation-website")
                .WaitForCompletion(docFxBuilder);

            return builder;
        }

        private static async Task<string> GetGripeAssemblyPath()
        {
            var upstreamAnalyzers = new Projects.upstream_analyzers();

            const string packageId = "Dhgms.GripeWithRoslyn.Analyzer";
            var packageReference = GetPackageReferenceFromProject(upstreamAnalyzers.ProjectPath, packageId);
            if (packageReference == null)
            {
                throw new InvalidOperationException("Package reference not found.");
            }

            var version = packageReference.GetMetadataValue("Version");

            var nugetVersion = new NuGetVersion(version);

            var package = await GetNuGetPackageFromCacheOrDownload(packageId, nugetVersion).ConfigureAwait(false);
            if (package == null)
            {
                throw new InvalidOperationException("Package not found.");
            }

            return Path.Combine(package, "analyzers\\dotnet\\cs\\Dhgms.GripeWithRoslyn.Analyzer.dll");
        }

        private static ProjectItem? GetPackageReferenceFromProject(string projectFilePath, string packageId)
        {
            return GetPackageReferencesFromProject(projectFilePath).FirstOrDefault(item => item.EvaluatedInclude.Equals(packageId, StringComparison.OrdinalIgnoreCase));
        }

        private static ICollection<ProjectItem> GetPackageReferencesFromProject(string projectFilePath)
        {
            var projectCollection = new ProjectCollection();
            var project = projectCollection.LoadProject(projectFilePath);

            var items = project.GetItems("PackageReference");
            projectCollection.UnloadAllProjects();
            return items;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task<string?> GetNuGetPackageFromCacheOrDownload(string packageId, NuGetVersion version)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

            // Setup NuGet settings
            var settings = Settings.LoadDefaultSettings(null);
            var packageSourceProvider = new PackageSourceProvider(settings);

            // Try to find the package locally first
            var globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
            var localPackagePath = Path.Combine(globalPackagesFolder, packageId.ToLowerInvariant(), version.ToNormalizedString());

            if (Directory.Exists(localPackagePath))
            {
                return localPackagePath;
            }

            var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, Repository.Provider.GetCoreV3());
            var logger = new Whipstaff.Nuget.NugetForwardingToNetCoreLogger(NullLogger.Instance);
            var cancellationToken = CancellationToken.None;

#if TBC
            // If not found locally, try to download it
            foreach (var repo in sourceRepositoryProvider.GetRepositories())
            {
                var resource = await repo.GetResourceAsync<FindPackageByIdResource>();
                using var cacheContext = new SourceCacheContext();

                string packageInstallPath = Path.Combine(globalPackagesFolder, packageId.ToLowerInvariant(), version.ToNormalizedString());

                bool success = await resource.CopyNupkgToStreamAsync(
                    packageId,
                    version,
                    new FileStream(Path.Combine(packageInstallPath, $"{packageId.ToLowerInvariant()}.{version.ToNormalizedString()}.nupkg"), FileMode.Create),
                    cacheContext,
                    logger,
                    cancellationToken
                );

                if (success)
                {
                    return packageInstallPath;
                }
            }
#endif

            return null;
        }
    }
}
