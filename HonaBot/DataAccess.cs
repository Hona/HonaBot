using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace HonaBot
{
    internal class DataAccess
    {
        public List<ServerConfigModel> GetServerConfigs()
        {
            using (IDbConnection connection =
                new SqlConnection(DatabaseHelper.ConnectionValue("HonaBot")))
            {
                return connection.Query<ServerConfigModel>("GetServerConfigs").ToList();
            }
        }

        public ulong GetAnnouncementChannel(ulong serverId)
        {
            using (IDbConnection connection =
                new SqlConnection(DatabaseHelper.ConnectionValue("HonaBot")))
            {
                return connection.Query<ServerConfigModel>("GetAnnouncementChannel @ServerId", new {ServerId = Convert.ToInt64(serverId)}).First().AnnouncementId;
            }
        }

        public void SetAnnouncementChannel(ulong channelId, ulong serverId)
        {
            using (IDbConnection connection =
                new SqlConnection(DatabaseHelper.ConnectionValue("HonaBot")))
            {
                connection.Execute("SetAnnouncementChannel @ServerId, @AnnouncementId",
                    new {ServerId = Convert.ToInt64(serverId), AnnouncementId = Convert.ToInt64(channelId)});
            }
        }

        public List<string> GetGiveableRoles(ulong serverId)
        {
            using (IDbConnection connection =
                new SqlConnection(DatabaseHelper.ConnectionValue("HonaBot")))
            {
                var rows = connection.Query<GiveableRoleModel>("GetGiveableRoles @GuildId",
                    new {GuildId = Convert.ToInt64(serverId)}).ToList();
                return rows.Select(role => role.RoleName).ToList();
            }
        }

        public void AddGiveableRole(ulong serverId, string roleName)
        {
            using (IDbConnection connection =
                new SqlConnection(DatabaseHelper.ConnectionValue("HonaBot")))
            {
                connection.Execute("AddGiveableRole @GuildId, @Role",
                    new {GuildId = Convert.ToInt64(serverId), Role = roleName});
            }
        }
    }
}