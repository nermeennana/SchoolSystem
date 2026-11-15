using System;

namespace DomainLayer.Exceptions;

public abstract class NotFoundException(string message) : Exception(message)
{
    
}
