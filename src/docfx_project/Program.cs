using System;
using System.Threading.Tasks;
using Dhgms.DocFx.MermaidJs.Plugin.Markdig;
using Microsoft.DocAsCode;
using Microsoft.DocAsCode.Dotnet;
using Microsoft.DocAsCode.MarkdigEngine.Extensions;

namespace docfx_project
{
    /// <summary>
    /// Holds the program entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <returns>Zero for success, non-zero for failure.</returns>
        public static async Task<int> Main()
        {
            try
            {
                // TODO: embed roslyn doc gen

                const string configPath = "docfx.json";
                await DotnetApiCatalog.GenerateManagedReferenceYamlFiles(configPath).ConfigureAwait(false);

                var options = new BuildOptions
                {
                    // Enable MermaidJS markdown extension
                    ConfigureMarkdig = pipeline => pipeline.UseMermaidJsExtension(new MarkdownContext())
                };

                await Docset.Build(configPath, options).ConfigureAwait(false);

                // TODO: we need to generate the PDF.
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Console.Error.WriteLine(ex);
                return 1;
            }

            return 0;
        }
    }
}
