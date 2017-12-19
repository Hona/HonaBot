using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HonaBot
{
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        private const string HelpPage =
                "`!givemelist` or `!gml` - Lists all current giveable roles\r\n" +
                "`!giveme <role name>` - gives or takes a role specified\r\n" +
                "`!giveme <Role Name or @Mention Role>`\r\n" +
                "Admins can do `!adminhelp` - displays help on admin commands"
            ;

        private readonly DataAccess _db = new DataAccess();

        [Command("giveme")]
        [Alias("gm")]
        [Summary("Gives a role.")]
        public async Task GiveRoleAsync([Remainder] [Summary("Role")] string rolename)
        {
            try
            {
                var success = false;
                rolename = rolename.Trim(' ');
                var user = Context.User;
                var giveable = _db.GetGiveableRoles(Context.Guild.Id);
                var upperGiveable = giveable.ConvertAll(x => x.ToUpper());
                if (upperGiveable.Contains(rolename.ToUpper()))
                {
                    var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name.ToString(), rolename,
                        StringComparison.CurrentCultureIgnoreCase));
                    if (user == null)
                    {
                        await ReplyAsync("", false,
                            EmbedHelper.CreateEmbed("Could not give role", "User not found", Color.Orange));
                        return;
                    }
                    if (user is IGuildUser guildUser) await guildUser.AddRoleAsync(role);


                    await ReplyAsync("", false,
                        EmbedHelper.CreateEmbed("Giveme", $"Successfully added {user.Mention} to **{role.Name}**",
                            Color.Green));
                    success = true;
                }
                if (!success)
                {
                    var entries = Context.Guild.Roles.ToList().ConvertAll(x => x.Mention);
                    if (entries.Contains(rolename))
                    {
                        var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Mention, rolename,
                            StringComparison.CurrentCultureIgnoreCase));
                        if (user == null)
                        {
                            await ReplyAsync("", false,
                                EmbedHelper.CreateEmbed("Could not give role", "User not found", Color.Orange));
                            return;
                        }
                        if (user is IGuildUser guildUser) await guildUser.AddRoleAsync(role);
                        await ReplyAsync("", false,
                            EmbedHelper.CreateEmbed("Giveme", $"Successfully added {user.Mention} to **{role.Name}**",
                                Color.Green));
                        success = true;
                    }
                }
                if (!success)
                    await ReplyAsync("", false,
                        EmbedHelper.CreateEmbed("Could not give role", "That role cannot be added.", Color.Orange));
            }
            catch
            {
                await ReplyAsync("", false,
                    EmbedHelper.CreateEmbed("!giveme - Error",
                        "The HonaBot role is below the role you are trying to obtain. Notify an admin to move HonaBot higher in the roles hierarchy.",
                        Color.Orange));
            }
        }

        [Command("givemelist")]
        [Alias("gml")]
        [Summary("Lists roles giveable")]
        public async Task GiveRoleListAsync()
        {
            var list = _db.GetGiveableRoles(Context.Guild.Id);
            if (list.Count != 0)
                await ReplyAsync("", false,
                    EmbedHelper.CreateEmbed("Giveable Roles", string.Join(", ", _db.GetGiveableRoles(Context.Guild.Id)),
                        Color.Gold));
            else
                await ReplyAsync("", false, EmbedHelper.CreateEmbed("No giveable roles...", Color.Orange));
        }

        [Command("help")]
        [Summary("Displays the help message.")]
        public async Task SayHelpAsync(int page = 0)
        {
            // ReplyAsync is a method on ModuleBase
            await ReplyAsync("", false, EmbedHelper.CreateEmbed("Help", HelpPage, Color.Gold));
        }
    }
}