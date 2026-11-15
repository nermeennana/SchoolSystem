using System;
using AutoMapper;
using DomainLayer.Contracts;
using ServiceAbstractionLayer;

namespace ServiceLayer;

public class ServiceManager(IMapper _mapper, IUnitOfWork _unitOfWork) : IServiceManager
{
    private readonly Lazy<ISubjectService> _lazySubjectService = new(() => new SubjectService(_unitOfWork, _mapper));
    public ISubjectService SubjectService => _lazySubjectService.Value;
}
