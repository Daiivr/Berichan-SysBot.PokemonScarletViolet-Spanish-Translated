using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace SysBot.Pokemon.Discord
{
    public class OwnerModule : SudoModule
    {
        [Command("addSudo")]
        [Summary("Agrega el usuario mencionado a sudo global")]
        [RequireOwner]
        // ReSharper disable once UnusedParameter.Global
        public async Task SudoUsers([Remainder] string _)
        {
            var users = Context.Message.MentionedUsers;
            var objects = users.Select(GetReference);
            SysCordSettings.Settings.GlobalSudoList.AddIfNew(objects);
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("removeSudo")]
        [Summary("Elimina al usuario mencionado de sudo global")]
        [RequireOwner]
        // ReSharper disable once UnusedParameter.Global
        public async Task RemoveSudoUsers([Remainder] string _)
        {
            var users = Context.Message.MentionedUsers;
            var objects = users.Select(GetReference);
            SysCordSettings.Settings.GlobalSudoList.RemoveAll(z => objects.Any(o => o.ID == z.ID));
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("addChannel")]
        [Summary("Agrega un canal a la lista de canales que aceptan comandos.")]
        [RequireOwner]
        // ReSharper disable once UnusedParameter.Global
        public async Task AddChannel()
        {
            var obj = GetReference(Context.Message.Channel);
            SysCordSettings.Settings.ChannelWhitelist.AddIfNew(new[] { obj });
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("removeChannel")]
        [Summary("Elimina un canal de la lista de canales que aceptan comandos.")]
        [RequireOwner]
        // ReSharper disable once UnusedParameter.Global
        public async Task RemoveChannel()
        {
            var obj = GetReference(Context.Message.Channel);
            SysCordSettings.Settings.ChannelWhitelist.RemoveAll(z => z.ID == obj.ID);
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("leave")]
        [Alias("bye")]
        [Summary("Abandona el servidor actual.")]
        [RequireOwner]
        // ReSharper disable once UnusedParameter.Global
        public async Task Leave()
        {
            await ReplyAsync("Goodbye.").ConfigureAwait(false);
            await Context.Guild.LeaveAsync().ConfigureAwait(false);
        }

        [Command("leaveguild")]
        [Alias("lg")]
        [Summary("Abandona el servidor según el ID suministrado.")]
        [RequireOwner]
        // ReSharper disable once UnusedParameter.Global
        public async Task LeaveGuild(string userInput)
        {
            if (!ulong.TryParse(userInput, out ulong id))
            {
                await ReplyAsync("Proporcione una ID de servidor válida.").ConfigureAwait(false);
                return;
            }

            var guild = Context.Client.Guilds.FirstOrDefault(x => x.Id == id);
            if (guild is null)
            {
                await ReplyAsync($"La informacion proporcionada ({userInput}) no es una ID de servidor válida o el bot no está en el servidor especificado.").ConfigureAwait(false);
                return;
            }

            await ReplyAsync($"Saliendo del server: {guild}.").ConfigureAwait(false);
            await guild.LeaveAsync().ConfigureAwait(false);
        }

        [Command("leaveall")]
        [Summary("Deja todos los servidores en los que se encuentra actualmente el bot.")]
        [RequireOwner]
        // ReSharper disable once UnusedParameter.Global
        public async Task LeaveAll()
        {
            await ReplyAsync("Dejando todos los servidores.").ConfigureAwait(false);
            foreach (var guild in Context.Client.Guilds)
            {
                await guild.LeaveAsync().ConfigureAwait(false);
            }
        }

        [Command("sudoku")]
        [Alias("kill", "shutdown")]
        [Summary("Hace que todos los procesos terminen y se apague el bot !")]
        [RequireOwner]
        // ReSharper disable once UnusedParameter.Global
        public async Task ExitProgram()
        {
            await Context.Channel.EchoAndReply("Apagando... adiós! **Los servicios de bot se desconectan.**").ConfigureAwait(false);
            Environment.Exit(0);
        }

        private RemoteControlAccess GetReference(IUser channel) => new()
        {
            ID = channel.Id,
            Name = channel.Username,
            Comment = $"Agregado por: {Context.User.Username} el {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
        };

        private RemoteControlAccess GetReference(IChannel channel) => new()
        {
            ID = channel.Id,
            Name = channel.Name,
            Comment = $"Agregado por: {Context.User.Username} el {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
        };
    }
}
