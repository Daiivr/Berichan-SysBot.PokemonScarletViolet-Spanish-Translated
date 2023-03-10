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
- Biblioteca de lógica basica que se basará en proyectos específicos del juego.
- Contiene una clase de conexión Bot síncrona y asíncrona para interactuar con sys-botbase.

## SysBot.Tests:
- Pruebas unitarias para garantizar que la lógica se comporte según lo previsto :)

# Implementaciones de ejemplo

La fuerza impulsora para desarrollar este proyecto son los bots automatizados para los juegos Pokémon de Nintendo Switch. En este repositorio se proporciona un ejemplo de implementación para demostrar tareas interesantes que este marco es capaz de realizar. Consulte la [Wiki](https://github.com/kwsch/SysBot.NET/wiki) para obtener más detalles sobre las funciones de Pokémon admitidas.

## SysBot.Pokemon:
- Biblioteca de clases que usa SysBot.Base para contener la lógica relacionada con la creación y ejecución de bots Pokémon.

## SysBot.Pokemon.WinForms:
- Lanzador de GUI simple para agregar, iniciar y detener bots Pokémon (como se describe anteriormente).
- La configuración de los ajustes del programa se realiza en la aplicación y se guarda como un archivo json local.

## SysBot.Pokemon.Discord:
- Interfaz de Discord para interactuar de forma remota con la GUI de WinForms.
- Proporcione un token de inicio de sesión de discord y los roles que pueden interactuar con sus bots.
- Se proporcionan comandos para administrar y unirse a la cola de distribución.

## SysBot.Pokemon.Twitch:
- Interfaz Twitch.tv para anunciar de forma remota cuando comienza la distribución.
- Pproporcione un token de inicio de sesión de Twitch, un nombre de usuario y un canal para iniciar sesión.

## SysBot.Pokemon.YouTube:
- Interfaz de YouTube.com para anunciar de forma remota cuándo comienza la distribución.
- Proporcione un ID de cliente de inicio de sesión de YouTube, ClientSecret y ChannelID para iniciar sesión.

Usa [Discord.Net](https://github.com/discord-net/Discord.Net) , [TwitchLib](https://github.com/TwitchLib/TwitchLib) y [StreamingClientLibary](https://github.com/SaviorXTanren/StreamingClientLibrary) como una dependencia a través de Nuget.

## Otras dependencias
La lógica de la API de Pokémon proporcionada por: [PKHeX](https://github.com/kwsch/PKHeX/), y la generación de plantillas es proporcionada por: [AutoMod](https://github.com/architdate/PKHeX-Plugins/).

# Licencia
Consulte `License.md` para obtener detalles sobre la concesión de licencias.
