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
        private readonly Action<ILogger, string, int, Exception?> _generationComplete;

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

            _failedToFindAnalyzersInAssembly = LoggerMessage.Define<string>(
                LogLevel.Information,
                JobEventIdFactory.FailedToFindAnalyzersInAssembly(),
                "Failed to find analyzers in assembly: {AssemblyName}");

            _generationComplete = LoggerMessage.Define<string, int>(
                LogLevel.Information,
                JobEventIdFactory.GenerationComplete(),
                "Generation complete for assembly: {AssemblyName}, Analyzers process: {NumberOfAnalyzersProcessed}");
        }

        internal void StartingHandleCommand(ILogger<CommandLineJob> logger)
        {
            _startingHandleCommand(logger, null);
        }

        internal void FailedToLoadAssembly(ILogger<CommandLineJob> logger, string assemblyName)
        {
            _failedToLoadAssembly(logger, assemblyName, null);
        }

        internal void FailedToFindAnalyzersInAssembly(ILogger<CommandLineJob> logger, string assemblyName)
        {
            _failedToFindAnalyzersInAssembly(logger, assemblyName, null);
        }

        internal void GenerationComplete(ILogger<CommandLineJob> logger, string assemblyName, int numberOfAnalyzersProcessed)
        {
            _generationComplete(logger, assemblyName, numberOfAnalyzersProcessed, null);
        }
        
    }
}
