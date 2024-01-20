using API.Data;
using API.Data.Migrations;
using API.DTO;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _dataContext;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _dataContext = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")] // POST: api/acount/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is Taken");


            using HMACSHA512 hmac = new HMACSHA512();
            AppUser user = new AppUser
            {
                UserName = registerDto.Username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();
            return new UserDto { Username = user.UserName, Token = _tokenService.CreateToken(user) };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            AppUser user = await _dataContext.Users.SingleOrDefaultAsync(x => x.UserName.Equals(loginDto.Username));

            if (user == null)
                return Unauthorized("Invalid Usersname");

            using HMACSHA512 hmac = new HMACSHA512(user.PasswordSalt);
            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) 
                    return Unauthorized("Invalid Password");
            }

            return new UserDto { Username = user.UserName, Token = _tokenService.CreateToken(user) };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _dataContext.Users.AnyAsync(x=>x.UserName.Equals(username.ToLower()));
        }
    }
}
