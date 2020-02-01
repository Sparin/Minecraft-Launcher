using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sparin.Minecraft.Launcher.Cli.Options.List
{
    [Verb("versions", HelpText = "Lists available Minecraft versions for the launcher")]
    public class ListVersionsOptions : Options, IListOptions
    {
    }
}