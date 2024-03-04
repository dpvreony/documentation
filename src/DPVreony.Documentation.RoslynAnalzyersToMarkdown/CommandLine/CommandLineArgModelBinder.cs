// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown.CommandLine
{
    /// <summary>
    /// Binding logic for the command line arguments.
    /// </summary>
    public sealed class CommandLineArgModelBinder : BinderBase<CommandLineArgModel>
    {
        private readonly Option<FileInfo> _assemblyOption;
        private readonly Option<DirectoryInfo> _outputDirectoryOption;
        private readonly Option<bool> _outputFilePerAnalyzerOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineArgModelBinder"/> class.
        /// </summary>
        /// <param name="assemblyOption">Assembly to parse and bind against.</param>
        /// <param name="dbContextNameOption">Name of the db context to parse and bind against.</param>
        /// <param name="outputFilePathOption">Output file path to parse and bind against.</param>
        /// <param name="diagramTypeOption">Diagram type to parse and bind against.</param>
        public CommandLineArgModelBinder(
            Option<FileInfo> assemblyOption,
            Option<FileInfo> outputDirectoryOption,
            Option<bool> outputFilePerAnalyzerOption)
        {
            ArgumentNullException.ThrowIfNull(assemblyOption);
            ArgumentNullException.ThrowIfNull(outputDirectoryOption);
            ArgumentNullException.ThrowIfNull(outputFilePerAnalyzerOption);

            _assemblyOption = assemblyOption;
            _outputDirectoryOption = outputDirectoryOption;
            _outputDirectoryOption = outputDirectoryOption;
        }

        /// <inheritdoc/>
        protected override CommandLineArgModel GetBoundValue(BindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var assembly = bindingContext.ParseResult.GetValueForOption(_assemblyOption);
            var outputDirectory = bindingContext.ParseResult.GetValueForOption(_outputDirectoryOption);
            var outputFilePerAnalyzer = bindingContext.ParseResult.GetValueForOption(_outputFilePerAnalyzerOption);

            return new CommandLineArgModel(
                assembly!,
                outputDirectory!,
                outputFilePerAnalyzer!);
        }
    }
}
