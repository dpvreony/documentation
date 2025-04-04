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
using System.Threading.Tasks;
using System.Xml.Linq;
using DPVreony.Documentation.RoslynAnalzyersToMarkdown.CommandLine;
using DPVreony.Documentation.RoslynAnalzyersToMarkdown.MarkdownGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;
using Whipstaff.CommandLine;

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

            _commandLineJobLogMessageActionsWrapper.GenerationComplete(
                commandLineArgModel.AssemblyPath.FullName,
                analyzers.Value.Length);
            return 0;
        }

        private async Task GenerateMarkdownFromAnalyzers(
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            DirectoryInfo outputFilePath)
        {
            var workspace = new AdhocWorkspace();
            var solution = workspace.CurrentSolution;
            var project = solution.AddProject("Project", "Project", LanguageNames.CSharp);

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

            await CreateTocFile(
                    analyzers,
                    pathWrapper,
                    fileWrapper,
                    outputFilePathFullName)
                .ConfigureAwait(false);

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
            // TODO: add ability to attach additional documentation via injectable resolver.
            //       this should come from a documentation resource as it should be maintained with the documentation rather than the code?
        }

        private async Task CreateTocFile(ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers, IPath pathWrapper, IFile fileWrapper, string outputFilePathFullName)
        {
            var indexFilePath = pathWrapper.Combine(
                outputFilePathFullName,
                "toc.yml");

            var stringBuilder = new StringBuilder();

            var orderedDiagnostics = diagnosticAnalyzers.Select(analyzer => analyzer.SupportedDiagnostics)
                .SelectMany(supportedDiagnostics => supportedDiagnostics)
                .OrderBy(diagnosticDescriptor => diagnosticDescriptor.Id)
                .ToArray();

            stringBuilder.Append("- name: ").AppendLine("Table of Analyzers");
            stringBuilder.Append("  href: ").AppendLine("index.md");

            foreach (var diagnostic in orderedDiagnostics)
            {
                stringBuilder.Append("- name: \"").Append(diagnostic.Id).Append(": ").Append(diagnostic.Title.ToString().Replace("\"", "\\\"")).AppendLine("\"");
                stringBuilder.Append("  href: ").Append(diagnostic.Id).AppendLine(".md");
            }

            await fileWrapper.WriteAllTextAsync(
                    indexFilePath,
                    stringBuilder.ToString())
                .ConfigureAwait(false);
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
                MarkdownGenerationHelpers.AddMetadataHeader(stringBuilder);
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

            MarkdownGenerationHelpers.AddMetadataHeader(stringBuilder);
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
