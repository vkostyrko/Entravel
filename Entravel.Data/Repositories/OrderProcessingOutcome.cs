namespace Entravel.Data.Repositories;

public enum OrderProcessingOutcome
{
    Success,
    AlreadyProcessed,
    NotFound,
    ConcurrentProcessing,
    PermanentFailure
}
