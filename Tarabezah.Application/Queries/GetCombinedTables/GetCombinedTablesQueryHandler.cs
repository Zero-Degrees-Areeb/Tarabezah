using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetCombinedTables;

/// <summary>
/// Handler for the GetCombinedTablesQuery
/// </summary>
public class GetCombinedTablesQueryHandler : IRequestHandler<GetCombinedTablesQuery, IEnumerable<CombinedTableDto>>
{
    private readonly ICombinedTableRepository _combinedTableRepository;
    private readonly ILogger<GetCombinedTablesQueryHandler> _logger;

    public GetCombinedTablesQueryHandler(
        ICombinedTableRepository combinedTableRepository,
        ILogger<GetCombinedTablesQueryHandler> logger)
    {
        _combinedTableRepository = combinedTableRepository ?? throw new ArgumentNullException(nameof(combinedTableRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<CombinedTableDto>> Handle(GetCombinedTablesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting combined tables for floorplan: {FloorplanGuid}", request.FloorplanGuid);

        var combinedTables = await _combinedTableRepository.GetByFloorplanGuidAsync(request.FloorplanGuid, cancellationToken);

        return combinedTables.Select(ct => new CombinedTableDto
        {
            Guid = ct.Guid,
            GroupName = ct.GroupName,
            MinCapacity = ct.MinCapacity,
            MaxCapacity = ct.MaxCapacity,
            Members = ct.Members.Select(m => new CombinedTableMemberDto
            {
                Guid = m.Guid,
                FloorplanElementInstanceGuid = m.FloorplanElementInstance.Guid,
                TableId = m.FloorplanElementInstance.TableId,
                MinCapacity = m.FloorplanElementInstance.MinCapacity,
                MaxCapacity = m.FloorplanElementInstance.MaxCapacity
            }).ToList()
        });
    }
} 