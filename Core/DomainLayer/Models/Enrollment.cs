using System;
using Shared.Enums;

namespace DomainLayer.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public EnrollmentStatus Status { get; set; }
    public int Year { get; set; }
    public int? Grade { get; set; }

}
