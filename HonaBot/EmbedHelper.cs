using Discord;

namespace HonaBot
{
    public static class EmbedHelper
    {
        public static Embed CreateEmbed(string text, Color color)
        {
            return new EmbedBuilder
            {
                Description = text,
                Color = color
            };
        }

        public static Embed CreateEmbed(string title, string text, Color color)
        {
            return new EmbedBuilder
            {
                Title = title,
                Description = text,
                Color = color
            };
        }
    }
}