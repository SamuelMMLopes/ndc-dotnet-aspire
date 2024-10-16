using Dometrain.Monolith.Api.Contracts;

namespace Dometrain.Monolith.Api.Courses;

public static class CourseMapper
{
    public static Course MapToCourse(this CreateCourseRequest request)
    {
        return new Course
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Author = request.Author
        };
    }
    
    public static Course MapToCourse(this UpdateCourseRequest request, Guid id)
    {
        return new Course
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Author = request.Author
        };
    }
    
    public static CourseResponse? MapToResponse(this Course? course)
    {
        if (course is null)
        {
            return null;
        }
        
        return new CourseResponse
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Slug = course.Slug,
            Author = course.Author
        };
    }
}
