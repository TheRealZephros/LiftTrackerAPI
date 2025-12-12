using System.ComponentModel.DataAnnotations;

namespace api.Dtos.User
{
    public class UserInfoDto
    {
        [Required]
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
    }
}