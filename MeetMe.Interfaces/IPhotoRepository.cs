using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MeetMe.Entities;

namespace MeetMe.Interfaces
{
    public interface IPhotoRepository : IRepository<Photo>
    {
        Task<Photo> GetPhoto(int id);
        Task<Photo> GetMainPhotoForUser(int userId);
    }
}
