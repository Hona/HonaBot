using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HonaBot
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private const string AdminHelpString = "`!purge <amount>` - deletes messages up to the specified number\r\n" +
                                               "`!announcechannel <mention channel (#channel)>` - sets the announcement channel for messages like join/leave\r\n" +
                                               "`!addgiveme <role name or role mention>` or `!agm <role>` - adds a role to be used by !giveme\r\n" +
                                               "`!givethem <user mention> <role name or role metion>` or `!gt <user> <role>` - adds a role to someone else\r\n" +
                                               "`!spam <message count> <message>` - repeats a message by the specified amount";

        private readonly DataAccess _db = new DataAccess();

        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [Command("purge")]
        public async Task PurgeAsync([Summary("Messages to purge")] uint amount)
        {
            var messages = await Context.Channel.GetMessagesAsync((int) amount + 1).Flatten();

            await Context.Channel.DeleteMessagesAsync(messages);
            const int delay = 3000;

            var m = await ReplyAsync("", false, EmbedHelper.CreateEmbed(
                $"Purge completed. _This message will be deleted in {delay / 1000} seconds._", Color.Gold));
            await Task.Delay(delay);
            await m.DeleteAsync();
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("announcechannel")]
        [Alias("ac")]
        public async Task SetAnnouncementChannel([Summary("channel")] string channel)
        {
            var channelTextChannel = Context.Guild.TextChannels.First(x => x.Mention == channel);
            if (channelTextChannel != null)
            {
                _db.SetAnnouncementChannel(channelTextChannel.Id, Context.Guild.Id);
                await ReplyAsync("", false, EmbedHelper.CreateEmbed("Set Announcement Channel",
                    "Channel set.", Color.Green));
            }
            else
            {
                await ReplyAsync("", false, EmbedHelper.CreateEmbed("Set Announcement Channel",
                    "Channel not found.", Color.Orange));
            }
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addgiveme")]
        [Alias("agm")]
        public async Task AddGiveMeRole([Remainder] [Summary("rolename")] string roleName)
        {
            var server = Context.Guild.Id;
            roleName = roleName.Trim(' ');
            if (roleName.Contains("@"))
            {
                if (Context.Guild.Roles.Where(x => x.Mention == roleName).ToList().Count != 0)
                {
                    var name = Context.Guild.Roles.First(x => x.Mention == roleName).Name;
                    _db.AddGiveableRole(server, name);
                    await ReplyAsync("", false, EmbedHelper.CreateEmbed("Add !giveme Role",
                        "Role added successfully.", Color.Green));
                }
                else
                {
                    await ReplyAsync("", false, EmbedHelper.CreateEmbed("Add !giveme Role",
                        "Role not found.", Color.Orange));
                }
            }
            else
            {
                if (Context.Guild.Roles.Where(x => x.Name == roleName).ToList().Count != 0)
                {
                    _db.AddGiveableRole(server, roleName);
                    await ReplyAsync("", false, EmbedHelper.CreateEmbed("Add !giveme Role",
                        "Role added successfully.", Color.Green));
                }
                else
                {
                    await ReplyAsync("", false, EmbedHelper.CreateEmbed("Add !giveme Role",
                        "Role not found.", Color.Orange));
                }
            }
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("givethem")]
        [Alias("gt")]
        [Summary("Gives a role.")]
        public async Task GiveRoleAsync(string userMention, [Remainder] [Summary("Role")] string rolename)
        {
            var success = false;
            rolename = rolename.Trim(' ');
            userMention = userMention.TrimStart('<', '@').TrimEnd('>');

            var user = Context.Guild.Users.FirstOrDefault(x => x.Id.ToString() == userMention);
            var giveable = _db.GetGiveableRoles(Context.Guild.Id);
            var upperGiveable = giveable.ConvertAll(x => x.ToUpper());
            if (upperGiveable.Contains(rolename.ToUpper()))
            {
                var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, rolename,
                    StringComparison.CurrentCultureIgnoreCase));
                if (user == null)
                {
                    await ReplyAsync("", false,
                        EmbedHelper.CreateEmbed("Could not give role", "User not found", Color.Orange));
                    return;
                }
                await ((IGuildUser) user).AddRoleAsync(role);
                if (role != null)
                    await ReplyAsync("", false,
                        EmbedHelper.CreateEmbed("Givethem", $"Successfully added {user.Mention} to **{role.Name}**",
                            Color.Green));
                else
                {
                    await ReplyAsync("", false, EmbedHelper.CreateEmbed("Could not give role","Role is null",Color.Orange));
                }
                success = true;
            }
            if (!success)
            {
                rolename = rolename.TrimStart('<', '@', '&').TrimEnd('>');

                var entries = Context.Guild.Roles.ToList().ConvertAll(x => x.Id.ToString());
                if (entries.Contains(rolename))
                {
                    var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Id.ToString(), rolename,
                        StringComparison.CurrentCultureIgnoreCase));
                    if (user == null)
                    {
                        await ReplyAsync("", false,
                            EmbedHelper.CreateEmbed("Could not give role", "User not found", Color.Orange));
                        return;
                    }
                    await ((IGuildUser) user).AddRoleAsync(role);
                    await ReplyAsync("", false,
                        EmbedHelper.CreateEmbed("Givethem", $"Successfully added {user.Mention} to **{role.Name}**",
                            Color.Green));
                    success = true;
                }
            }
            if (!success)
                await ReplyAsync("", false,
                    EmbedHelper.CreateEmbed("Givethem - Error", "That role cannot be added.", Color.Orange));
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("spam")]
        public async Task Spam(int times, [Remainder] string message)
        {
            for (var i = 0; i < times; i++)
            {
                await Context.Channel.SendMessageAsync(message);
                await Task.Delay(1234);
            }
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("adminhelp")]
        public async Task AdminHelp()
        {
            await ReplyAsync("", false, EmbedHelper.CreateEmbed("Admin Help",
                AdminHelpString, Color.Gold));
        }
    }
}