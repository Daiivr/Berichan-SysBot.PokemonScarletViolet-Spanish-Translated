# Un tradebot SysBot.NET personalizado para Pokémon Escarlata/Violeta
![License](https://img.shields.io/badge/License-AGPLv3-blue.svg)

Este es el bot de tradeo que se ejecuta en el canal de Twitch de [berichan](https://twitch.tv/berichandev/)
Si todo lo que quieres es solicitar Pokémon, ¡ve allí!

Este es un sysbot personalizado traducido al español, la funcionalidad Scarlet/Violet de este bot no fue creada ni admitida por los desarrolladores de PKHeX, ¡no los molestes!
[Lea las instrucciones oficiales en la wiki](https://github.com/kwsch/SysBot.NET/wiki/Bot-Startup-Details) si nunca ha alojado un sysbot.

## Supporte Discord:

Si necesita ayuda para configurar su propia instancia de este SysBot, ¡no dude en unirse al discord! (Este sysbot no es compatible con el discord de PKHeX-Projects, no pida ayuda allí ni los moleste con preguntas relacionadas con este bot).

[<img src="https://canary.discordapp.com/api/guilds/771477382409879602/widget.png?style=banner2">](https://discord.gg/berichan)

[sys-botbase](https://github.com/olliz0r/sys-botbase) cliente para la automatización del control remoto de las consolas Nintendo Switch.

## SysBot.Base:
- Base logic library to be built upon in game-specific projects.
- Contains a synchronous and asynchronous Bot connection class to interact with sys-botbase.

## SysBot.Tests:
- Unit Tests for ensuring logic behaves as intended :)

# Example Implementations

The driving force to develop this project is automated bots for Nintendo Switch Pokémon games. An example implementation is provided in this repo to demonstrate interesting tasks this framework is capable of performing. Refer to the [Wiki](https://github.com/kwsch/SysBot.NET/wiki) for more details on the supported Pokémon features.

## SysBot.Pokemon:
- Class library using SysBot.Base to contain logic related to creating & running Pokémon bots.

## SysBot.Pokemon.WinForms:
- Simple GUI Launcher for adding, starting, and stopping Pokémon bots (as described above).
- Configuration of program settings is performed in-app and is saved as a local json file.

## SysBot.Pokemon.Discord:
- Discord interface for remotely interacting with the WinForms GUI.
- Provide a discord login token and the Roles that are allowed to interact with your bots.
- Commands are provided to manage & join the distribution queue.

## SysBot.Pokemon.Twitch:
- Twitch.tv interface for remotely announcing when the distribution starts.
- Provide a Twitch login token, username, and channel for login.

## SysBot.Pokemon.YouTube:
- YouTube.com interface for remotely announcing when the distribution starts.
- Provide a YouTube login ClientID, ClientSecret, and ChannelID for login.

Uses [Discord.Net](https://github.com/discord-net/Discord.Net) , [TwitchLib](https://github.com/TwitchLib/TwitchLib) and [StreamingClientLibary](https://github.com/SaviorXTanren/StreamingClientLibrary) as a dependency via Nuget.

## Other Dependencies
Pokémon API logic is provided by [PKHeX](https://github.com/kwsch/PKHeX/), and template generation is provided by [AutoMod](https://github.com/architdate/PKHeX-Plugins/).

# License
Refer to the `License.md` for details regarding licensing.
