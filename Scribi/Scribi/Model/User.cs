namespace Scribi.Models
{
    public enum UserType { ReadOnly, ReadWrite, Admin }

    public class User
    {
        public UserType Type { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
