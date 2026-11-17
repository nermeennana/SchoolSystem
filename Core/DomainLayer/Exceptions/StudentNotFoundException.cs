using System;

namespace DomainLayer.Exceptions;

public sealed class StudentNotFoundException(int id): NotFoundException($"Student with ID {id} was not found.")
{
    
}
