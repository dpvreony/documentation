// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Dhgms.DocFx.MermaidJs.Plugin.Markdig;
using docfx_project.CommandLine;
using Microsoft.DocAsCode;
using Microsoft.DocAsCode.Dotnet;
using Microsoft.DocAsCode.MarkdigEngine.Extensions;
using Whipstaff.CommandLine;

namespace docfx_project
{
    /// <summary>
    /// Command line job for handling the creation of the Entity Framework Diagram.
    /// </summary>
    public sealed class CommandLineJob : AbstractCommandLineHandler<CommandLineArgModel, CommandLineJobLogMessageActionsWrapper>
    {
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineJob"/> class.
        /// </summary>
        /// <param name="commandLineJobLogMessageActionsWrapper">Wrapper for logging framework messages.</param>
        /// <param name="fileSystem">File System abstraction.</param>
        public CommandLineJob(
            CommandLineJobLogMessageActionsWrapper commandLineJobLogMessageActionsWrapper,
            IFileSystem fileSystem)
            : base(commandLineJobLogMessageActionsWrapper)
        {
            ArgumentNullException.ThrowIfNull(fileSystem);

            _fileSystem = fileSystem;
        }

        /// <inheritdoc/>
        protected override async Task<int> OnHandleCommand(CommandLineArgModel commandLineArgModel)
        {
            ArgumentNullException.ThrowIfNull(commandLineArgModel);

            // TODO: embed roslyn doc gen

            var configPath = commandLineArgModel.DocFxJsonConfigPath.FullName;
            var workingDirectory = _fileSystem.Path.GetDirectoryName(configPath)!;
            Environment.CurrentDirectory = workingDirectory;
            await DotnetApiCatalog.GenerateManagedReferenceYamlFiles(configPath).ConfigureAwait(false);

            var options = new BuildOptions
            {
                // Enable MermaidJS markdown extension
                ConfigureMarkdig = pipeline => pipeline.UseMermaidJsExtension(new MarkdownContext())
            };

            await Docset.Build(configPath, options).ConfigureAwait(false);

            // TODO: we need to generate the PDF.
            return 0;
        }
    }
}
