using System;
using System.Threading.Tasks;
using Docfx;
using Docfx.Dotnet;
using docfx_project.Mermaid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Whipstaff.Mermaid.HttpServer;
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
                // TODO: embed roslyn doc gen

                const string configPath = "docfx.json";
                await DotnetApiCatalog.GenerateManagedReferenceYamlFiles(configPath).ConfigureAwait(false);

                var loggerFactory = new NullLoggerFactory();
                var mermaidHttpServer = MermaidHttpServerFactory.GetTestServer(loggerFactory);
                var logMessageActions = new PlaywrightRendererLogMessageActions();
                var logMessageActionsWrapper = new PlaywrightRendererLogMessageActionsWrapper(
                    logMessageActions,
                    loggerFactory.CreateLogger<PlaywrightRenderer>());
                var playwrightRenderer = new PlaywrightRenderer(
                    mermaidHttpServer,
                    logMessageActionsWrapper);

                var browserSession = await playwrightRenderer.GetBrowserSessionAsync(PlaywrightBrowserTypeAndChannel.Chrome());

                var options = new BuildOptions
                {
                    // Enable MermaidJS markdown extension
                    ConfigureMarkdig = pipeline => pipeline.UseMermaidJsExtension(browserSession)
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
