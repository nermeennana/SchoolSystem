using System;

namespace DomainLayer.Exceptions;

public sealed class SubjectNotFoundException(int id) : NotFoundException($"Subject with ID {id} not found.")
{

}
