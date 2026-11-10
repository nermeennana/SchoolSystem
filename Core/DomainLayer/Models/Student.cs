using System;

namespace DomainLayer.Models;

public class Student : BaseEntity<int>
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string ParentPhoneNumber { get; set; } = null!;
    public int GradeLevel { get; set; }
    public string UserId { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public AppUser User { get; set; } = null!;
}
