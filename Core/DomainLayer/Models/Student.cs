using System;

namespace DomainLayer.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string ParentPhoneNumber { get; set; } = null!;
    public int GradeLevel { get; set; }
    public string UserId { get; set; } = null!;
}
