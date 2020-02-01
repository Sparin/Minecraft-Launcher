using dotMCLauncher.Versioning;
using Microsoft.Extensions.Logging;
using Sparin.Minecraft.Launcher.Cli.Options;
using Sparin.Minecraft.Launcher.Cli.Options.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Sparin.Minecraft.Launcher.Cli.Commands.List
{
    public class ListVersionsCommand : IListCommand, ICommand<ListVersionsOptions>
    {
        private const string VERSION_MANIFEST_URL = @"https://launchermeta.mojang.com/mc/game/version_manifest.json";

        private readonly ILogger<ListVersionsCommand> _logger;
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _clientFactory;

        public ListVersionsCommand(IHttpClientFactory clientFactory, ILogger<ListVersionsCommand> logger = null)
        {
            _clientFactory = clientFactory;
            _httpClient = clientFactory.CreateClient();
            _logger = logger;
        }

        public void Execute(ListVersionsOptions options)
        {
            var json = _httpClient.GetStringAsync(VERSION_MANIFEST_URL).Result;
            var manifest = RawVersionListManifest.ParseList(json);
            foreach (var version in manifest.Versions)
            {
                var message = string.Format("{0,-50}{1}", version.VersionId, version.ReleaseType);
                _logger.LogInformation(message);
            }
        }
    }
}