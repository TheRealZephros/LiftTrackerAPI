using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.User
{
    public class UserRefreshTokenResponseDto
   {
        public string AccessToken { get; set; }
        
    }
}