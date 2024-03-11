// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DPVreony.Documentation.RoslynAnalzyersToMarkdown.CommandLine;
using DPVreony.Documentation.RoslynAnalzyersToMarkdown.MarkdownGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Whipstaff.CommandLine;
using Whipstaff.Nuget;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown
{
    /// <summary>
    /// Command line job for handling the creation of the Entity Framework Diagram.
    /// </summary>
    public sealed class CommandLineJob : ICommandLineHandler<CommandLineArgModel>
    {
        private readonly CommandLineJobLogMessageActionsWrapper _commandLineJobLogMessageActionsWrapper;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<CommandLineJob> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineJob"/> class.
        /// </summary>
        /// <param name="commandLineJobLogMessageActionsWrapper">Wrapper for logging framework messages.</param>
        /// <param name="fileSystem">File System abstraction.</param>
        /// <param name="logger">Logging framework instance.</param>
        public CommandLineJob(
            CommandLineJobLogMessageActionsWrapper commandLineJobLogMessageActionsWrapper,
            IFileSystem fileSystem,
            ILogger<CommandLineJob> logger)
        {
            ArgumentNullException.ThrowIfNull(commandLineJobLogMessageActionsWrapper);
            ArgumentNullException.ThrowIfNull(fileSystem);
            ArgumentNullException.ThrowIfNull(logger);

            _commandLineJobLogMessageActionsWrapper = commandLineJobLogMessageActionsWrapper;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<int> HandleCommand(CommandLineArgModel commandLineArgModel)
        {
            var appDomain = AppDomain.CurrentDomain;
            var loadedAssemblies = new List<string>();
            appDomain.AssemblyResolve += (_, args) =>
            {
                var assemblyName = new AssemblyName(args.Name);

                if (args.RequestingAssembly?.Location.Equals(commandLineArgModel.AssemblyPath.FullName, StringComparison.Ordinal) != true
                    && loadedAssemblies.Exists(la => la.Equals(args.RequestingAssembly?.Location, StringComparison.Ordinal)))
                {
                    return null;
                }

                var assemblyPath = _fileSystem.Path.Combine(commandLineArgModel.AssemblyPath.DirectoryName!, $"{assemblyName.Name}.dll");

                if (_fileSystem.File.Exists(assemblyPath))
                {
                    loadedAssemblies.Add(assemblyPath);
#pragma warning disable S3885
                    return Assembly.LoadFrom(assemblyPath);
#pragma warning restore S3885
                }

                return null;
            };

            _commandLineJobLogMessageActionsWrapper.StartingHandleCommand();

            // TODO: pull the assembly in from nuget helper
            var packageSourceProvider = new PackageSourceProvider(new Settings(Environment.CurrentDirectory));
            var packageSources = packageSourceProvider.LoadPackageSources();


            foreach (var packageSource in packageSources)
            {
                var sourceRepository = packageSource.GetRepository();

                string packageId = commandLineArgModel.AssemblyPath.FullName;
                var packageVersion = new NuGetVersion("12.0.1");
                using MemoryStream packageStream = new MemoryStream();

                var resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>();

                var cache = new SourceCacheContext();
                var logger = new Whipstaff.Nuget.NugetForwardingToNetCoreLogger(_logger);

                var downloader = await resource.GetPackageDownloaderAsync(new PackageIdentity(packageId, packageVersion), cache, logger, CancellationToken.None);
                var files = await downloader.CoreReader.GetPackageFilesAsync(PackageSaveMode.Defaultv3, CancellationToken.None);

                /*
                await resource.CopyNupkgToStreamAsync(
                    packageId,
                    packageVersion,
                    packageStream,
                    cache,
                    logger,
                    CancellationToken.None);
                */
            }

#pragma warning disable S3885
            var assembly = Assembly.LoadFrom(commandLineArgModel.AssemblyPath.FullName);
#pragma warning restore S3885

            var outputDirectory = commandLineArgModel.OutputDirectory;
            var analyzers = Helpers.ReflectionHelpers.GetAnalyzersFromAssembly(assembly);

            if (analyzers == null)
            {
                _commandLineJobLogMessageActionsWrapper.FailedToFindAnalyzersInAssembly(commandLineArgModel.AssemblyPath.FullName);
                return 1;
            }

            await GenerateMarkdownFromAnalyzers(
                analyzers.Value,
                outputDirectory).ConfigureAwait(false);

            return 0;
        }

        private async Task GenerateMarkdownFromAnalyzers(
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            DirectoryInfo outputFilePath)
        {
            var workspace = new AdhocWorkspace();
            var solution = workspace.CurrentSolution;
            var project = solution.Projects.First();
            var compilation = await project.GetCompilationAsync();
            if (compilation == null)
            {
                throw new InvalidOperationException("Failed to get compilation");
            }

            var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
            if (compilationWithAnalyzers == null)
            {
                throw new InvalidOperationException("Failed to set up analyzers into compilation session");
            }

            var outputFilePathFullName= outputFilePath.FullName;

            _fileSystem.Directory.CreateDirectory(outputFilePathFullName);

            var pathWrapper = _fileSystem.Path;
            var fileWrapper = _fileSystem.File;

            await CreateIndexFile(
                analyzers,
                pathWrapper,
                fileWrapper,
                outputFilePathFullName)
                .ConfigureAwait(false);

            await CreateIndividualFiles(
                analyzers,
                pathWrapper,
                fileWrapper,
                outputFilePathFullName)
                .ConfigureAwait(false);


            // TODO: refactor to have a documentation helper dll that can be injected into our pipeline instead of running out of process.
            // TODO: add support for custom formatting
            // TODO: add support for toc.yml?
            // TODO: add ability to attach additional documentation via injectable resolver.
            //       this should come from a documentation resource as it should be maintained with the documentation rather than the code?
        }

        private async Task CreateIndividualFiles(
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            IPath pathWrapper,
            IFile fileWrapper,
            string outputFilePathFullName)
        {
            foreach (var diagnosticAnalyzer in analyzers)
            {
                await CreateIndividualFiles(
                        diagnosticAnalyzer,
                        pathWrapper,
                        fileWrapper,
                        outputFilePathFullName)
                    .ConfigureAwait(false);
            }
        }

        private async Task CreateIndividualFiles(
            DiagnosticAnalyzer diagnosticAnalyzer,
            IPath pathWrapper,
            IFile fileWrapper,
            string outputFilePathFullName)
        {
            var supportedDiagnostics = diagnosticAnalyzer.SupportedDiagnostics;
            foreach (var diagnostic in supportedDiagnostics)
            {
                var stringBuilder = new StringBuilder();
                MarkdownGenerationHelpers.GenerateContentForDiagnosticDescriptor(diagnostic, stringBuilder);

                var filename = $"{diagnostic.Id}.md";

                var outputFilePath = pathWrapper.Combine(
                    outputFilePathFullName,
                    filename
                    );

                await fileWrapper.WriteAllTextAsync(
                        outputFilePath,
                        stringBuilder.ToString())
                    .ConfigureAwait(false);
            }
        }

        private async Task CreateIndexFile(
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            IPath pathWrapper,
            IFile fileWrapper,
            string outputFilePathFullName)
        {
            var indexFilePath = pathWrapper.Combine(
                outputFilePathFullName,
                "index.md");

            var stringBuilder = new StringBuilder();

            MarkdownGenerationHelpers.GenerateTableOfContentTable(
                analyzers,
                stringBuilder);

            await fileWrapper.WriteAllTextAsync(
                    indexFilePath,
                    stringBuilder.ToString())
                .ConfigureAwait(false);
        }
    }
}
