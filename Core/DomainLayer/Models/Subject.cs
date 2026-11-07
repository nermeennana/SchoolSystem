using System;

namespace DomainLayer.Models;

public class Subject : BaseEntity<int>
{
    public string Name { get; set; } = null!;
}
