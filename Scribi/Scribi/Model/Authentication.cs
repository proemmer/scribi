using System;

namespace Scribi.Models
{
    public class Authentication
    {
        public bool Authenticated { get; set; }
        public string User { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public DateTime? TokenExpires { get; set; }
    }
}
