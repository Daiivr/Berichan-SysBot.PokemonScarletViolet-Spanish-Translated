using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;

namespace SysBot.Pokemon.Twitch
{
    public static class TwitchCommandsHelper<T> where T : PKM, new()
    {
        // Helper functions for commands
        public static bool AddToWaitingList(string setstring, string display, string username, ulong mUserId, bool sub, out string msg)
        {
            string msgAddParams = string.Empty;

            if (!TwitchBot<T>.Info.GetCanQueue() || !TwitchBot<T>.CanQueueTwitch)
            {
                msg = "❌ Lo siento, actualmente no acepto solicitudes de cola.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(setstring))
            {
                msg = $"⚠️ @{username}: ¡Tienes que pedir algo! Incluye el nombre del Pokémon en tu orden.";
                return false;
            }

            try
            {
                PKM? pkm = PokemonPool<T>.TryFetchFromDistributeDirectory(TwitchBot<T>.Hub.Config.Folder.DistributeFolder, setstring.Trim());
                string result = string.Empty;

                if (pkm == null)
                {
                    var set = ShowdownUtil.ConvertToShowdown(setstring);
                    if (set == null)
                    {
                        msg = $"⚠️ Omitiendo el intercambio, @{username}: Apodo vacío proporcionado para la especie.";
                        return false;
                    }
                    var template = AutoLegalityWrapper.GetTemplate(set);
                    if (template.Species < 1)
                    {
                        msg = $"⚠️ Omitiendo el intercambio, @{username}: Por favor, lea lo que se supone que debe escribir como argumento del comando, asegúrese de que el nombre de su especie y las líneas de personalización son correctas.";
                        return false;
                    }

                    if (set.InvalidLines.Count != 0)
                    {
                        msg = $"⚠️ Omitiendo el intercambio, @{username}: No se puede analizar el conjunto Showdown:\n{string.Join("\n", set.InvalidLines)}";
                        return false;
                    }

                    var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                    pkm = sav.GetLegal(template, out result);
                }

                if (pkm == null)
                {
                    msg = $"⚠️ Omitiendo el intercambio, @{username}: No se puede legalizar el Pokémon.";
                    return false;
                }

                if (!pkm.CanBeTraded())
                {
                    msg = $"⚠️ Omitiendo el intercambio, El contenido Pokémon proporcionado está bloqueado para el comercio!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var la = new LegalityAnalysis(pkm);
                    var valid = la.Valid;
                    if (valid)
                    {
                        var tq = new TwitchQueue<T>(pk, new PokeTradeTrainerInfo(display, mUserId), username, sub);
                        TwitchBot<T>.QueuePool.RemoveAll(z => z.UserName == username); // remove old requests if any
                        TwitchBot<T>.QueuePool.Add(tq);
                        msg = $"MUY BIEN! @{username} ➜ Añadido a la lista de espera. Por favor, susúrrame tu código comercial de 8 dígitos! (Susurre a este bot, no el streamer) {msgAddParams}";
                        return true;
                    }
                }

                var reason = result == "Timeout" ? "El conjunto tardó demasiado en generarse." : "No se puede legalizar el Pokémon.";
                msg = $"⚠️ Omitiendo el intercambio, @{username}: {reason}";
            }

            catch (Exception ex)

            {
                LogUtil.LogSafe(ex, nameof(TwitchCommandsHelper<T>));
                msg = $"⚠️  Omitiendo el intercambio, @{username}: El comando o la sintaxis no son válidos.";
            }
            return false;
        }

        public static string ClearTrade(string user)
        {
            var result = TwitchBot<T>.Info.ClearTrade(user);
            return GetClearTradeMessage(result);
        }

        public static string ClearTrade(ulong userID, out bool wasInQueue)
        {
            var result = TwitchBot<T>.Info.ClearTrade(userID);
            wasInQueue = result != QueueResultRemove.NotInQueue;
            return GetClearTradeMessage(result);
        }

        private static string GetClearTradeMessage(QueueResultRemove result)
        {
            return result switch
            {
                QueueResultRemove.CurrentlyProcessing => "⚠️ ¡Parece que actualmente está siendo procesado! No se eliminó de la cola.",
                QueueResultRemove.CurrentlyProcessingRemoved => "⚠️  ¡Parece que actualmente está siendo procesado! Eliminado de la cola.",
                QueueResultRemove.Removed => "✔️ Eliminado de la cola.",
                _ => "⚠️ Lo sentimos, no estás actualmente en la cola.",
            };
        }

        public static string GetCode(ulong parse)
        {
            var detail = TwitchBot<T>.Info.GetDetail(parse);
            return detail == null
                ? "⚠️ Lo sentimos, no estás actualmente en la cola."
                : $"Su código comercial es {detail.Trade.Code:0000 0000}";
        }
    }
}
