using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MeetMe.Interfaces
{
    public interface IUnitOfWork: IDisposable
    {
        IAuthRepository Auth { get; }
        IUserRepository Users { get; }
        ILikeRepository Likes { get; }
        IPhotoRepository Photos { get; }
        IMessageRepository Messages { get; }

        Task<bool> SaveAll();
    }
}
