using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseAPIController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _autoMapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper autoMapper, IPhotoService photoService)
        {
            _userRepository = userRepository;
            _autoMapper = autoMapper;
            _photoService = photoService;
        }

        [HttpGet("{userName}", Name ="GetUser")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
        {
            var members = await _userRepository.GetMembersAsync();
            return Ok(members);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var username = User.GetUsername();
            var user = await _userRepository.GetUserByUsernameAsync(username);
            
            _autoMapper.Map(memberUpdateDTO, user);
            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var username = User.GetUsername();
            var user = await _userRepository.GetUserByUsernameAsync(username);

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            // Check if it's the only photo the user has uploaded
            if (user.Photos.Count == 0)
                photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync())
            {
                var photoDTO = _autoMapper.Map<PhotoDTO>(photo);
                return CreatedAtRoute("GetUser", new { userName = user.UserName }, photoDTO);
            }
            
            return BadRequest("Problem uploading photo");
        }
    
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            // Check if the photo is already set as the main photo
            if (photo.IsMain) 
                return BadRequest("This is already your main photo!");

            // Set the current main photo flag to false
            var currentMainPhoto = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMainPhoto != null)
                currentMainPhoto.IsMain = false;
            
            // Set new photo as main photo
            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }
    
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            // Check if the photo is stored in Cloudinary
            if (photo.PublicId != null) 
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete photo");
        }
    }
}