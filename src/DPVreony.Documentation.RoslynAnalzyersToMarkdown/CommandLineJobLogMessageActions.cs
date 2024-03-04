// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;
using Whipstaff.Core.Logging;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown
{
    /// <summary>
    /// Log message actions for <see cref="CommandLineJob"/>.
    /// </summary>
    public class CommandLineJobLogMessageActions : ILogMessageActions<CommandLineJob>
    {
        private readonly Action<ILogger, Exception?> _startingHandleCommand;
        private readonly Action<ILogger, string, Exception?> _failedToLoadAssembly;
        private readonly Action<ILogger, string, Exception?> _failedToFindAnalyzersInAssembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineJobLogMessageActions"/> class.
        /// </summary>
        public CommandLineJobLogMessageActions()
        {
            _startingHandleCommand = LoggerMessage.Define(
                LogLevel.Information,
                JobEventIdFactory.StartingHandleCommand(),
                "Starting execution of CLI job handler");

            _failedToLoadAssembly = LoggerMessage.Define<string>(
                LogLevel.Information,
                JobEventIdFactory.FailedToLoadAssembly(),
                "Failed to load assembly: {AssemblyName}");

            _failedToFindAnalyzersAssembly = LoggerMessage.Define<string>(
                LogLevel.Information,
                JobEventIdFactory.FailedToFindAnalyzersInAssembly(),
                "Failed to find analyzers in assembly: {AssemblyName}");
        }

        internal void StartingHandleCommand(ILogger<CommandLineJob> logger)
        {
            _startingHandleCommand(logger, null);
        }

        internal void FailedToLoadAssembly(ILogger<CommandLineJob> logger, string assemblyName)
        {
            _failedToLoadAssembly(logger, assemblyName, null);
        }

        internal void FailedToFindAnakyzersInAssembly(ILogger<CommandLineJob> logger, string assemblyName)
        {
            _failedToFindAnalyzersInAssembly(logger, assemblyName, null);
        }
    }
}
