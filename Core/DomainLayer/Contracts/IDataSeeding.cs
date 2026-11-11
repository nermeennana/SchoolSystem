using System;

namespace DomainLayer.Contracts;

public interface IDataSeeding
{
    Task DataSeedAsync();
}
