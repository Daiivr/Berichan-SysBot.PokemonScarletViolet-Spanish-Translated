using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace SysBot.Pokemon.Discord
{
    public class TradeStartModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private class TradeStartAction : ChannelAction<PokeRoutineExecutorBase, PokeTradeDetail<T>>
        {
            public TradeStartAction(ulong id, Action<PokeRoutineExecutorBase, PokeTradeDetail<T>> messager, string channel) : base(id, messager, channel)
            {
            }
        }

        private static readonly Dictionary<ulong, TradeStartAction> Channels = new();

        private static void Remove(TradeStartAction entry)
        {
            Channels.Remove(entry.ChannelID);
            SysCord<T>.Runner.Hub.Queues.Forwarders.Remove(entry.Action);
        }

#pragma warning disable RCS1158 // Static member in generic type should use a type parameter.
        public static void RestoreTradeStarting(DiscordSocketClient discord)
        {
            var cfg = SysCordSettings.Settings;
            foreach (var ch in cfg.TradeStartingChannels)
            {
                if (discord.GetChannel(ch.ID) is ISocketMessageChannel c)
                    AddLogChannel(c, ch.ID);
            }

            LogUtil.LogInfo("Added Trade Start Notification to Discord channel(s) on Bot startup.", "Discord");
        }

        public static bool IsStartChannel(ulong cid)
#pragma warning restore RCS1158 // Static member in generic type should use a type parameter.
        {
            return Channels.TryGetValue(cid, out _);
        }

        [Command("startHere")]
        [Summary("Hace que el comercio de registro de bot comience en el canal.")]
        [RequireSudo]
        public async Task AddLogAsync()
        {
            var c = Context.Channel;
            var cid = c.Id;
            if (Channels.TryGetValue(cid, out _))
            {
                await ReplyAsync("Ya se está registrando aquí.").ConfigureAwait(false);
                return;
            }

            AddLogChannel(c, cid);

            // Add to discord global loggers (saves on program close)
            SysCordSettings.Settings.TradeStartingChannels.AddIfNew(new[] { GetReference(Context.Channel) });
            await ReplyAsync("Salida de notificación de inicio agregada a este canal!").ConfigureAwait(false);
        }

        private static void AddLogChannel(ISocketMessageChannel c, ulong cid)
        {
            void Logger(PokeRoutineExecutorBase bot, PokeTradeDetail<T> detail)
            {
                if (detail.Type == PokeTradeType.Random)
                    return;
                c.SendMessageAsync(GetMessage(bot, detail));
            }

            Action<PokeRoutineExecutorBase, PokeTradeDetail<T>> l = Logger;
            SysCord<T>.Runner.Hub.Queues.Forwarders.Add(l);
            static string GetMessage(PokeRoutineExecutorBase bot, PokeTradeDetail<T> detail) => $"> [{DateTime.Now:hh:mm:ss}] - {bot.Connection.Label} is now trading (ID {detail.ID}) {detail.Trainer.TrainerName}";

            var entry = new TradeStartAction(cid, l, c.Name);
            Channels.Add(cid, entry);
        }

        [Command("startInfo")]
        [Summary("Copia la configuración de notificación de inicio.")]
        [RequireSudo]
        public async Task DumpLogInfoAsync()
        {
            foreach (var c in Channels)
                await ReplyAsync($"{c.Key} - {c.Value}").ConfigureAwait(false);
        }

        [Command("startClear")]
        [Summary("Borra la configuración de notificación de inicio en ese canal específico.")]
        [RequireSudo]
        public async Task ClearLogsAsync()
        {
            var cfg = SysCordSettings.Settings;
            if (Channels.TryGetValue(Context.Channel.Id, out var entry))
                Remove(entry);
            cfg.TradeStartingChannels.RemoveAll(z => z.ID == Context.Channel.Id);
            await ReplyAsync($"Notificaciones borradas del canal: {Context.Channel.Name}").ConfigureAwait(false);
        }

        [Command("startClearAll")]
        [Summary("Borra todas las configuraciones de notificación de inicio.")]
        [RequireSudo]
        public async Task ClearLogsAllAsync()
        {
            foreach (var l in Channels)
            {
                var entry = l.Value;
                await ReplyAsync($"Registro borrado de: {entry.ChannelName} ({entry.ChannelID}!").ConfigureAwait(false);
                SysCord<T>.Runner.Hub.Queues.Forwarders.Remove(entry.Action);
            }
            Channels.Clear();
            SysCordSettings.Settings.TradeStartingChannels.Clear();
            await ReplyAsync("Notificaciones borradas de todos los canales!").ConfigureAwait(false);
        }

        private RemoteControlAccess GetReference(IChannel channel) => new()
        {
            ID = channel.Id,
            Name = channel.Name,
            Comment = $"Agregado por: {Context.User.Username} el {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
        };
    }
}
