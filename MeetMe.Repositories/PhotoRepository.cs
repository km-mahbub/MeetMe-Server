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
    public class PhotoRepository : Repository<Photo>, IPhotoRepository
    {
        private readonly MeetMeDbContext _context;
        public PhotoRepository(MeetMeDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }
    }
}
