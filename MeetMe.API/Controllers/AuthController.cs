using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MeetMe.Interfaces;
using MeetMe.Entities;
using MeetMe.API.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace MeetMe.API.Controllers
{

    [Route("api/Auth")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthController(IUnitOfWork unitOfWork, IConfiguration config, IMapper mapper)
        {
            _config = config;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserForRegisterDto userForRegisterDto)
        {
            if (!string.IsNullOrEmpty(userForRegisterDto.Username))
            {
                userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            }

            if (await _unitOfWork.Auth.UserExists(userForRegisterDto.Username))
            {
                ModelState.AddModelError("Username", "Username already exists");
            }


            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var createdUser = await _unitOfWork.Auth.Register(userToCreate, userForRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailDto>(createdUser);

            return CreatedAtRoute("GetUser", new {controller = "Users", id = createdUser.Id}, userToReturn);

        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserForLoginDto userForLogin)
        {
            //throw new Exception("Computer Says No!");
            var userFromRepo = await _unitOfWork.Auth.Login(userForLogin.Username.ToLower(), userForLogin.Password);
            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.GetSection("AppSettings:Token").Value);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                    new Claim(ClaimTypes.Name , userFromRepo.Username)

                }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            var user = _mapper.Map<UserForListDto>(userFromRepo);

            return Ok(new { tokenString, user });



        }
    }
}

