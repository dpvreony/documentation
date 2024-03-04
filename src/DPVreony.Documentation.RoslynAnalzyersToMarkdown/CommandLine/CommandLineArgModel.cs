﻿// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown.CommandLine
{
    /// <summary>
    /// Model that represents the command line arguments.
    /// </summary>
    public sealed record CommandLineArgModel(FileInfo AssemblyPath, DirectoryInfo OutputDirectory, bool OutputFilePerAnalyzer);
}
