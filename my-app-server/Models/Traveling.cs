using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class Traveling
    {
        public int HeroId { get; set; }
        public int Start { get; set; }
        public string StartName { get; set; }
        public int Target { get; set; }
        public string TargetName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsReverse { get; set; }
        public DateTime? ReverseTime { get; set; }

        public Heros Hero { get; set; }
    }
}
