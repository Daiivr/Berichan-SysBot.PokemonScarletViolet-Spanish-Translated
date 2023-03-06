using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;
using PKHeX.Core;
using System.Threading.Tasks;
using PKHeX.Core.AutoMod;

namespace SysBot.Pokemon.Discord
{
    public static class QueueHelper<T> where T : PKM, new()
    {
        private const uint MaxTradeCode = 9999_9999;

        public static async Task AddToQueueAsync(SocketCommandContext context, int code, string trainer, RequestSignificance sig, T trade, PokeRoutineType routine, PokeTradeType type, SocketUser trader)
        {
            if ((uint)code > MaxTradeCode)
            {
                await context.Channel.SendMessageAsync("El codigo de tradeo debe ser un numero entre: 00000000-99999999!").ConfigureAwait(false);
                return;
            }

            try
            {
                const string helper = "✓ Te he añadido a la __lista__! Te enviaré un __mensaje__ aquí cuando comience tu operación...";
                IUserMessage test = await trader.SendMessageAsync(helper).ConfigureAwait(false);

                // Try adding
                var result = AddToTradeQueue(context, trade, code, trainer, sig, routine, type, trader, out var msg);

                // Notify in channel
                await context.Channel.SendMessageAsync(msg).ConfigureAwait(false);
                // Notify in PM to mirror what is said in the channel.
                await trader.SendMessageAsync($"{msg}\nTu codigo de tradeo sera: **{code:0000 0000}**.").ConfigureAwait(false);

                // Clean Up
                if (result)
                {
                    // Delete the user's join message for privacy
                    if (!context.IsPrivate)
                        await context.Message.DeleteAsync(RequestOptions.Default).ConfigureAwait(false);
                }
                else
                {
                    // Delete our "I'm adding you!", and send the same message that we sent to the general channel.
                    await test.DeleteAsync().ConfigureAwait(false);
                }
            }
            catch (HttpException ex)
            {
                await HandleDiscordExceptionAsync(context, trader, ex).ConfigureAwait(false);
            }
        }

        public static async Task AddToQueueAsync(SocketCommandContext context, int code, string trainer, RequestSignificance sig, T trade, PokeRoutineType routine, PokeTradeType type)
        {
            await AddToQueueAsync(context, code, trainer, sig, trade, routine, type, context.User).ConfigureAwait(false);
        }

        private static bool AddToTradeQueue(SocketCommandContext context, T pk, int code, string trainerName, RequestSignificance sig, PokeRoutineType type, PokeTradeType t, SocketUser trader, out string msg)
        {
            var user = trader;
            var userID = user.Id;
            var name = user.Username;

            var trainer = new PokeTradeTrainerInfo(trainerName, userID);
            var notifier = new DiscordTradeNotifier<T>(pk, trainer, code, user, context.Channel);
            var detail = new PokeTradeDetail<T>(pk, trainer, notifier, t, code, sig == RequestSignificance.Favored);
            var trade = new TradeEntry<T>(detail, userID, type, name);

            var hub = SysCord<T>.Runner.Hub;
            var Info = hub.Queues.Info;
            var added = Info.AddToTradeQueue(trade, userID, sig == RequestSignificance.Owner);

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                msg = "✘ Lo siento, ya estás en la cola..";
                return false;
            }

            var position = Info.CheckPosition(userID, type);
            notifier.QueueSizeEntry = position.Position;

            var ticketID = "";
            if (TradeStartModule<T>.IsStartChannel(context.Channel.Id))
                ticketID = $", unique ID: {detail.ID}";

            var pokeName = "";
            if (t == PokeTradeType.Specific && pk.Species != 0)
                pokeName = $" Recibiendo: **{(Species)pk.Species}**.";
            msg = $"{user.Mention} ➜ Agregado al **{type}**{ticketID}. Posicion actual: **{position.Position}**.{pokeName}";

            var botct = Info.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                msg += $" Tiempo estimado: **{eta:F1}** minutos.";
            }
            return true;
        }

        private static async Task HandleDiscordExceptionAsync(SocketCommandContext context, SocketUser trader, HttpException ex)
        {
            string message = string.Empty;
            switch (ex.DiscordCode)
            {
                case DiscordErrorCode.InsufficientPermissions or DiscordErrorCode.MissingPermissions:
                    {
                        // Check if the exception was raised due to missing "Send Messages" or "Manage Messages" permissions. Nag the bot owner if so.
                        var permissions = context.Guild.CurrentUser.GetPermissions(context.Channel as IGuildChannel);
                        if (!permissions.SendMessages)
                        {
                            // Nag the owner in logs.
                            message = "¡Debes otorgarme permisos de \"Enviar mensajes\"!";
                            Base.LogUtil.LogError(message, "QueueHelper");
                            return;
                        }
                        else if (!permissions.ManageMessages)
                        {
                            var app = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
                            var owner = app.Owner.Id;
                            message = $"<@{owner}> ¡Debes otorgarme permisos de \"Enviar mensajes\"!";
                        }
                    }; break;
                case DiscordErrorCode.CannotSendMessageToUser:
                    {
                        // The user either has DMs turned off, or Discord thinks they do.
                        message = context.User == trader ? "✘ Debes __habilitar__ los mensajes privados para poder __intercambiar__ con el bot!" : "El usuario mencionado debe __habilitar__ los mensajes privados para poder tradear!";
                    }; break;
                default:
                    {
                        // Send a generic error message.
                        message = ex.DiscordCode != null ? $"Discord error {(int)ex.DiscordCode}: {ex.Reason}" : $"Http error {(int)ex.HttpCode}: {ex.Message}";
                    }; break;
            }
            await context.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
