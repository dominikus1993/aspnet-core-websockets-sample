namespace Sample.Api.Services;

public class EchoBackgroundWorker : BackgroundService
{
    private readonly ILogger<EchoBackgroundWorker> _logger;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}