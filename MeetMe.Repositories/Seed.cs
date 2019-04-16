using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetMe.Entities;

namespace MeetMe.Repositories
{
    public class Seed
    {
        private readonly MeetMeDbContext _context;

        public Seed(MeetMeDbContext context)
        {
            _context = context;
        }

        public void SeedUsers()
        {
            //_context.Users.RemoveRange(_context.Users);
            //_context.SaveChanges();

            //seed users
            var userData = System.IO.File.ReadAllText("../MeetMe.Repositories/UserSeedData.json");
            var users = JsonConvert.DeserializeObject<List<User>>(userData);
            foreach (var user in users)
            {
                //create the password hash 
                CreatePasswordHash("password", out var passwordHash, out var passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.Username = user.Username.ToLower();

                _context.Users.Add(user);
            }

            _context.SaveChanges();

        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            }
        }

    }
}
