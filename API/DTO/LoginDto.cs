using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
