using System;
using AutoMapper;
using DomainLayer.Models;
using Shared.DataTransferObjects.Subjects;

namespace ServiceLayer.MappingProfiles;

public class SubjectProfile : Profile
{
    public SubjectProfile()
    {
        CreateMap<Subject, SubjectDto>();
        CreateMap<CreateAndUpdateSubjectDto, Subject>();
    }
}
