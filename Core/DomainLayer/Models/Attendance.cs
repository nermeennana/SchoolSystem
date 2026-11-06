using System;

namespace DomainLayer.Models;

public class Attendance
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public DateTime Date { get; set; }
}
