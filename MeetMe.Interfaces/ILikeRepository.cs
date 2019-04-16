using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MeetMe.Entities;

namespace MeetMe.Interfaces
{
    public interface ILikeRepository: IRepository<Like>
    {
        Task<Like> GetLike(int userId, int recipientId);
    }
}
