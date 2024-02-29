﻿using System.Text;

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
                .Append(" | ");
        }

        public void GenerateContentForAnalyzer(DiagnosticAnalyzer diagnosticAnalyzer, StringBuilder stringBuilder)
        {
            ArgumentNullException.ThrowIfNull(diagnosticAnalyzer);
            ArgumentNullException.ThrowIfNull(stringBuilder);
        }
    }
}
