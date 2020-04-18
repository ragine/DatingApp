using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CloudinaryDotNet;
using System.Threading.Tasks;
using DatingApp.API.Dtos;
using System.Security.Claims;
using CloudinaryDotNet.Actions;
using DatingApp.API.Models;
using System.Linq;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinarySettings;
        private readonly Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
                                IOptions<CloudinarySettings> cloudinarySettings)
        {
            _cloudinarySettings = cloudinarySettings;
            _mapper = mapper;
            _repo = repo;

            Account cloudAcc = new Account (
                _cloudinarySettings.Value.CloudName,
                _cloudinarySettings.Value.ApiKey,
                _cloudinarySettings.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(cloudAcc);
        }

        [HttpGet("{id}", Name="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photofromRepo =  await _repo.GetPhoto(id);

            var photoDto = _mapper.Map<PhotoToReturnDto>(photofromRepo);

            return Ok(photoDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoFromUser(int userId, [FromForm]PhotoForCreationDto photoDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);

            var file = photoDto.File;

            var uploadResult = new ImageUploadResult();

            if(file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
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

           var photo =  _mapper.Map<Photo>(photoDto);

           if (!user.Photos.Any(u => u.IsMain))
           {
               photo.IsMain = true;
           }

           user.Photos.Add(photo);

           if (await _repo.SaveAll())
           {
               var photoToReturn = _mapper.Map<PhotoToReturnDto>(photo);
               return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id}, photoToReturn );
           }

            return BadRequest("Could not add photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int Id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await this._repo.GetUser(userId);

            if(!user.Photos.Any(p => p.Id == Id))
            {
                return Unauthorized();
            }

            var photo = await this._repo.GetPhoto(Id);

            if(photo.IsMain == true)
            {
                return BadRequest("Photo is main already!");
            }

            var mainPhoto = await this._repo.GetMainPhotoForUser(userId);
            mainPhoto.IsMain = false;

            photo.IsMain = true;

            if (await this._repo.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Couldnt save main photo!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult>  DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await this._repo.GetUser(userId);

            if(!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            var photo = await this._repo.GetPhoto(id);

            if(photo.IsMain == true)
            {
                return BadRequest("You cant delete your main photo!");
            }

            if (photo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photo.PublicId);

                var response = _cloudinary.Destroy(deleteParams);

                if (response.Result == "ok")
                {
                    _repo.Delete(photo);
                }
            }
            else 
            {
                 _repo.Delete(photo);
            }

            
            if (await _repo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to delete photo!");
        }
    }



}