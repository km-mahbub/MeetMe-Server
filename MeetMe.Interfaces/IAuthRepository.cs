using MeetMe.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MeetMe.Interfaces
{
    public interface IAuthRepository: IRepository<User>
    {
        Task<User> Register(User user, string password);
        Task<User> Login(string userName, string password);
        Task<bool> UserExists(string userName);
    }
}
