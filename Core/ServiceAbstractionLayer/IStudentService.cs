using System;
using Shared.DataTransferObjects.Students;

namespace ServiceAbstractionLayer;

public interface IStudentService
{
    Task<IEnumerable<StudentDto>> GetAllStudentsAsync();
    Task<StudentDto> GetStudentByIdAsync(int id);
    Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto);
    Task UpdateStudentAsync(int id, UpdateStudentDto updateStudentDto);
    Task DeleteStudentAsync(int id);
}
