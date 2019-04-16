using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MeetMe.API.Dtos;
using MeetMe.API.Helpers;
using MeetMe.Entities;
using MeetMe.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MeetMe.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private readonly Cloudinary _cloudinary;

        public PhotosController(IUnitOfWork unitOfWork,
            IMapper mapper,
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _unitOfWork.Photos.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, PhotoForCreationDto photoDto)
        {
            var user = await _unitOfWork.Users.GetUser(userId);
            if (user==null)
            {
                return BadRequest("Colud not found user");
            }
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (currentUserId!=user.Id)
            {
                return Unauthorized();
            }
            var file = photoDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoDto.Url = uploadResult.Uri.ToString();
            photoDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoDto);

            photo.User = user;

            if (!user.Photos.Any(m => m.IsMain))
                photo.IsMain = true;

            user.Photos.Add(photo);

            

            if (await _unitOfWork.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }

            return BadRequest("Could not add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var photoFromRepo = await _unitOfWork.Photos.GetPhoto(id);
            if (photoFromRepo == null)
            {
                return NotFound();
            }

            if (photoFromRepo.IsMain)
                return BadRequest("This is already the main photo!");

            var currentMainPhoto = await _unitOfWork.Photos.GetMainPhotoForUser(userId);
            if (currentMainPhoto != null)
                currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await _unitOfWork.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main");

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var photoFromRepo = await _unitOfWork.Photos.GetPhoto(id);
            if (photoFromRepo == null)
                return NotFound();
            
            if (photoFromRepo.IsMain)
                return BadRequest("You cannot delete the main photo!");

            if (photoFromRepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                    _unitOfWork.Photos.Delete(photoFromRepo);
            }

            if (photoFromRepo.PublicId == null)
            {
                _unitOfWork.Photos.Delete(photoFromRepo);
            }

            if (await _unitOfWork.SaveAll())
                return Ok();

            return BadRequest("Failed to delete the photo");
        }


    }
}