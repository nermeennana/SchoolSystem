using System;

namespace DomainLayer.Models;

public class Teacher
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string PhoneNumber {get; set; } = null!;
    public int SubjectId {get; set; }
    public string UserId {get; set; } = null!;
}
