using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace HonaBot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private DataAccess _db;
        private BotStats _botStats = new BotStats();
        public static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            _db = new DataAccess();
            _client = new DiscordSocketClient(new DiscordSocketConfig());
            _commands = new CommandService();

            _client.Log += Log;

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_botStats)
                .BuildServiceProvider();

            await InstallCommandsAsync();

            const string token = "Mzc3MDQxNzQ5OTY2NzE2OTMw.DOHNVQ.7hkqJrR_4J-GVI2FNCYPi1es8-Y";

            await _client.SetGameAsync("with settings | !help");

            _client.UserJoined += AnnounceJoinedUser;
            _client.UserLeft += AnnounceUserLeft;
            _client.UserBanned += _client_UserBanned;
            _client.UserUnbanned += _client_UserUnbanned;
            _client.JoinedGuild += _client_JoinedGuild;
            _client.MessageUpdated += _client_MessageUpdated;

            _client.Ready += _client_Ready;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task _client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            await HandleCommandAsync(arg2);
        }

        private async Task _client_JoinedGuild(SocketGuild arg)
        {
            var channel = arg.DefaultChannel;
            if (channel != null)
            {
                await channel.SendMessageAsync("",false,EmbedHelper.CreateEmbed(
                    "Thank you for using HonaBot! Please PM `Hona#0856` with suggestions or bugs.\r\nPlease configure the bots announcement channel with `!announcechannel <#channel>`", Color.Gold));
            }
            else
            {
                await arg.Owner.SendMessageAsync(
                    "Thank you for using HonaBot! Please PM `Hona#0856` with suggestions or bugs.");
                await arg.Owner.SendMessageAsync(
                    "Please configure the bots announcement channel with `!announcechannel <#channel>`");
            }
        }

        private async Task _client_Ready()
        {
            await Log(new LogMessage(LogSeverity.Info, "HonaBot", "Connected servers: " + _client.Guilds.Count));
        }

        private async Task _client_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            if (_client.GetChannel(_db.GetAnnouncementChannel(arg2.Id)) is SocketTextChannel channel)
                await channel.SendMessageAsync($"{arg1} has been unbanned! Thank gaben!");
        }

        private async Task _client_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            if (_client.GetChannel(_db.GetAnnouncementChannel(arg2.Id)) is SocketTextChannel channel)
                await channel.SendMessageAsync($"{arg1} has been banned. What have you done...");
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += HandleCommandAsync;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) ||
                  message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new SocketCommandContext(_client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            _botStats.CommandsCount++;
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync("", false,
                    EmbedHelper.CreateEmbed(result.ErrorReason, Color.Orange));
        }

        private async Task AnnounceJoinedUser(SocketGuildUser user) //welcomes New Players
        {
            if (_client.GetChannel(_db.GetAnnouncementChannel(user.Guild.Id)) is SocketTextChannel channel)
                await channel.SendMessageAsync("Welcome " + user.Mention + " to the server!");
        }

        private async Task AnnounceUserLeft(SocketGuildUser user) //welcomes New Players
        {
            if (_client.GetChannel(_db.GetAnnouncementChannel(user.Guild.Id)) is SocketTextChannel channel)
                await channel.SendMessageAsync(user.Username.Split('#')[0] +
                                               " has left... farewell");
        }
    }
}