// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;

namespace docfx_project.CommandLine
{
    /// <summary>
    /// Binding logic for the command line arguments.
    /// </summary>
    public sealed class CommandLineArgModelBinder : BinderBase<CommandLineArgModel>
    {
        private readonly Option<FileInfo> _docFxJsonConfigPathOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineArgModelBinder"/> class.
        /// </summary>
        /// <param name="docFxJsonConfigPathOption">DocFx Json Config Path option to parse and bind against.</param>
        public CommandLineArgModelBinder(
            Option<FileInfo> docFxJsonConfigPathOption)
        {
            ArgumentNullException.ThrowIfNull(docFxJsonConfigPathOption);

            _docFxJsonConfigPathOption = docFxJsonConfigPathOption;
        }

        /// <inheritdoc/>
        protected override CommandLineArgModel GetBoundValue(BindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var docFxJsonConfigPath = bindingContext.ParseResult.GetValueForOption(_docFxJsonConfigPathOption);

            return new CommandLineArgModel(docFxJsonConfigPath!);
        }
    }
}
