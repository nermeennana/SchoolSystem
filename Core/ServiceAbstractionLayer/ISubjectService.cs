using System;
using Shared.DataTransferObjects.Subjects;

namespace ServiceAbstractionLayer;

public interface ISubjectService
{
    Task<IEnumerable<SubjectDto>> GetAllSubjectsAsync();
    Task<SubjectDto> GetSubjectByIdAsync(int id);
    Task<SubjectDto> CreateSubjectAsync(CreateAndUpdateSubjectDto subjectDto);
    Task UpdateSubjectAsync(int id, CreateAndUpdateSubjectDto subjectDto);
    Task DeleteSubjectAsync(int id);
}
