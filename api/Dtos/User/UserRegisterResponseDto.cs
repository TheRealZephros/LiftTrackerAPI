using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.User
{
    public class UserRegisterResponseDto

    {
        [Required]
        public required UserInfoDto User { get; set; }
        [Required]
        public required string AccessToken { get; set; }
        [Required]
        public required string RefreshToken { get; set; }
    }
}