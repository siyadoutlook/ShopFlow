using Grpc.Core;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace ShopFlow.OrderService.Resilience;

public static class ResiliencePipelines
{
    public static IHttpClientBuilder AddInventoryServiceResilience(
        this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler("inventory-grpc", pipeline =>
        {
            pipeline.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(10),
                OnTimeout = args =>
                {
                    Console.WriteLine(
                        $"[Timeout] gRPC call exceeded {args.Timeout.TotalSeconds}s");
                    return ValueTask.CompletedTask;
                }
            });

            pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<RpcException>(ex =>              // ← this was missing
                        ex.StatusCode is StatusCode.Unavailable or
                            StatusCode.DeadlineExceeded),
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"[Retry] Attempt {args.AttemptNumber + 1} " +
                        $"after {args.RetryDelay.TotalSeconds:F1}s delay");
                    return ValueTask.CompletedTask;
                }
            });

            pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<RpcException>(ex =>
                        ex.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded),
                OnOpened = args =>
                {
                    Console.WriteLine(
                        $"[Circuit Breaker] OPENED — " +
                        $"Inventory Service unavailable. " +
                        $"Pausing calls for {args.BreakDuration.TotalSeconds}s");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    Console.WriteLine(
                        "[Circuit Breaker] CLOSED — " +
                        "Inventory Service recovered");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    Console.WriteLine(
                        "[Circuit Breaker] HALF-OPEN — " +
                        "Testing Inventory Service with one request");
                    return ValueTask.CompletedTask;
                }
            });
        });

        return builder; 
    }
}