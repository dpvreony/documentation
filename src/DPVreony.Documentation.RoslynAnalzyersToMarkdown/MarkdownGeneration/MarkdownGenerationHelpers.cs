using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DPVreony.Documentation.RoslynAnalzyersToMarkdown.MarkdownGeneration
{
    internal class MarkdownGenerationHelpers
    {
        internal void GenerateTableOfContentTable(DiagnosticAnalyzer[] diagnosticAnalyzers, StringBuilder stringBuilder)
        {
            ArgumentNullException.ThrowIfNull(diagnosticAnalyzers);
            ArgumentNullException.ThrowIfNull(stringBuilder);

            foreach (var diagnosticAnalyzer in diagnosticAnalyzers)
            {
                GenerateTableOfContentRowsForDiagnosticAnalyzer(
                    diagnosticAnalyzer,
                    stringBuilder);
            }

            stringBuilder.Insert(0, "| Id | Title | Category | Severity |");
        }

        private void GenerateTableOfContentRowsForDiagnosticAnalyzer(DiagnosticAnalyzer diagnosticAnalyzer, StringBuilder stringBuilder)
        {
            var supportedDiagnostics = diagnosticAnalyzer.SupportedDiagnostics;

            foreach (var diagnosticDescriptor in supportedDiagnostics)
            {
                GenerateTableOfContentRow(diagnosticDescriptor, stringBuilder);
            }
        }

        private void GenerateTableOfContentRow(DiagnosticDescriptor diagnosticDescriptor, StringBuilder stringBuilder)
        {
            var diagnosticId = diagnosticDescriptor.Id;
            var title = diagnosticDescriptor.Title;
            var category = diagnosticDescriptor.Category;
            var severity = diagnosticDescriptor.Severity;

            stringBuilder.Append("| ")
                .Append(diagnosticId)
                .Append(" | ")
                .Append(title)
                .Append(" | ")
                .Append(category)
                .Append(" | ")
                .Append(severity)
                .AppendLine(" | ");
        }

        public void GenerateContentForDiagnosticDescriptor(DiagnosticDescriptor diagnosticDescriptor, StringBuilder stringBuilder)
        {
            GenerateDescriptorRow("Title", diagnosticDescriptor.Title);
            GenerateDescriptorRow("Id", diagnosticDescriptor.Id);
            GenerateDescriptorRow("Category", diagnosticDescriptor.Category);
            GenerateDescriptorRow("Severity", diagnosticDescriptor.Severity);
            GenerateDescriptorRow("Enabled By Default", diagnosticDescriptor.EnabledByDefault);
        }
    }
}
