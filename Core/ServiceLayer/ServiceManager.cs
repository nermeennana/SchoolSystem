using System;
using AutoMapper;
using DomainLayer.Contracts;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using ServiceAbstractionLayer;

namespace ServiceLayer;

public class ServiceManager(IMapper _mapper, IUnitOfWork _unitOfWork, UserManager<AppUser> _userManager) : IServiceManager
{
    private readonly Lazy<ISubjectService> _lazySubjectService = new(() => new SubjectService(_unitOfWork, _mapper));
    public ISubjectService SubjectService => _lazySubjectService.Value;

    private readonly Lazy<IStudentService> _lazyStudentService = new(() => new StudentService(_unitOfWork, _mapper, _userManager));
    public IStudentService StudentService => _lazyStudentService.Value;
}
