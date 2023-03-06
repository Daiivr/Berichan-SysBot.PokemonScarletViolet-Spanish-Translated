using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using System;
using System.Linq;

namespace SysBot.Pokemon.Discord
{
    public class DiscordTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        public string IdentifierLocator => "Discord";
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private SocketUser Trader { get; }
        private ISocketMessageChannel CommandSentChannel { get; }
        public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }
        public int QueueSizeEntry { get; set; }
        public bool ReminderSent { get; set; } = false;

        public readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;

        public DiscordTradeNotifier(T data, PokeTradeTrainerInfo info, int code, SocketUser trader, ISocketMessageChannel commandSentChannel)
        {
            Data = data;
            Info = info;
            Code = code;
            Trader = trader;
            CommandSentChannel = commandSentChannel;

            QueueSizeEntry = Hub.Queues.Info.Count;
        }

        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            Trader.SendMessageAsync($"Iniciando __trade__ **{receive}**. Porfavor este atento. Tu codigo es: **{Code:0000 0000}**.").ConfigureAwait(false);
        }

        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $" {name}";
            Trader.SendMessageAsync($"Estoy esperando por ti, **{trainer}**! __Tienes **40 segundos**__. Tu codigo es: **{Code:0000 0000}**. Mi __IGN__ es: **{routine.InGameName}**.").ConfigureAwait(false);
        }

        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            OnFinish?.Invoke(routine);
            Trader.SendMessageAsync($"✘ Trade __cancelado__: {msg}").ConfigureAwait(false);
            if (msg == PokeTradeResult.NoTrainerWasFound)
                CommandSentChannel.SendMessageAsync($"⚠️ {Trader.Mention} ➜ Algo pasó con tu **trade**: {msg}. Su solicitud ha sido __**eliminada**__.");
        }

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {
            OnFinish?.Invoke(routine);
            var tradedToUser = Data.Species;
            var message = tradedToUser != 0 ? $"✓ Trade finalizado. Disfruta de tu **{(Species)tradedToUser}**!" : "Trade finalizado!";
            Trader.SendMessageAsync(message).ConfigureAwait(false);
            if (result.Species != 0 && Hub.Config.Discord.ReturnPKMs)
                Trader.SendPKMAsync(result, "Aqui esta lo que me enviaste!").ConfigureAwait(false);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            Trader.SendMessageAsync(message.TrimStart("SSR")).ConfigureAwait(false);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
        {
            if (message.ExtraInfo is SeedSearchResult r)
            {
                SendNotificationZ3(r);
                return;
            }

            var msg = message.Summary;
            if (message.Details.Count > 0)
                msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
            Trader.SendMessageAsync(msg).ConfigureAwait(false);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
        {
            if (result.Species != 0 && (Hub.Config.Discord.ReturnPKMs || info.Type == PokeTradeType.Dump))
                Trader.SendPKMAsync(result, message).ConfigureAwait(false);
        }

        private void SendNotificationZ3(SeedSearchResult r)
        {
            var lines = r.ToString();

            var embed = new EmbedBuilder { Color = Color.LighterGrey };
            embed.AddField(x =>
            {
                x.Name = $"Seed: {r.Seed:X16}";
                x.Value = lines;
                x.IsInline = false;
            });
            var msg = $"Aqui estan los detalles para: `{r.Seed:X16}`:";
            Trader.SendMessageAsync(msg, embed: embed.Build()).ConfigureAwait(false);
        }

        public void SendReminder(int position, string message)
        {
            if (ReminderSent)
                return;
            ReminderSent = true;
            Trader.SendMessageAsync($"**[**Recordatorio**]** {Trader.Mention} ➜ Tu posición actual es: **{position}**. Su trade comenzará pronto!");
        }
    }
}
