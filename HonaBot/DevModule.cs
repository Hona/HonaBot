using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace HonaBot
{
    public class DevModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly DataAccess _db = new DataAccess();

        private readonly BotStats _botStats;
        // Dependencies can be injected via the constructor
        public DevModule(DiscordSocketClient client, BotStats botStats)
        {
            _client = client;
            _botStats = botStats;
        }

        [Command("listservers")]
        [Summary("Echos a message.")]
        [RequireOwner]
        public async Task ListServers()
        {
            //var db = new DataAccess();
            //var serverConfigs = db.GetServerConfigs();
            //var replyList = (from serverConfigModel in serverConfigs
            //    let socketTextChannel = _client.GetChannel(serverConfigModel.AnnouncementId) as SocketTextChannel
            //    where socketTextChannel != null
            //    select
            //    $"Server: {_client.GetGuild(serverConfigModel.ServerId).Name} ({serverConfigModel.ServerId}){Environment.NewLine}Channel: {socketTextChannel.Name} ({serverConfigModel.AnnouncementId})"
            //).ToList();
            await ReplyAsync("", false,
                EmbedHelper.CreateEmbed(
                    string.Join(Environment.NewLine + Environment.NewLine,
                        _client.Guilds.ToList().ConvertAll(x => x.Name)), Color.Gold));
        }

        [Command("listchannels")]
        [Summary("Echos a message.")]
        [RequireOwner]
        public async Task ListChannels()
        {
            await ReplyAsync("", false,
                EmbedHelper.CreateEmbed(
                    Context.Guild.TextChannels.Aggregate("", (x, y) => x + y.Mention + Environment.NewLine),
                    Color.Gold));
        }

        [Command("announceall")]
        [Summary("Echos a message.")]
        [RequireOwner]
        public async Task AnnounceAll([Remainder] string text)
        {
            var servers = _db.GetServerConfigs();
            foreach (var server in servers)
                if (_client.GetChannel(server.AnnouncementId) is SocketTextChannel textChannel)
                    await textChannel.SendMessageAsync(text);
        }

        [Command("testembed")]
        [RequireOwner]
        public async Task Embed([Remainder] string text)
        {
            await ReplyAsync("", false, EmbedHelper.CreateEmbed(text, Color.Gold));
        }

        [Command("snoop")]
        [RequireOwner]
        public async Task Snoop([Remainder] string server)
        {
            var guild = _client.Guilds.ToList()
                .FirstOrDefault(x => string.Equals(x.Name, server, StringComparison.CurrentCultureIgnoreCase));
            if (guild == null)
            {
                await ReplyAsync("", false, EmbedHelper.CreateEmbed("Snoop", "Guild not found", Color.Orange));
            }
            else
            {
                var embedBuilder = new EmbedBuilder {Title = $"**Snoop - {server}**"};
                embedBuilder.Description += "**Members - " + guild.MemberCount + "**" + Environment.NewLine;
                embedBuilder.Description += string.Join(", ", guild.Users.ToList().Where(x => !x.IsBot));
                embedBuilder.Description += Environment.NewLine;
                embedBuilder.Description += "**Bots**" + Environment.NewLine;
                embedBuilder.Description += string.Join(", ", guild.Users.ToList().Where(x => x.IsBot));
                embedBuilder.Description += Environment.NewLine;
                embedBuilder.Description += "**Roles**" + Environment.NewLine;
                embedBuilder.Description += string.Join(", ", guild.Roles) + Environment.NewLine;
                embedBuilder.Description += "**Channels**" + Environment.NewLine;
                embedBuilder.Description += string.Join(", ", guild.TextChannels) + Environment.NewLine +
                                            Environment.NewLine;
                await ReplyAsync("", false, embedBuilder);
            }
        }
        [Command("getannounce")]
        [RequireOwner]
        public async Task GetAnnouncementChannel()
        {
            await ReplyAsync("",false,EmbedHelper.CreateEmbed(_db.GetAnnouncementChannel(Context.Guild.Id).ToString(),Color.Gold));
        }
        [Command("stats")]
        [RequireOwner]
        public async Task Stats()
        {
            await ReplyAsync("", false, EmbedHelper.CreateEmbed($"Commands issued: {_botStats.CommandsCount}", Color.Gold));
        }
        [Command("uptime")]
        [RequireOwner]
        public async Task Uptime()
        {
            await ReplyAsync("", false, EmbedHelper.CreateEmbed($"Uptime: {_botStats.UpTime}", Color.Gold));
        }
    }
}