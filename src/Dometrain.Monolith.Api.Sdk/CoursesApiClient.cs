using System.Net;
using System.Net.Http.Json;
using Dometrain.Monolith.Api.Contracts;
using Refit;

namespace Dometrain.Monolith.Api.Sdk;

public interface ICoursesApiClient
{
    [Get("/courses/{idOrSlug}")]
    Task<CourseResponse?> GetAsync(string idOrSlug);
}
