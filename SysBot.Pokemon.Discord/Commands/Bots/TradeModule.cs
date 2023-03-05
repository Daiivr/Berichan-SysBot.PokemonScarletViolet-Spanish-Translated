using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;
using System.Linq;
using System.Threading.Tasks;
using SysBot.Base;
using PKHeX.Core.AutoMod;
using System.Data.SqlTypes;

namespace SysBot.Pokemon.Discord
{
    [Summary("Queues new Link Code trades")]
    public class TradeModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

        [Command("tradeList")]
        [Alias("tl")]
        [Summary("Prints the users in the trade queues.")]
        [RequireSudo]
        public async Task GetTradeListAsync()
        {
            string msg = Info.GetTradeList(PokeRoutineType.LinkTrade);
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = "Pending Trades";
                x.Value = msg;
                x.IsInline = false;
            });
            await ReplyAsync("Estos son los usuarios que están esperando actualmente:", embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you the provided Pokémon file.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeAsyncAttach([Summary("Trade Code")] int code)
        {
            var sig = Context.User.GetFavor();
            await TradeAsyncAttach(code, sig, Context.User).ConfigureAwait(false);
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you a Pokémon converted from the provided Showdown Set.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeAsync([Summary("Trade Code")] int code, [Summary("Showdown Set")][Remainder] string content)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            var template = AutoLegalityWrapper.GetTemplate(set);
            if (set.InvalidLines.Count != 0 || set.Species <= 0)
            {
                var msg = $"✘ No se puede analizar el conjunto Showdown:\n{string.Join("\n", set.InvalidLines)}";
                await ReplyAsync(msg).ConfigureAwait(false);
                return;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);
                var la = new LegalityAnalysis(pkm);
                var spec = GameInfo.Strings.Species[template.Species];
                pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;
                if (pkm is not T pk || !la.Valid)
                {
                    var reason = result == "Timeout" ? $"Este **{spec}** tomo demaciado tiempo en generarse." : $"No puede crear un **{spec}** con los datos proporcionados.";
                    var imsg = $"⚠️ Oops! {reason}";
                    if (result == "Failed")
                        imsg += $"\n{AutoLegalityWrapper.GetLegalizationHint(template, sav, pkm)}";
                    await ReplyAsync(imsg).ConfigureAwait(false);
                    return;
                }
                pk.ResetPartyStats();

                var sig = Context.User.GetFavor();
                await AddTradeToQueueAsync(code, Context.User.Username, pk, sig, Context.User).ConfigureAwait(false);
            }

            catch (Exception ex)

            {
                LogUtil.LogSafe(ex, nameof(TradeModule<T>));
                var msg = $"⚠️ Oops! Ocurrió un problema inesperado con este Showdown Set:\n```{string.Join("\n", set.GetSetLines())}```";
                await ReplyAsync(msg).ConfigureAwait(false);
            }
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you a Pokémon converted from the provided Showdown Set.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeAsync([Summary("Showdown Set")][Remainder] string content)
        {
            var code = Info.GetRandomTradeCode();
            await TradeAsync(code, content).ConfigureAwait(false);
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you the attached file.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeAsyncAttach()
        {
            var code = Info.GetRandomTradeCode();
            await TradeAsyncAttach(code).ConfigureAwait(false);
        }

        [Command("banTrade")]
        [Alias("bt")]
        [RequireSudo]
        public async Task BanTradeAsync([Summary("Online ID")] ulong nnid, string comment)
        {
            SysCordSettings.HubConfig.TradeAbuse.BannedIDs.AddIfNew(new[] { GetReference(nnid, comment) });
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        private RemoteControlAccess GetReference(ulong id, string comment) => new()
        {
            ID = id,
            Name = id.ToString(),
            Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss} ({comment})",
        };

        [Command("tradeUser")]
        [Alias("tu", "tradeOther")]
        [Summary("Makes the bot trade the mentioned user the attached file.")]
        [RequireSudo]
        public async Task TradeAsyncAttachUser([Summary("Trade Code")] int code, [Remainder]string _)
        {
            if (Context.Message.MentionedUsers.Count > 1)
            {
                await ReplyAsync("⚠️ Demasiadas menciones. Solo puedes agregar a la lista un usario a la vez.").ConfigureAwait(false);
                return;
            }

            if (Context.Message.MentionedUsers.Count == 0)
            {
                await ReplyAsync("⚠️ Un usuario debe ser mencionado para hacer esto.").ConfigureAwait(false);
                return;
            }

            var usr = Context.Message.MentionedUsers.ElementAt(0);
            var sig = usr.GetFavor();
            await TradeAsyncAttach(code, sig, usr).ConfigureAwait(false);
        }

        [Command("tradeUser")]
        [Alias("tu", "tradeOther")]
        [Summary("Makes the bot trade the mentioned user the attached file.")]
        [RequireSudo]
        public async Task TradeAsyncAttachUser([Remainder] string _)
        {
            var code = Info.GetRandomTradeCode();
            await TradeAsyncAttachUser(code, _).ConfigureAwait(false);
        }

        private async Task TradeAsyncAttach(int code, RequestSignificance sig, SocketUser usr)
        {
            var attachment = Context.Message.Attachments.FirstOrDefault();
            if (attachment == default)
            {
                await ReplyAsync("No se proporcionó ningún archivo adjunto!").ConfigureAwait(false);
                return;
            }

            var att = await NetUtil.DownloadPKMAsync(attachment).ConfigureAwait(false);
            var pk = GetRequest(att);
            if (pk == null)
            {
                await ReplyAsync("⚠️ El archivo adjunto proporcionado no es compatible con este módulo!").ConfigureAwait(false);
                return;
            }

            await AddTradeToQueueAsync(code, usr.Username, pk, sig, usr).ConfigureAwait(false);
        }

        [Command("giveaway")]
        [Alias("ga", "preset")]
        [Summary("Makes the bot trade you a Pokémon from the giveaway pool.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeGiveawayAsync([Summary("Trade Code")] int code, [Summary("Showdown Set")][Remainder] string content)
        {
            try
            {
                PKM? pk = PokemonPool<T>.TryFetchFromDistributeDirectory(SysCord<T>.Runner.Hub.Config.Folder.DistributeFolder, content.Trim());
                if (pk == null || pk is not T pkSend)
                {
                    var msg = $"⚠️ Oops! Incapaz de iniciar el sorteo: `{content}`";
                    await ReplyAsync(msg).ConfigureAwait(false);
                    return;
                }

                pk.ResetPartyStats();

                var sig = Context.User.GetFavor();
                await AddTradeToQueueAsync(code, Context.User.Username, pkSend, sig, Context.User).ConfigureAwait(false);
            }

            catch (Exception ex)

            {
                LogUtil.LogSafe(ex, nameof(TradeModule<T>));
                var msg = $"⚠️ Ocurrió un problema inesperado con los ajustes preestablecidos:\n```{string.Join("\n", content)}```";
                await ReplyAsync(msg).ConfigureAwait(false);
            }
        }

        [Command("giveaway")]
        [Alias("ga", "preset")]
        [Summary("Makes the bot trade you a Pokémon from the giveaway pool.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeGiveawayAsync([Summary("Showdown Set")][Remainder] string content)
        {
            var code = Info.GetRandomTradeCode();
            await TradeGiveawayAsync(code, content).ConfigureAwait(false);
        }

        private static T? GetRequest(Download<PKM> dl)
        {
            if (!dl.Success)
                return null;
            return dl.Data switch
            {
                null => null,
                T pk => pk,
                _ => EntityConverter.ConvertToType(dl.Data, typeof(T), out _) as T,
            };
        }

        private async Task AddTradeToQueueAsync(int code, string trainerName, T pk, RequestSignificance sig, SocketUser usr)
        {
            if (!pk.CanBeTraded())
            {
                await ReplyAsync("⚠️ El contenido Pokémon esta bloqueado del tradeo!").ConfigureAwait(false);
                return;
            }

            var la = new LegalityAnalysis(pk);
            if (!la.Valid)
            {
                await ReplyAsync($"⚠️ **{typeof(T).Name}** el __archivo__ no es legal y no puede ser tradeado!").ConfigureAwait(false);
                return;
            }

            await QueueHelper<T>.AddToQueueAsync(Context, code, trainerName, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific, usr).ConfigureAwait(false);
        }
    }
}
