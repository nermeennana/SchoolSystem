using System;

namespace DomainLayer.Exceptions;

public class BadRequestException(string message) : Exception(message)
{

}
