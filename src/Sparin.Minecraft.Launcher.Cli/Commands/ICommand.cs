using Sparin.Minecraft.Launcher.Cli.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sparin.Minecraft.Launcher.Cli.Commands
{
    public interface ICommand<in T> where T : IOptions
    {
        void Execute(T options);
    }
}