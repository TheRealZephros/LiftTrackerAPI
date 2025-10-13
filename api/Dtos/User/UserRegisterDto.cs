using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.User
{
    public class UserRegisterDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}