using System;

namespace DomainLayer.Models;

public class Teacher : BaseEntity<int>
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string PhoneNumber {get; set; } = null!;
    public int SubjectId {get; set; }
    public string UserId {get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public Subject Subject { get; set; } = null!;
}
