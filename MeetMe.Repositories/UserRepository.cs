using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetMe.Entities;
using MeetMe.Infrastructure;
using MeetMe.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeetMe.Repositories
{
    public class UserRepository: Repository<User>, IUserRepository
    {
        private readonly MeetMeDbContext _context;
        public UserRepository(MeetMeDbContext context) : base(context)
        {
            _context = context;
        }
        
        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();

            users = users.Where(u => u.Id != userParams.UserId);

            users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Any(liker => liker.LikerId == u.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Any(likee => likee.LikeeId == u.Id));
            }


            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                //users = users.Where(u =>
                //    u.DateOfBirth.CalculateAge() >= userParams.MinAge &&
                //    u.DateOfBirth.CalculateAge() <= userParams.MaxAge);

                var min = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var max = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= min && u.DateOfBirth <= max);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<Like>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
                .Include(x => x.Likee)
                .Include(x => x.Liker)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (likers)
            {
                return user.Likee.Where(u => u.LikeeId == id);
            }
            else
            {
                return user.Liker.Where(u => u.LikerId == id);
            }
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }
    }
}
