using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly IAuthRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthController(IAuthRepository repository, IConfiguration configuration, IMapper mapper)
    {
      _mapper = mapper;
      _repository = repository;
      _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
    {
      //validate the request
      userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

      if (await _repository.UserExists(userForRegisterDto.Username))
        return BadRequest("Username already exists");

      var userToCreate = _mapper.Map<User>(userForRegisterDto);
      var createdUser = await _repository.Register(userToCreate, userForRegisterDto.Password);
      var userToReturn = _mapper.Map<UserForDetailsDto>(createdUser);


      return CreatedAtRoute("GetUser", new { Controller = "Users", id = createdUser.Id }, userToReturn);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
    {
      var userFromRepository = await _repository.Login(userForLoginDto.Username, userForLoginDto.Password);
      if (userFromRepository == null)
        return Unauthorized();

      //if user exists, generate the token

      var claims = new[]
      {
        new Claim(ClaimTypes.NameIdentifier, userFromRepository.Id.ToString()),
        new Claim(ClaimTypes.Name, userFromRepository.Username)
      };

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(100),
        SigningCredentials = credentials
      };

      var tokenHandler = new JwtSecurityTokenHandler();

      var token = tokenHandler.CreateToken(tokenDescriptor);

      var user = _mapper.Map<UserForListDto>(userFromRepository);

      return Ok(new
      {
        token = tokenHandler.WriteToken(token),
        user
      });
    }
  }
}