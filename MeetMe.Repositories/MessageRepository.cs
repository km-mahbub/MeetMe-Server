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
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        private readonly MeetMeDbContext _context;
        public MessageRepository(MeetMeDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false && u.IsRead == false);
                    break;
            }

            messages = messages.OrderByDescending(d => d.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => (m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId) || (m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted == false))
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();

            return messages;
        }
    }
}
