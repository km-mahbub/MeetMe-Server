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
    [ServiceFilter((typeof(LogUserActivity)))]
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _unitOfWork.Users.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _unitOfWork.Users.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _unitOfWork.Users.GetUser(id);

            var userToReturn = _mapper.Map<UserForDetailDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserForUpdateDto userForUpdateDto)
        {
            //throw new Exception("Computer says no");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _unitOfWork.Users.GetUser(id);
            if (userFromRepo == null)
            {
                return NotFound($"Could not find user with and ID of {id}");
            }

            if (currentUserId != userFromRepo.Id)
            {
                return Unauthorized();
            }

            _mapper.Map(userForUpdateDto, userFromRepo);

            if (await _unitOfWork.SaveAll())
            {
                return NoContent();
            }

            throw new Exception($"update user {id} failed on save");


        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var like = await _unitOfWork.Likes.GetLike(id, recipientId);

            if (like != null)
                return BadRequest("You already like this user");

            if (await _unitOfWork.Users.GetUser(recipientId) == null)
                return NotFound();

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            await _unitOfWork.Likes.Add(like);

            if (await _unitOfWork.SaveAll())
                return Ok(new {});

            return BadRequest("Failed to add user");

        }
    }
}