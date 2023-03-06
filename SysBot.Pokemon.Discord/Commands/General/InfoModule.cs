using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    // src: https://github.com/foxbot/patek/blob/master/src/Patek/Modules/InfoModule.cs
    // ISC License (ISC)
    // Copyright 2017, Christopher F. <foxbot@protonmail.com>
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private const string detail = "Soy un bot de Discord personalizado de código abierto impulsado por PKHeX.Core y otros softwares de código abierto.";
        private const string repo = "https://github.com/berichan/SysBot.PokemonScarletViolet"; // refer to https://github.com/kwsch/SysBot.NET for official SysBot.NET
        private const string ESP = "https://github.com/Daiivr/Berichan-SysBot.PokemonScarletViolet-Spanish-Translated";

        [Command("info")]
        [Alias("about", "whoami", "owner")]
        public async Task InfoAsync()
        {
            var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = detail,
            };

            builder.AddField("Informacion",
                $"- [Código fuente]({repo})\n" +
                $"- [Bot en español]({ESP})\n" + 
                $"- {Format.Bold("Dueño")}: {app.Owner} ({app.Owner.Id})\n" +
                $"- {Format.Bold("Biblioteca")}: Discord.Net ({DiscordConfig.Version})\n" +
                $"- {Format.Bold("Tiempo de actividad")}: {GetUptime()}\n" +
                $"- {Format.Bold("Tiempo de ejecución")}: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} " +
                $"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n" +
                $"- {Format.Bold("Buildtime")}: {GetBuildTime()}\n" +
                $"- {Format.Bold("Core")}: {GetCoreDate()}\n" +
                $"- {Format.Bold("AutoLegality")}: {GetALMDate()}\n"
                );

            builder.AddField("Estadísticas",
                $"- {Format.Bold("Tamaño")}: {GetHeapSize()}MiB\n" +
                $"- {Format.Bold("Servers")}: {Context.Client.Guilds.Count}\n" +
                $"- {Format.Bold("Canales")}: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- {Format.Bold("Usuarios")}: {Context.Client.Guilds.Sum(g => g.MemberCount)}\n"
                );

            await ReplyAsync("Aquí hay un poco sobre mí!", embed: builder.Build()).ConfigureAwait(false);
        }

        private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.CurrentCulture);

        private static string GetBuildTime()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
                return "Unknown";
            return File.GetLastWriteTime(assembly.Location).ToString(@"yy-MM-dd\.hh\:mm");
        }

        public static string GetCoreDate() => GetDateOfDll("PKHeX.Core.dll");
        public static string GetALMDate() => GetDateOfDll("PKHeX.Core.AutoMod.dll");

        private static string GetDateOfDll(string dll)
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
                return "Unknown";
            var folder = Path.GetDirectoryName(assembly.Location);
            var path = Path.Combine(folder ?? "", dll);
            var date = File.GetLastWriteTime(path);
            return date.ToString(@"yy-MM-dd\.hh\:mm");
        }
    }
}
