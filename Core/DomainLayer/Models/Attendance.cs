using System;

namespace DomainLayer.Models;

public class Attendance : BaseEntity<int>
{
    public int EnrollmentId { get; set; }
    public DateTime Date { get; set; }

    public Enrollment Enrollment { get; set; } = null!;
}
