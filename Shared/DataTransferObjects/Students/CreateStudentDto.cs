using System;

namespace Shared.DataTransferObjects.Students;

public class CreateStudentDto
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string ParentPhoneNumber { get; set; } = null!;
    public int GradeLevel { get; set; }
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
