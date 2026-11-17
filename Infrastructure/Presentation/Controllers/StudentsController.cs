using System;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstractionLayer;
using Shared.DataTransferObjects.Students;

namespace Presentation.Controllers;
[ApiController]
[Route("api/[controller]")]
public class StudentsController(IServiceManager _serviceManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await _serviceManager.StudentService.GetAllStudentsAsync();
        return Ok(students);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStudentById(int id)
    {
        var student = await _serviceManager.StudentService.GetStudentByIdAsync(id);
        return Ok(student);
    }
    [HttpPost]
    public async Task<IActionResult> CreateStudent(CreateStudentDto createStudentDto)
    {
        var student = await _serviceManager.StudentService.CreateStudentAsync(createStudentDto);
        return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, student);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStudent(int id, UpdateStudentDto updateStudentDto)
    {
        await _serviceManager.StudentService.UpdateStudentAsync(id, updateStudentDto);
        return NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        await _serviceManager.StudentService.DeleteStudentAsync(id);
        return NoContent();
    }
}
