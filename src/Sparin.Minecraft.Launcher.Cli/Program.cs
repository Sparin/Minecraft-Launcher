using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Sparin.Minecraft.Launcher.Cli.Commands;
using Sparin.Minecraft.Launcher.Cli.Commands.List;
using Sparin.Minecraft.Launcher.Cli.Options;
using Sparin.Minecraft.Launcher.Cli.Options.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sparin.Minecraft.Launcher.Cli
{
    public class Program
    {
        private static bool Verbose { get; set; } = false;

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            // Get all options and commands excepting multi verb commands with their options
            var options = GetOptions()
                .Except(GetOptions(typeof(IListOptions)))
                .ToArray();
            var commands = GetCommands()
                .Except(GetCommands(typeof(IListCommand)))
                .ToArray();

            try
            {
                Parser.Default.ParseArguments(args, options)
                .WithParsed<ListOptions>(options =>
                {
                    // Pass arguments (n - 1) when is multi verb commands are met
                    var subOptions = GetOptions(typeof(IListOptions)).ToArray();
                    var subCommands = GetCommands(typeof(IListCommand)).ToArray();
                    args = args.Skip(1).ToArray();

                    // Parse arguments with new set of subcommands and suboptions
                    Parser.Default.ParseArguments(args, subOptions)
                        .WithParsed<IOptions>(subOptions => Execute(subOptions, subCommands));
                })
                .WithParsed<IOptions>(options =>
                {
                    if (options is ISingleVerbOptions)
                        Execute(options, commands);
                });
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unexpected error occured");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void Execute<TOptions>(TOptions options, Type[] commands = null)
            where TOptions : IOptions
        {
            //commands = commands ?? GetCommands().ToArray();

            var command = commands.FirstOrDefault(type =>
            {
                var commandInterface = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
                var optionsType = commandInterface.GetGenericArguments()
                    .FirstOrDefault(opt => opt.Equals(options.GetType()));

                return optionsType != null;
            });

            if (options is Options.Options sharedOptions)
            {
                Verbose = sharedOptions.Verbose;
            }

            var instance = ActivatorUtilities.CreateInstance(BuildServiceProvider(), command);
            var execute = command.GetMethod(nameof(ICommand<TOptions>.Execute), new Type[] { options.GetType() });
            execute?.Invoke(instance, new object[] { options });
        }

        private static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true);
                builder.SetMinimumLevel(Verbose ? LogLevel.Trace : LogLevel.Information);
            });

            services.AddHttpClient();

            return services.BuildServiceProvider();
        }

        private static IEnumerable<Type> GetCommands(Type commandType = null)
        {
            var commands = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract)
                .Where(type =>
                {
                    var commandInterface = type.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
                    return commandInterface != null;
                });

            if (commandType != null)
                commands = commands.Where(type => commandType.IsAssignableFrom(commandType));

            return commands;
        }

        /////// <summary>
        /////// Returns options type of the command.
        /////// </summary>
        /////// <typeparam name="TCommand">Command type with unknown options type</typeparam>
        /////// <typeparam name="TOptions">Options type which are we searching for</typeparam>
        /////// <param name="command">Instance of the command for determination on compile</param>
        /////// <returns>Options type of the command</returns>
        ////private static Type GetOption<TCommand, TOptions>(TCommand command = null)
        ////    where TCommand : class, ICommand<TOptions>
        ////    where TOptions : IOptions
        ////{
        ////    return typeof(TOptions);
        ////}

        private static IEnumerable<Type> GetOptions(Type optionsType = null)
        {
            optionsType = optionsType ?? typeof(IOptions);

            var options = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(assembly => assembly.GetTypes())
               .Where(type => optionsType.IsAssignableFrom(type))
               .Where(x => !x.IsAbstract);

            return options;
        }
    }
}