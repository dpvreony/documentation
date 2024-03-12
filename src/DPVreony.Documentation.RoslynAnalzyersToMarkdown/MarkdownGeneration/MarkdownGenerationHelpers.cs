using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown.MarkdownGeneration
{
    public static class MarkdownGenerationHelpers
    {
        public static void GenerateContentForDiagnosticDescriptor(DiagnosticDescriptor diagnosticDescriptor, StringBuilder stringBuilder)
        {
            GenerateDescriptorRow(stringBuilder, "Title", diagnosticDescriptor.Title.ToString());
            GenerateDescriptorRow(stringBuilder, "Id", diagnosticDescriptor.Id);
            GenerateDescriptorRow(stringBuilder, "Category", diagnosticDescriptor.Category);
            GenerateDescriptorRow(stringBuilder, "Default Severity", diagnosticDescriptor.DefaultSeverity.ToString());
            GenerateDescriptorRow(stringBuilder, "Enabled By Default", diagnosticDescriptor.IsEnabledByDefault.ToString());
            GenerateDescriptorRow(stringBuilder, "Description", diagnosticDescriptor.Description.ToString());
            GenerateDescriptorRow(stringBuilder, "Help link", diagnosticDescriptor.HelpLinkUri);
            GenerateDescriptorRow(stringBuilder, "Custom Tags", string.Join(", ", diagnosticDescriptor.CustomTags));
        }

        public static void GenerateTableOfContentTable(ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers, StringBuilder stringBuilder)
        {
            ArgumentNullException.ThrowIfNull(diagnosticAnalyzers);
            ArgumentNullException.ThrowIfNull(stringBuilder);

            stringBuilder.AppendLine("| Id | Title | Category | Default Severity |");
            stringBuilder.AppendLine("| - | - | - | - |");

            var orderedDiagnostics = diagnosticAnalyzers.Select(analyzer => analyzer.SupportedDiagnostics)
                .SelectMany(supportedDiagnostics => supportedDiagnostics)
                .OrderBy(diagnosticDescriptor => diagnosticDescriptor.Id)
                .ToArray();

            foreach (var diagnostic in orderedDiagnostics)
            {
                GenerateTableOfContentRow(diagnostic, stringBuilder);
            }
        }

        private static void GenerateTableOfContentRow(DiagnosticDescriptor diagnosticDescriptor, StringBuilder stringBuilder)
        {
            var diagnosticId = diagnosticDescriptor.Id;
            var title = diagnosticDescriptor.Title;
            var category = diagnosticDescriptor.Category;
            var defaultSeverity = diagnosticDescriptor.DefaultSeverity;

            stringBuilder.Append("| [")
                .Append(diagnosticId)
                .Append("](")
                .Append(diagnosticId)
                .Append(".md) |")
                .Append(title)
                .Append('|')
                .Append(category)
                .Append('|')
                .Append(defaultSeverity)
                .AppendLine("|");
        }

        private static void GenerateDescriptorRow(StringBuilder stringBuilder, string title, string value)
        {
            stringBuilder.Append('|')
                .Append(title)
                .Append('|')
                .Append(value)
                .AppendLine("|");
        }
    }
}
