using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown.MarkdownGeneration
{
    public static class MarkdownGenerationHelpers
    {
        public static void GenerateContentForDiagnosticDescriptor(DiagnosticDescriptor diagnosticDescriptor, StringBuilder stringBuilder)
        {
            var title = diagnosticDescriptor.Title.ToString();
            stringBuilder.Append("# ").Append(diagnosticDescriptor.Id).Append(": ").AppendLine(title);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("<table>");

            GenerateDescriptorRow(stringBuilder, "Title", title);
            GenerateDescriptorRow(stringBuilder, "Id", diagnosticDescriptor.Id);
            GenerateDescriptorRow(stringBuilder, "Category", diagnosticDescriptor.Category);
            GenerateDescriptorRow(stringBuilder, "Default Severity", diagnosticDescriptor.DefaultSeverity.ToString());
            GenerateDescriptorRow(stringBuilder, "Enabled By Default", diagnosticDescriptor.IsEnabledByDefault.ToString());
            GenerateDescriptorRow(stringBuilder, "Description", diagnosticDescriptor.Description.ToString());
            // GenerateDescriptorRow(stringBuilder, "Help link", diagnosticDescriptor.HelpLinkUri);
            GenerateDescriptorRow(stringBuilder, "Custom Tags", string.Join(", ", diagnosticDescriptor.CustomTags));

            stringBuilder.AppendLine("</table>");
        }

        public static void GenerateTableOfContentTable(ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers, StringBuilder stringBuilder)
        {
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
                .Append(HttpUtility.HtmlEncode(diagnosticId))
                .Append("](")
                .Append(HttpUtility.HtmlEncode(diagnosticId))
                .Append(".md) |")
                .Append(HttpUtility.HtmlEncode(title))
                .Append('|')
                .Append(HttpUtility.HtmlEncode(category))
                .Append('|')
                .Append(HttpUtility.HtmlEncode(defaultSeverity))
                .AppendLine("|");
        }

        private static void GenerateDescriptorRow(StringBuilder stringBuilder, string title, string value)
        {
            stringBuilder.AppendLine("<tr>");
            stringBuilder.Append("<th>")
                .Append(HttpUtility.HtmlEncode(title))
                .Append("</th><td>")
                .Append(HttpUtility.HtmlEncode(value))
                .AppendLine("</td>");
            stringBuilder.AppendLine("</tr>");
        }

        public static void AddMetadataHeader(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("---");
            stringBuilder.Append("_disableContribution")
                .Append(" : ")
                .AppendLine("true");
            stringBuilder.AppendLine("---");
        }
    }
}
