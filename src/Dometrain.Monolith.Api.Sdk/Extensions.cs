using System.Net;
using Dometrain.Monolith.Api.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Refit;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IServiceCollection AddDometrainApi(this IServiceCollection services,
        string baseUrl, string apiKey)
    {
       
        services.AddRefitClient<ICoursesApiClient>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
                c.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }).AddResilienceHandler("my-retry", builder =>
            {
                builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    Delay = TimeSpan.FromSeconds(2),
                    UseJitter = true,
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 3,
                    ShouldHandle = static args => ValueTask.FromResult(args is
                    {
                        Outcome.Result.StatusCode:
                        HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
                    })
                });
            });
        
        services.AddRefitClient<IStudentsApiClient>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
                c.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }).AddStandardResilienceHandler();

        return services;
    }
}
