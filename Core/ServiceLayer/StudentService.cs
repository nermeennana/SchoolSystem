using System;
using AutoMapper;
using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using ServiceAbstractionLayer;
using Shared.Constants;
using Shared.DataTransferObjects.Students;

namespace ServiceLayer;

public class StudentService(IUnitOfWork _unitOfWork, IMapper _mapper, UserManager<AppUser> _userManager) : IStudentService
{
    public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
    {
        var repo = _unitOfWork.GetRepository<Student, int>();
        var students = await repo.GetAllAsync();
        var studentDtos = _mapper.Map<IEnumerable<StudentDto>>(students);

        return studentDtos;
    }

    public async Task<StudentDto> GetStudentByIdAsync(int id)
    {
        var repo = _unitOfWork.GetRepository<Student, int>();
        var student = await repo.GetByIdAsync(id);
        if(student == null)
        {
            throw new StudentNotFoundException(id);
        }
        var studentDto = _mapper.Map<StudentDto>(student);

        return studentDto;
    }
    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = new AppUser
                {
                    UserName = createStudentDto.Email,
                    Email = createStudentDto.Email,
                    EmailConfirmed = true
                };
    
            var result = await _userManager.CreateAsync(user, createStudentDto.Password);
            if (!result.Succeeded)
            {
                throw new BadRequestException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            
            await _userManager.AddToRoleAsync(user, Roles.Student);
            var student = _mapper.Map<Student>(createStudentDto);
            student.UserId = user.Id;
    
            var repo = _unitOfWork.GetRepository<Student, int>();
            repo.Add(student);

            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();
            return _mapper.Map<StudentDto>(student);
        }
        catch (System.Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task UpdateStudentAsync(int id, UpdateStudentDto updateStudentDto)
    {
        var repo = _unitOfWork.GetRepository<Student, int>();
        var student = await repo.GetByIdAsync(id);
        if(student == null)
        {
            throw new StudentNotFoundException(id);
        }
        _mapper.Map(updateStudentDto, student);
        repo.Update(student);
        await _unitOfWork.SaveChangesAsync();

    }
    public async Task DeleteStudentAsync(int id)
    {
        var repo = _unitOfWork.GetRepository<Student, int>();
        var student = await repo.GetByIdAsync(id);
        if(student == null)
        {
            throw new StudentNotFoundException(id);
        }
        repo.Delete(student);
        await _unitOfWork.SaveChangesAsync();
    }

}