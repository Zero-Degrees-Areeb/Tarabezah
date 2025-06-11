using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetFloorplanById;

// Keeping the name for backward compatibility, but now it uses GUID
public record GetFloorplanByIdQuery(Guid FloorplanGuid) : IRequest<FloorplanDto?>; 