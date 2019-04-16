using MeetMe.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using MeetMe.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MeetMe.Repositories
{
    public class AuthRepository: Repository<User>, IAuthRepository
    {
        private readonly MeetMeDbContext _context;

        public AuthRepository(MeetMeDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> Login(string userName, string password)
        {
            var user= await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x=>x.Username == userName);
            if (user == null)
            {
                return null;
            }
            if(!VarifPassworHash(password,  user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return user;

        }

       
        public async Task<User> Register(User user, string password)
        {
            byte[] passwordSalt, passwordHash;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;


        }

        public async Task<bool> UserExists(string userName)
        {
            if(await _context.Users.AnyAsync( x=>x.Username == userName))
            {
                return true;
            }

            return false;

        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            }
        }

        private bool VarifPassworHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {

                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for(int i=0; i<computedHash.Length; i++)
                {
                    if(computedHash[i] != passwordHash[i] )
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
