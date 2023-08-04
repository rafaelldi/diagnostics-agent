namespace DiagnosticsAgent.Common.Session;

internal interface IValueProducer
{
    Task ProduceAsync();
}