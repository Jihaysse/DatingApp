using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(maximumLength: 12, MinimumLength = 6)]
        public string Password { get; set; }
    }
}