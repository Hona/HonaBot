using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonaBot
{
    public class BotStats
    {
        private DateTime _startTime;
        public BotStats()
        {
            _startTime = DateTime.Now;
        }
        public long CommandsCount { get; set; }
        public string UpTime => (DateTime.Now - _startTime).ToString("g");
    }
}
