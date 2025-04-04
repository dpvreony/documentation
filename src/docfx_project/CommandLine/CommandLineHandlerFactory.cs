// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.CommandLine;
using System.IO;
using System.IO.Abstractions;
using Whipstaff.CommandLine;

namespace docfx_project.CommandLine
{
    /// <summary>
    /// Factory for creating the root command and binder.
    /// </summary>
    public sealed class CommandLineHandlerFactory : IRootCommandAndBinderFactory<CommandLineArgModelBinder>
    {
        /// <inheritdoc/>
        public RootCommandAndBinderModel<CommandLineArgModelBinder> GetRootCommandAndBinder(IFileSystem fileSystem)
        {
            ArgumentNullException.ThrowIfNull(fileSystem);

#pragma warning disable CA1861 // Avoid constant arrays as arguments
            var docfxJsonConfigPathOption = new Option<FileInfo>(
                [
                    "--docfx-config-path",
                    "-dc"
                ],
                "Path to the file containing the docfx config")
            {
                IsRequired = true
            }.SpecificFileExtensionsOnly(
                fileSystem,
                [
                    ".json"
                ])
                .ExistingOnly(fileSystem);

#pragma warning restore CA1861 // Avoid constant arrays as arguments

            var rootCommand = new RootCommand("Generates a docfx site based on config.")
                {
                    docfxJsonConfigPathOption
                };

            return new RootCommandAndBinderModel<CommandLineArgModelBinder>(
                rootCommand,
                new CommandLineArgModelBinder(docfxJsonConfigPathOption));
        }
    }
}
