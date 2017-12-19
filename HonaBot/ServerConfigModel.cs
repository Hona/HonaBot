namespace HonaBot
{
    internal class ServerConfigModel
    {
        public ulong ServerId { get; set; }
        public ulong AnnouncementId { get; set; }

        public override string ToString()
        {
            return $"ServerID: {ServerId}\nAnnouncementID: {AnnouncementId}\n";
        }
    }
}