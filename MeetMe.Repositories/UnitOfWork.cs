using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MeetMe.Interfaces;
using SQLitePCL;

namespace MeetMe.Repositories
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly MeetMeDbContext _context;

        public UnitOfWork
        (
            MeetMeDbContext context,
            IAuthRepository auth,
            IUserRepository user,
            ILikeRepository like,
            IPhotoRepository photo,
            IMessageRepository message
        )
        {
            _context = context;
            Auth = auth;
            Users = user;
            Likes = like;
            Photos = photo;
            Messages = message;
        }

        public IAuthRepository Auth { get; }
        public IUserRepository Users { get; }
        public ILikeRepository Likes { get; }
        public IPhotoRepository Photos { get; }
        public IMessageRepository Messages { get; }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
