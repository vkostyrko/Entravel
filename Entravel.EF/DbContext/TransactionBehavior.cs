using MediatR;

namespace Entravel.EF.DbContext;

public sealed class TransactionBehavior<TRequest, TResponse>(AppDbContext appDbContext) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (appDbContext.Database.CurrentTransaction is not null)
        {
            return await next(cancellationToken);
        }

        await using var tx = await appDbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();
            await appDbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

