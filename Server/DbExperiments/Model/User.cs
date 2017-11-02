using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbExperiments
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string EncodedPassword { get; set; }

        [NotMapped]
        public string Password
        {
            get { return FlipFlop(EncodedPassword); }
            set { EncodedPassword = FlipFlop(value); }
        }

        public string Email { get; set; }
        public bool IsEmailActivated { get; set; }
        public Role Role { get; set; }
        public Guid ZoneId { get; set; }
        public bool IsDefaultZoneUser { get; set; }



        private  string FlipFlop(string before)
        {
            return string.IsNullOrEmpty(before) ? "" : before.Substring(before.Length - 1, 1) + FlipFlop(before.Substring(0, before.Length - 1));
        }

       
    }
  
}