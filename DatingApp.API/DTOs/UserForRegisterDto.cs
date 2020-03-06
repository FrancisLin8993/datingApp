using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTOs
{
  public class UserForRegisterDto
  {
    [Required]
    public string Username { get; set; }

    [Required]
    [StringLength(8, MinimumLength = 4, ErrorMessage = "Passowrd length must be between 4 and 8")]
    public string Password { get; set; }
  }
}