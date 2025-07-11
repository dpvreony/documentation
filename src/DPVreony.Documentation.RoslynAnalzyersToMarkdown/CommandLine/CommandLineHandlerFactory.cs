﻿// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.CommandLine;
using System.IO;
using System.IO.Abstractions;
using Whipstaff.CommandLine;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown.CommandLine
{
    /// <summary>
    /// Factory for creating the root command and binder.
    /// </summary>
    public sealed class CommandLineHandlerFactory : IRootCommandAndBinderFactory<CommandLineArgModelBinder>
    {
        /// <inheritdoc/>
        public RootCommandAndBinderModel<CommandLineArgModelBinder> GetRootCommandAndBinder(IFileSystem fileSystem)
        {
#pragma warning disable CA1861 // Avoid constant arrays as arguments
            var assemblyOption = new Option<FileInfo>(
                    "--assembly-path",
                    "-a")
            {
                Description = "Path to the assembly containing the Roslyn Analyzers",
                Required = true
            }.SpecificFileExtensionOnly(
                fileSystem,
                ".dll")
                .ExistingOnly(fileSystem);

            var outputDirectoryOption = new Option<DirectoryInfo>(
                "--output-directory",
                "-o")
            {
                Description = "Path to the output directory",
                Required = true
            };

            var outputFilePerAnalyzerOption = new Option<bool>(
                "--output-file-per-analyzer",
                "-ofpn")
            {
                Description = "Whether to output each analyzer to a separate markdown file.",
                Required = true
            };
#pragma warning restore CA1861 // Avoid constant arrays as arguments

            var rootCommand = new RootCommand("Generates markdown documentation from a Roslyn Analyzer Assembly.")
                {
                    assemblyOption,
                    outputDirectoryOption,
                    outputFilePerAnalyzerOption
                };

            return new RootCommandAndBinderModel<CommandLineArgModelBinder>(
                rootCommand,
                new CommandLineArgModelBinder(assemblyOption, outputDirectoryOption, outputFilePerAnalyzerOption));
        }
    }
}
