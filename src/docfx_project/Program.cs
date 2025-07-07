using System;
using System.Threading.Tasks;
using Dhgms.DocFx.MermaidJs.Plugin.Markdig;
using Microsoft.DocAsCode;
using Microsoft.DocAsCode.Dotnet;
using Microsoft.DocAsCode.MarkdigEngine.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Whipstaff.Markdig.Settings;
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

                var serviceCollection = new ServiceCollection();
                serviceCollection.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
                var serviceProvider = serviceCollection.BuildServiceProvider();
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                var settings =
                    new MarkdownJsExtensionSettings(PlaywrightBrowserTypeAndChannel.Chrome(), OutputMode.Png);

                var options = new BuildOptions
                {
                    // Enable MermaidJS markdown extension
                    ConfigureMarkdig = pipeline => pipeline.UseMermaidJsExtension(settings, loggerFactory)
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
