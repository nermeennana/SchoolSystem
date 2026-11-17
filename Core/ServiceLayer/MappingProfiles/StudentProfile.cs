using System;
using AutoMapper;
using DomainLayer.Models;
using Shared.DataTransferObjects.Students;

namespace ServiceLayer.MappingProfiles;

public class StudentProfile : Profile
{
    public StudentProfile()
    {
        CreateMap<Student, StudentDto>();
        CreateMap<UpdateStudentDto, Student>();
        CreateMap<CreateStudentDto, Student>();
    }
}
