using System;
using Shared.Enums;

namespace DomainLayer.Models;

public class Enrollment : BaseEntity<int>
{
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public EnrollmentStatus Status { get; set; }
    public int Year { get; set; }
    public int? Grade { get; set; }

    public Student Student { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
