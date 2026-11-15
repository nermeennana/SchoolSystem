using System;
using AutoMapper;
using DomainLayer.Contracts;
using DomainLayer.Models;
using ServiceAbstractionLayer;
using Shared.DataTransferObjects.Subjects;

namespace ServiceLayer;

public class SubjectService(IUnitOfWork _unitOfWork, IMapper _mapper) : ISubjectService
{
    public async Task<IEnumerable<SubjectDto>> GetAllSubjectsAsync()
    {
        // Get the repository for Subject entity
        var repo = _unitOfWork.GetRepository<Subject, int>();
        // Get all subjects from the repository
        var subjects = await repo.GetAllAsync();
        // Map the subjects to SubjectDto using AutoMapper
        var subjectDtos = _mapper.Map<IEnumerable<SubjectDto>>(subjects);

        return subjectDtos;
    }

    public async Task<SubjectDto?> GetSubjectByIdAsync(int id)
    {
        var repo = _unitOfWork.GetRepository<Subject, int>();
        var subject = await repo.GetByIdAsync(id);
        if(subject == null)
        {
            return null;
        }
        var subjectDto = _mapper.Map<SubjectDto?>(subject);

        return subjectDto;
    }
    public async Task<SubjectDto> CreateSubjectAsync(CreateAndUpdateSubjectDto subjectDto)
    {
        var subject = _mapper.Map<Subject>(subjectDto);
        var repo = _unitOfWork.GetRepository<Subject, int>();
        repo.Add(subject);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<SubjectDto>(subject);
    }
    public async Task UpdateSubjectAsync(int id, CreateAndUpdateSubjectDto subjectDto)
    {
        var repo = _unitOfWork.GetRepository<Subject, int>();
        var subject = await repo.GetByIdAsync(id);
        if(subject == null)
        {
            throw new Exception($"Subject with id {id} not found.");
        }
        _mapper.Map(subjectDto, subject);
        repo.Update(subject);
        await _unitOfWork.SaveChangesAsync();
    }
    public async Task DeleteSubjectAsync(int id)
    {
        var repo = _unitOfWork.GetRepository<Subject, int>();
        var subject = await repo.GetByIdAsync(id);
        if(subject == null)
        {
            throw new Exception($"Subject with id {id} not found.");
        }
        repo.Delete(subject);
        await _unitOfWork.SaveChangesAsync();
    }

}
