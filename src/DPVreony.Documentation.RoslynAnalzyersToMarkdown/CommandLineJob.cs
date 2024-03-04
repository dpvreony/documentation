// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DPVreony.Documentation.RoslynAnalzyersToMarkdown.CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Whipstaff.CommandLine;
using Whipstaff.Runtime.Extensions;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown
{
    /// <summary>
    /// Command line job for handling the creation of the Entity Framework Diagram.
    /// </summary>
    public sealed class CommandLineJob : ICommandLineHandler<CommandLineArgModel>
    {
        private readonly CommandLineJobLogMessageActionsWrapper _commandLineJobLogMessageActionsWrapper;
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineJob"/> class.
        /// </summary>
        /// <param name="commandLineJobLogMessageActionsWrapper">Wrapper for logging framework messages.</param>
        /// <param name="fileSystem">File System abstraction.</param>
        public CommandLineJob(
            CommandLineJobLogMessageActionsWrapper commandLineJobLogMessageActionsWrapper,
            IFileSystem fileSystem)
        {
            ArgumentNullException.ThrowIfNull(commandLineJobLogMessageActionsWrapper);
            ArgumentNullException.ThrowIfNull(fileSystem);

            _commandLineJobLogMessageActionsWrapper = commandLineJobLogMessageActionsWrapper;
            _fileSystem = fileSystem;
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
            var analyzers = GetAnalyzersFromAssembly(assembly);

            if (analyzers == null)
            {
                _commandLineJobLogMessageActionsWrapper.FailedToFindAnalyzersInAssembly(commandLineArgModel.AssemblyPath.FullName);
                return 1;
            }

            await GenerateMarkdownFromAnalyzers(
                analyzers.Value,
                _fileSystem,
                outputDirectory).ConfigureAwait(false);

            return 0;
        }

        private async Task GenerateMarkdownFromAnalyzers(ImmutableArray<DiagnosticAnalyzer> analyzers, IFileSystem fileSystem, object outputFilePath)
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

        }

        private ImmutableArray<DiagnosticAnalyzer>? GetAnalyzersFromAssembly(Assembly assembly)
        {
            var allTypes = assembly.GetTypes();

            var matchingTypes = allTypes.Where(type => IsDiagnosticAnalyzer(type));

            var result = new List<DiagnosticAnalyzer>();

            foreach (var matchingType in matchingTypes)
            {
                var ctor = matchingType.GetParameterlessConstructor();
                if (ctor == null)
                {
                    continue;
                }

                var instance = ctor.Invoke(null);

                result.Add((DiagnosticAnalyzer)instance);
            }

            return result.ToImmutableArray();
        }

        private bool IsDiagnosticAnalyzer(Type type)
        {
            if (!type.IsPublic || type.IsAbstract)
            {
                return false;
            }

            if (type.ContainsGenericParameters)
            {
                return false;
            }

            return type.IsAssignableTo(typeof(DiagnosticAnalyzer));
        }
    }
}
