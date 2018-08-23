using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using WebApi.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services;
using WebApi.Dtos;
using WebApi.Entities;
using Microsoft.AspNetCore.Identity;
using WebApi.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public AccountController(
            UserManager<ApplicationUser> userManager,
             IMapper mapper, 
             IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Authenticate([FromBody]UserDto userDto)
        {
            var identity = await GetClaimsIdentity(userDto.UserName, userDto.Password);
            if (identity == null)
            { 
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            // return basic user info (without password) and token to store client side
            return Ok(identity);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserDto userDto)
        {

            try
            {
                var user = new ApplicationUser()
                {
                    UserName = userDto.UserName,
                    FullName = userDto.FullName,
                    PhoneNumber = userDto.MobilePhone,
                    Status = userDto.UserStatus,
                    Registered = DateTime.Now
                };

                // save 
                var createPowerUser = await _userManager.CreateAsync(user, userDto.Password);
                if (createPowerUser.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, userDto.UserRole);
                    return Ok();
                }
                return BadRequest(new { message = createPowerUser.Errors });
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userManager.Users;

            var userDtos = users.ToList().Select(curr =>
            {
                var roles = _userManager.GetRolesAsync(curr).Result;
                return new UserDto
                {
                    Id=curr.Id,
                    UserName = curr.UserName,
                    FullName = curr.FullName,
                    MobilePhone = curr.PhoneNumber,
                    UserStatus = curr.Status,
                    UserRole = roles.FirstOrDefault(),
                    Registered = curr.Registered
                };
            });
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var curr = await _userManager.FindByIdAsync(id.ToString());
            var roles = await _userManager.GetRolesAsync(curr);
            var userDto = new UserDto
            {
                Id = curr.Id,
                UserName = curr.UserName,
                FullName = curr.FullName,
                MobilePhone = curr.PhoneNumber,
                UserStatus = curr.Status,
                UserRole = roles.FirstOrDefault(),
                Registered = curr.Registered
            };
            return Ok(userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody]UserDto userDto)
        {

            var userToVerify = await _userManager.FindByIdAsync(id.ToString());
            if (userToVerify == null)
            {
               return  BadRequest(new { message = "the user is not existed." });
            }

            var currRole = await _userManager.GetRolesAsync(userToVerify);
            userToVerify.UserName = userDto.UserName;
            userToVerify.FullName = userDto.FullName;
            userToVerify.PhoneNumber = userDto.MobilePhone;
            userToVerify.Status = userDto.UserStatus;

            try
            {
                // save 
                var updated = await _userManager.UpdateAsync(userToVerify);
                if(updated.Succeeded && !string.IsNullOrEmpty(userDto.UserRole))
                {
                    await _userManager.RemoveFromRolesAsync(userToVerify, currRole);
                    await _userManager.AddToRoleAsync(userToVerify, userDto.UserRole);
                }
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            // get the user to verifty
            var userToVerify = await _userManager.FindByIdAsync(id.ToString());
            await _userManager.DeleteAsync(userToVerify);
            return Ok();
        }

        private async Task<object> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return null;

            // get the user to verifty
            var userToVerify = await _userManager.FindByNameAsync(userName);

            if (userToVerify == null) return null;

            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, userToVerify.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                var roles = await _userManager.GetRolesAsync(userToVerify);
                return await Task.FromResult(new
                {
                    Id = userToVerify.Id,
                    Username = userToVerify.UserName,
                    FullName = userToVerify.FullName,
                    Role = roles.FirstOrDefault(),
                    Token = tokenString
                });
            }

            // Credentials are invalid, or account doesn't exist
            return null;
        }


    }
}
