using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Data.Context;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.DeleteCombinedTable;

/// <summary>
/// Handler for the DeleteCombinedTableCommand
/// </summary>
public class DeleteCombinedTableCommandHandler : IRequestHandler<DeleteCombinedTableCommand, bool>
{
    private readonly ICombinedTableRepository _combinedTableRepository;
    private readonly TarabezahDbContext _dbContext;
    private readonly ILogger<DeleteCombinedTableCommandHandler> _logger;

    public DeleteCombinedTableCommandHandler(
        ICombinedTableRepository combinedTableRepository,
        TarabezahDbContext dbContext,
        ILogger<DeleteCombinedTableCommandHandler> logger)
    {
        _combinedTableRepository = combinedTableRepository ?? throw new ArgumentNullException(nameof(combinedTableRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(DeleteCombinedTableCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting combined table: {CombinedTableGuid}", request.CombinedTableGuid);

        var combinedTable = await _combinedTableRepository.GetByGuidAsync(request.CombinedTableGuid, cancellationToken);
        if (combinedTable == null)
        {
            _logger.LogWarning("Combined table with GUID {CombinedTableGuid} not found", request.CombinedTableGuid);
            return false;
        }

        await _combinedTableRepository.DeleteAsync(combinedTable, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Combined table with GUID {CombinedTableGuid} deleted successfully", request.CombinedTableGuid);
        return true;
    }
} 