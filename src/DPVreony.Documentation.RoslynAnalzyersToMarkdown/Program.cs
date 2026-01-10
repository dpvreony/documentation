// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using System.Threading.Tasks;
using DPVreony.Documentation.RoslynAnalzyersToMarkdown.CommandLine;
using Whipstaff.CommandLine.Hosting;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown
{
    /// <summary>
    /// Hosts the program entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>0 for success, non 0 for failure.</returns>
        public static Task<int> Main(string[] args)
        {
            return HostRunner.RunSimpleCliJobAsync<
                CommandLineJob,
                CommandLineArgModel,
                CommandLineArgModelBinder,
                CommandLineHandlerFactory>(
                args,
                (fileSystem, logger) => new CommandLineJob(
                    new CommandLineJobLogMessageActionsWrapper(
                        logger,
                        new CommandLineJobLogMessageActions()),
                    fileSystem,
                    logger),
                new FileSystem());
        }
    }
}
