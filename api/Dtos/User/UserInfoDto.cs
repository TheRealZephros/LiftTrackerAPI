using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.User
{
    public class UserInfoDto
    {
        [Required]
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
    }
}