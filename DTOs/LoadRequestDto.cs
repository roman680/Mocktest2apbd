using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs;

public class ReturnLoanRequestDto
{
    [Required]
    public string StatusName { get; set; } = null!;
}