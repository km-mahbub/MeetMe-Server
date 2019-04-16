using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetMe.Entities;
using MeetMe.Interfaces;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace MeetMe.Repositories
{
    public class LikeRepository : Repository<Like>, ILikeRepository
    {
        private readonly MeetMeDbContext _context;
        public LikeRepository(MeetMeDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }
    }
}
