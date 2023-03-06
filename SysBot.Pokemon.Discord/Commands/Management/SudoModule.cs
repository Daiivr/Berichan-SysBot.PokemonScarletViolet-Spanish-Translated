using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace SysBot.Pokemon.Discord
{
    public class SudoModule : ModuleBase<SocketCommandContext>
    {
        [Command("blacklist")]
        [Summary("Agrega al usuario mencionado a la lista negra")]
        [RequireSudo]
        // ReSharper disable once UnusedParameter.Global
        public async Task BlackListUsers([Remainder] string _)
        {
            var users = Context.Message.MentionedUsers;
            var objects = users.Select(GetReference);
            SysCordSettings.Settings.UserBlacklist.AddIfNew(objects);
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("blacklistComment")]
        [Summary("Agrega un comentario al ID de un usario en la lista negra.")]
        [RequireSudo]
        // ReSharper disable once UnusedParameter.Global
        public async Task BlackListUsers(ulong id, [Remainder] string comment)
        {
            var obj = SysCordSettings.Settings.UserBlacklist.List.Find(z => z.ID == id);
            if (obj is null)
            {
                await ReplyAsync($"Incapaz de encontrar un usuario con ese ID: ({id}).").ConfigureAwait(false);
                return;
            }

            var oldComment = obj.Comment;
            obj.Comment = comment;
            await ReplyAsync($"Hecho. Se modificó el comentario existente ({oldComment}) por ({comment}).").ConfigureAwait(false);
        }

        [Command("unblacklist")]
        [Summary("Sacar a un usuario de la lista negra.")]
        [RequireSudo]
        // ReSharper disable once UnusedParameter.Global
        public async Task UnBlackListUsers([Remainder] string _)
        {
            var users = Context.Message.MentionedUsers;
            var objects = users.Select(GetReference);
            SysCordSettings.Settings.UserBlacklist.RemoveAll(z => objects.Any(o => o.ID == z.ID));
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("blacklistId")]
        [Summary("Identificaciones de listas negras. (Útil si el usuario no está en el servidor).")]
        [RequireSudo]
        public async Task BlackListIDs([Summary("Separa los ID de Discord por comas")][Remainder] string content)
        {
            var IDs = GetIDs(content);
            var objects = IDs.Select(GetReference);
            SysCordSettings.Settings.UserBlacklist.AddIfNew(objects);
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("unBlacklistId")]
        [Summary("Un-Blacklists IDsIdentificaciones no incluidas en listas negras. (Útil si el usuario no está en el servidor).")]
        [RequireSudo]
        public async Task UnBlackListIDs([Summary("Separa los ID de Discord por comas")][Remainder] string content)
        {
            var IDs = GetIDs(content);
            SysCordSettings.Settings.UserBlacklist.RemoveAll(z => IDs.Any(o => o == z.ID));
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("blacklistSummary")]
        [Alias("printBlacklist", "blacklistPrint")]
        [Summary("Imprime la lista de usuarios en la lista negra.")]
        [RequireSudo]
        public async Task PrintBlacklist()
        {
            var lines = SysCordSettings.Settings.UserBlacklist.Summarize();
            var msg = string.Join("\n", lines);
            await ReplyAsync(Format.Code(msg)).ConfigureAwait(false);
        }

        [Command("removeAlt")]
        [Alias("removeLog", "rmAlt")]
        [Summary("Relimina una identidad (nombre-id) de la base de datos AntiAbuse local de usuario a comerciante")]
        [RequireSudo]
        public async Task RemoveAltAsync([Remainder] string identity)
        {
            if (NewAntiAbuse.Instance.Remove(identity))
                await ReplyAsync($"{identity} ha sido eliminado de la base de datos.").ConfigureAwait(false);
            else
                await ReplyAsync($"{identity} no es una identidad valida.").ConfigureAwait(false);
        }

        private RemoteControlAccess GetReference(IUser channel) => new()
        {
            ID = channel.Id,
            Name = channel.Username,
            Comment = $"Agregado por: {Context.User.Username} el {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
        };

        private RemoteControlAccess GetReference(ulong id) => new()
        {
            ID = id,
            Name = "Manual",
            Comment = $"Agregado por: {Context.User.Username} el {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
        };

        protected static IEnumerable<ulong> GetIDs(string content)
        {
            return content.Split(new[] { ",", ", ", " " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(z => ulong.TryParse(z, out var x) ? x : 0).Where(z => z != 0);
        }
    }
}
