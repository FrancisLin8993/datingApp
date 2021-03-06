using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
  [ServiceFilter(typeof(LogUserActivity))]
  [Authorize]
  [Route("api/[controller]")]
  [ApiController]
  public class UsersController : ControllerBase
  {
    private readonly IDatingRepository _repository;
    private readonly IMapper _mapper;

    public UsersController(IDatingRepository repository, IMapper mapper)
    {
      _repository = repository;
      _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
    {
      var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
      var currentUserFromRepo = await _repository.GetUser(currentUserId);

      userParams.UserId = currentUserId;

      if (string.IsNullOrEmpty(userParams.Gender))
      {
        userParams.Gender = currentUserFromRepo.Gender == "male" ? "female" : "male";
      }

      var users = await _repository.GetUsers(userParams);
      var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

      Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
      return Ok(usersToReturn);
    }

    [HttpGet("{id}", Name = "GetUser")]
    public async Task<IActionResult> GetUser(int id)
    {
      var user = await _repository.GetUser(id);

      var userToReturn = _mapper.Map<UserForDetailsDto>(user);

      return Ok(userToReturn);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
    {
      if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      var userFromRepo = await _repository.GetUser(id);

      _mapper.Map(userForUpdateDto, userFromRepo);

      if (await _repository.SaveAll())
      {
        return NoContent();
      }

      throw new Exception("Update user failed on save.");
    }


    [HttpPost("{userId}/like/{recipientId}")]
    public async Task<IActionResult> LikeUser(int userId, int recipientId)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      var like = await _repository.GetLike(userId, recipientId);

      if (like != null)
      {
        return BadRequest("You have already liked this user.");
      }

      if (await _repository.GetUser(recipientId) == null)
      {
        return NotFound();
      }

      like = new Like
      {
        LikerId = userId,
        LikeeId = recipientId
      };

      _repository.Add<Like>(like);

      if (await _repository.SaveAll())
      {
        return Ok();
      }

      return BadRequest("Failed to like this user");
    }
  }
}