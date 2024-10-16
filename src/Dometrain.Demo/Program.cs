
using System.Text.Json;
using Dometrain.Monolith.Api.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

var services = new ServiceCollection();

services.AddDometrainApi("http://localhost:5148", "ThisIsAlsoMeantToBeSecret");

var serviceProvider = services.BuildServiceProvider();

var client = serviceProvider.GetRequiredService<ICoursesApiClient>();

var course = await client.GetAsync("0eae3088-5cbe-4045-a141-2ba3be9fb591");

Console.WriteLine(JsonSerializer.Serialize(course));
