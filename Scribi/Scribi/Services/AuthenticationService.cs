using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Scribi.Interfaces;
using Scribi.Models;

namespace Scribi.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private IEnumerable<User> users;
        public int ValidationTimeInMin { get; private set; }

        public void Configure(IConfigurationSection config)
        {
            users = new List<User>();
            config.GetSection("Users").Bind(users);
            ValidationTimeInMin = config.GetValue<int>("TokenValidatenTimeinMinutes");
        }

        public void Init()
        {
            
        }

        public void Release()
        {
        }

        public bool TryAuthorize(string username, string password, ref User user)
        {
            user = users.FirstOrDefault(x => x.Username == username);
            if (user == null)
                return false;
            if (!string.Equals(password, user.Password))
                user = null;
            return user != null;
        }
    }
}
