using System.Configuration;

namespace HonaBot
{
    public static class DatabaseHelper
    {
        public static string ConnectionValue(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
    }
}