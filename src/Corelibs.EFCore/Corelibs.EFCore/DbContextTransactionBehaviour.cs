using Corelibs.Basic.Blocks;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Corelibs.EFCore
{
    public class DbContextTransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
    {
        private readonly DbContext _context;

        public DbContextTransactionBehaviour(DbContext context)
        {
            _context = context;
        }

        public async ValueTask<TResponse> Handle(TRequest command, CancellationToken ct, MessageHandlerDelegate<TRequest, TResponse> next)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var entries = _context.ChangeTracker.Entries().ToArray();
                    var response = await next(command, ct);
                    if (response is Result result && !result.IsSuccess)
                        return response;

                    bool isCommand = typeof(TRequest).GetInterface(typeof(IBaseCommand).Name) != null;
                    if (isCommand)
                        await transaction.CommitAsync();

                    return response;
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    Console.WriteLine(ex.ToString());
                    throw ex;
                    return default;
                }
            }
        }
    }
}
