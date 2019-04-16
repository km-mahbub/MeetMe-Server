using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MeetMe.Entities;
using MeetMe.Infrastructure;

namespace MeetMe.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<PagedList<User>> GetUsers(UserParams userParams);
        Task<User> GetUser(int id);
    }
}
