using API.DTO;
using API.Entities;
using API.Heplers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public void AddMessage(Message message)
        {
            _dataContext.Messages.Add(message); 
        }

        public void DeleteMessage(Message message)
        {
            _dataContext.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _dataContext.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            IQueryable<Message> query = _dataContext.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && u.SenderDeleted == false),
                _ => query.Where(x=>x.RecipientUsername == messageParams.Username && x.RecipientDeleted == false && x.DateRead == null)
            };

            IQueryable<MessageDto> messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesThreate(string currentUserName, string receipientUserName)
        {
            var messages = await _dataContext.Messages
                                                    .Include(x => x.Sender).ThenInclude(p=>p.Photos)
                                                    .Include(x => x.Recipient).ThenInclude(p=>p.Photos)
                                                    .Where(m=>m.RecipientUsername == currentUserName && m.SenderUsername == receipientUserName && m.RecipientDeleted == false
                                                                || m.RecipientUsername == receipientUserName && m.SenderUsername == currentUserName && m.SenderDeleted == false)
                                                    .OrderBy(x=>x.MessageSent)
                                                    .ToListAsync();

            var unreadMessages = messages.Where(x=>x.DateRead == null && x.RecipientUsername == currentUserName).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                    message.DateRead = DateTime.UtcNow;

                await _dataContext.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }
    }
}
