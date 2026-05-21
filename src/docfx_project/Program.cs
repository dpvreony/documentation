using System;
using System.Threading.Tasks;
using Dhgms.DocFx.MermaidJs.Plugin.Markdig;
using Docfx;
using Docfx.Dotnet;
using Whipstaff.Markdig.Settings;
using Whipstaff.Mermaid.Playwright;
using Whipstaff.Playwright;

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
                // TODO: embed roslyn doc gen - this is blocked by docfx #10969

                const string configPath = "docfx.json";
                await DotnetApiCatalog.GenerateManagedReferenceYamlFiles(configPath).ConfigureAwait(false);

                using (var loggerFactory = new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory())
                {
                    var playwrightRenderer = PlaywrightRenderer.Default(loggerFactory);
                    var browserSession = await playwrightRenderer.GetBrowserSessionAsync(PlaywrightBrowserTypeAndChannel.Chrome())
                        .ConfigureAwait(false);

                    var markdownJsExtensionSettings = new MarkdownJsExtensionSettings(
                        browserSession,
                        OutputMode.Svg);

                    var options = new BuildOptions
                    {
                        // Enable MermaidJS markdown extension
                        ConfigureMarkdig = pipeline => pipeline.UseMermaidJsExtension(
                            markdownJsExtensionSettings,
                            loggerFactory)
                    };

                    await Docset.Build("docfx.json", options);
                    await Docset.Pdf("docfx.json", options);
                }

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
