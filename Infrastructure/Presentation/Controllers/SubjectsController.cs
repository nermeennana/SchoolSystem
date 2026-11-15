using System;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstractionLayer;
using Shared.DataTransferObjects.Subjects;

namespace Presentation.Controllers;
    
[ApiController]
[Route("api/[controller]")]
public class SubjectsController(IServiceManager _serviceManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubjectDto>>> GetAllSubjects()
    {
        var subjects = await _serviceManager.SubjectService.GetAllSubjectsAsync();
        return Ok(subjects);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubjectDto>> GetSubjectById(int id)
    {
        var subject = await _serviceManager.SubjectService.GetSubjectByIdAsync(id);
        if(subject == null)
        {
            return NotFound();
        }
        return Ok(subject);
    }

    [HttpPost]
    public async Task<ActionResult<SubjectDto>> CreateSubject(CreateAndUpdateSubjectDto subjectDto)
    {
        var createdSubject = await _serviceManager.SubjectService.CreateSubjectAsync(subjectDto);
        return CreatedAtAction(nameof(GetSubjectById), new { id = createdSubject.Id }, createdSubject);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSubject(int id, CreateAndUpdateSubjectDto subjectDto)
    {
        await _serviceManager.SubjectService.UpdateSubjectAsync(id, subjectDto);
        return NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        await _serviceManager.SubjectService.DeleteSubjectAsync(id);
        return NoContent();
    }

}
