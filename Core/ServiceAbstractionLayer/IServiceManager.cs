using System;

namespace ServiceAbstractionLayer;

public interface IServiceManager
{
    ISubjectService SubjectService { get; }
    IStudentService StudentService { get; }
}
