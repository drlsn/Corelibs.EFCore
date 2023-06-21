using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Corelibs.EFCore
{
    public class DbContextTransactionDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ICommandHandler<TCommand, TResponse> _decorated;
        private readonly DbContext _context;

        public DbContextTransactionDecorator(ICommandHandler<TCommand, TResponse> decorated, DbContext context)
        {
            _decorated = decorated;
            _context = context;
        }

        public async ValueTask<TResponse> Handle(TCommand command, CancellationToken ct)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var response = await _decorated.Handle(command, ct);

                    await transaction.CommitAsync();

                    return response;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine(ex.ToString());

                    return default;
                }
            }
        }
    }
}
