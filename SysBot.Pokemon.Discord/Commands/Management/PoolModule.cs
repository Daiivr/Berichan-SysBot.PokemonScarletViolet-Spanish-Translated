using Discord;
using Discord.Commands;
using PKHeX.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    [Summary("Distribution Pool Module")]
    public class PoolModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        [Command("poolReload")]
        [Summary("Recarga el grupo de bots desde la carpeta de configuración.")]
        [RequireSudo]
        public async Task ReloadPoolAsync()
        {
            var me = SysCord<T>.Runner;
            var hub = me.Hub;

            var pool = hub.Ledy.Pool.Reload(hub.Config.Folder.DistributeFolder);
            if (!pool)
                await ReplyAsync("No se pudo recargar desde la carpeta.").ConfigureAwait(false);
            else
                await ReplyAsync($"Recargado desde la carpeta. Recuento de grupos: {hub.Ledy.Pool.Count}").ConfigureAwait(false);
        }

        [Command("pool")]
        [Summary("Muestra los detalles de los archivos Pokémon en el grupo aleatorio.")]
        public async Task DisplayPoolCountAsync()
        {
            var me = SysCord<T>.Runner;
            var hub = me.Hub;
            var pool = hub.Ledy.Pool;
            var count = pool.Count;
            if (count is > 0 and < 20)
            {
                var lines = pool.Files.Select((z, i) => $"{i + 1:00}: {z.Key} = {(Species)z.Value.RequestInfo.Species}");
                var msg = string.Join("\n", lines);

                var embed = new EmbedBuilder();
                embed.AddField(x =>
                {
                    x.Name = $"Count: {count}";
                    x.Value = msg;
                    x.IsInline = false;
                });
                await ReplyAsync("Detalles del grupo", embed: embed.Build()).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync($"Recuento de grupos: {count}").ConfigureAwait(false);
            }
        }
    }
}
