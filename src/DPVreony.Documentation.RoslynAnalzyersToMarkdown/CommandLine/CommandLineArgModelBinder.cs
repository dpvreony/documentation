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
        /// <param name="outputDirectoryOption">Option to parse and bind against for the output directory.</param>
        /// <param name="outputFilePerAnalyzerOption">Option to bind against for parsing whether to output each analyzer to a separate file.</param>
        public CommandLineArgModelBinder(
            Option<FileInfo> assemblyOption,
            Option<DirectoryInfo> outputDirectoryOption,
            Option<bool> outputFilePerAnalyzerOption)
        {
            ArgumentNullException.ThrowIfNull(assemblyOption);
            ArgumentNullException.ThrowIfNull(outputDirectoryOption);
            ArgumentNullException.ThrowIfNull(outputFilePerAnalyzerOption);

            _assemblyOption = assemblyOption;
            _outputDirectoryOption = outputDirectoryOption;
            _outputFilePerAnalyzerOption = outputFilePerAnalyzerOption;
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
