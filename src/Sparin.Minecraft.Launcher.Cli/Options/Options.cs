using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sparin.Minecraft.Launcher.Cli.Options
{
    public abstract class Options : IOptions
    {
        [Option("verbose", HelpText = "Show verbose output")]
        public bool Verbose { get; set; }
    }
}