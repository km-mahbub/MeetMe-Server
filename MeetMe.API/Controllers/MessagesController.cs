using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using MeetMe.API.Dtos;
using MeetMe.API.Helpers;
using MeetMe.Entities;
using MeetMe.Infrastructure;
using MeetMe.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetMe.API.Controllers
{
    [Authorize]
    [ServiceFilter((typeof(LogUserActivity)))]
    [Route("api/users/{userId}/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _unitOfWork.Messages.GetMessage(id);

            if (messageFromRepo == null)
                return NotFound();

            return Ok(messageFromRepo);
        }

        [HttpGet("thread/{id}")]
        public async Task<IActionResult> GetMessageThread(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messagesFromRepo = await _unitOfWork.Messages.GetMessageThread(userId, id);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            return Ok(messageThread);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messagesFromRepo = await _unitOfWork.Messages.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId,
            [FromBody]MessageForCreationDto messageForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDto.SenderId = userId;

            var recipient = await _unitOfWork.Users.GetUser(messageForCreationDto.RecipientId);
            var sender = await _unitOfWork.Users.GetUser(messageForCreationDto.SenderId);

            if (recipient == null)
                return BadRequest("Could not find user.");

            var message = _mapper.Map<Message>(messageForCreationDto);

            await _unitOfWork.Messages.Add(message);

            var messageToReturn = _mapper.Map<MessageToReturnDto>(message);

            if (await _unitOfWork.SaveAll())
                return CreatedAtRoute("GetMessage", new {id = message.Id}, messageToReturn);

            throw new Exception("Creating the message failed on save.");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _unitOfWork.Messages.GetMessage(id);

            if (messageFromRepo.SenderId == userId)
                messageFromRepo.SenderDeleted = true;
            if (messageFromRepo.RecipientId == userId)
                messageFromRepo.RecipientDeleted = true;

            if(messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
                _unitOfWork.Messages.Delete(messageFromRepo);

            if (await _unitOfWork.SaveAll())
                return NoContent();

            throw new Exception("Error deleting the message");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await _unitOfWork.Messages.GetMessage(id);

            if (message.RecipientId != userId)
                return BadRequest("Failed to mark message read");

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _unitOfWork.SaveAll();

            return NoContent();
        }
    }
}