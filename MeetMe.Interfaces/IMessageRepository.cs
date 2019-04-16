using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MeetMe.Entities;
using MeetMe.Infrastructure;

namespace MeetMe.Interfaces
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<Message> GetMessage(int id);
        Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId);
    }
}
