using API.Data;
using API.DTOs;
using API.Entity;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseController
    {
        private readonly DataContext _context;
        private readonly ITokenService tokenService;

        public AccountController(DataContext  context,ITokenService tokenService)
        {
            this._context = context;
            this.tokenService = tokenService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto register)
        {
            if(await UserExist(register.Username)) return BadRequest("Username is taken");
           
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = register.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password)),
                PasswordSalt = hmac.Key

            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExist(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto login)
        {
            var user= await _context.Users.FirstOrDefaultAsync(x=>x.UserName==login.Username);
            if(user==null)
            {
                return  Unauthorized("Unvalid Username");
            }
            else
            {
                using var hmac = new HMACSHA512(user.PasswordSalt);
                
                var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

                for(int i=0; i < computedHash.Length; i++)
                {
                    if(computedHash[i]!=user.PasswordHash[i])
                    {
                        return Unauthorized("Unalide Password");
                    }  
                }
                return new UserDto
                {
                    UserName = user.UserName,
                    Token = tokenService.CreateToken(user)
                }; 

            }
        }

    }
}
