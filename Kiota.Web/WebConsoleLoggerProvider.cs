namespace Kiota.Web;

public class WebConsoleLoggerProvider : ILoggerProvider
{
    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName) => new WebConsoleLogger(categoryName);
}

file class WebConsoleLogger(string categoryName) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);

        Console.WriteLine($"[{categoryName}] {logLevel}: {message}");
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return new WebConsoleScope(state);
    }
}

file class WebConsoleScope(object state) : IDisposable
{
    public void Dispose()
    {
    }
}
