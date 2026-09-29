/*
 * File: ReservationQueryService.cs
 * Description: Booking lists and dashboards. Counts are computed from Mongo, never hard-coded.
 * Author: samudith
 * Created: 29/09/2026
 */

using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class ReservationQueryService : IReservationQueryService
{
    public const int PageSize = 20;

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.Ordinal)
    {
        ReservationStatuses.Pending,
        ReservationStatuses.Approved,
        ReservationStatuses.Cancelled,
        ReservationStatuses.Completed
    };

    private readonly IReservationQueryRepository _reservations;
    private readonly IStationRepository _stations;

    // Injects the read repository and the station repository used for the active-station count.
    public ReservationQueryService(IReservationQueryRepository reservations, IStationRepository stations)
    {
        _reservations = reservations;
        _stations = stations;
    }

    // Lists reservations. A prosumer only ever sees their own NIC, taken from the token.
    public async Task<ReservationPageDto> SearchAsync(
        ReservationActor actor,
        string? status,
        DateTime? from,
        DateTime? to,
        string? stationId,
        string? query,
        string? nic,
        int page,
        CancellationToken cancellationToken = default)
    {
        status = NormalizeStatus(status);
        page = page < 1 ? 1 : page;
        var scopedNic = ScopeNic(actor, nic);

        if (actor.IsProsumer && string.IsNullOrWhiteSpace(scopedNic))
        {
            return EmptyPage(page);
        }

        var (items, total) = await _reservations.SearchAsync(
            scopedNic,
            status,
            stationId,
            AsUtc(from),
            AsUtc(to),
            query,
            (page - 1) * PageSize,
            PageSize,
            cancellationToken);

        return ToPage(items.Select(ReservationDto.From).ToList(), page, total);
    }

    // Returns the operator approval queue: future Pending bookings.
    public async Task<ReservationPageDto> GetPendingQueueAsync(int page, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        var now = SriLankaClock.UtcNow();
        var (items, total) = await _reservations.SearchAsync(
            null,
            ReservationStatuses.Pending,
            null,
            now,
            null,
            null,
            (page - 1) * PageSize,
            PageSize,
            cancellationToken);

        return ToPage(items.Select(ReservationDto.From).ToList(), page, total);
    }

    // Returns the signed-in prosumer's live dashboard counts and next booking.
    public async Task<ProsumerDashboardDto> GetProsumerDashboardAsync(ReservationActor actor, CancellationToken cancellationToken = default)
    {
        if (!actor.IsProsumer || string.IsNullOrWhiteSpace(actor.Nic))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, ErrorCodes.Forbidden, "Only a prosumer can open this dashboard.");
        }

        var now = SriLankaClock.UtcNow();
        var nic = actor.Nic;
        var pending = await _reservations.CountForProsumerAsync(nic, ReservationStatuses.Pending, null, false, cancellationToken);
        var approvedFuture = await _reservations.CountForProsumerAsync(nic, ReservationStatuses.Approved, now, false, cancellationToken);
        var active = await _reservations.CountForProsumerAsync(nic, null, now, true, cancellationToken);
        var next = await _reservations.FindNextUpcomingAsync(nic, now, cancellationToken);

        return new ProsumerDashboardDto
        {
            PendingCount = ToCount(pending),
            ApprovedFutureCount = ToCount(approvedFuture),
            ActiveCount = ToCount(active),
            NextReservation = next is null ? null : ReservationDto.From(next)
        };
    }

    // Returns today's operational counts for the staff Home page.
    public async Task<OperationsDashboardDto> GetOperationsDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = SriLankaClock.UtcNow();
        var (start, end) = SriLankaClock.DayRangeUtc(now);
        var stations = await _stations.GetAllAsync(cancellationToken);

        var todaysCount = await _reservations.CountScheduledBetweenAsync(start, end, cancellationToken);
        var pending = await _reservations.CountPendingFutureAsync(now, cancellationToken);
        var completed = await _reservations.CountCompletedBetweenAsync(start, end, cancellationToken);
        var todays = await _reservations.ListScheduledBetweenAsync(start, end, PageSize, cancellationToken);

        return new OperationsDashboardDto
        {
            TodaysBookingCount = ToCount(todaysCount),
            PendingApprovals = ToCount(pending),
            ActiveStations = stations.Count(station => station.Status == StationStatuses.Active),
            CompletedToday = ToCount(completed),
            TodaysBookings = todays.Select(ReservationDto.From).ToList()
        };
    }

    // Prosumers are locked to the NIC in the token. Staff may filter by the query NIC.
    private static string? ScopeNic(ReservationActor actor, string? requestedNic)
    {
        if (actor.IsProsumer)
        {
            return actor.Nic;
        }

        return string.IsNullOrWhiteSpace(requestedNic) ? null : requestedNic.Trim();
    }

    // Rejects a status that is not one of the four contract values.
    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var trimmed = status.Trim();
        if (!AllowedStatuses.Contains(trimmed))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "Status must be Pending, Approved, Cancelled or Completed.");
        }

        return trimmed;
    }

    // Treats an unspecified timestamp as UTC so date filters do not shift with the server locale.
    private static DateTime? AsUtc(DateTime? value)
    {
        if (value is null)
        {
            return null;
        }

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }

    // Builds an empty page when a prosumer token has no NIC.
    private static ReservationPageDto EmptyPage(int page)
    {
        return new ReservationPageDto { Items = Array.Empty<ReservationDto>(), Page = page, PageSize = PageSize, TotalCount = 0 };
    }

    // Wraps a result list in the page envelope.
    private static ReservationPageDto ToPage(IReadOnlyList<ReservationDto> items, int page, long total)
    {
        return new ReservationPageDto { Items = items, Page = page, PageSize = PageSize, TotalCount = total };
    }

    // Narrows a Mongo count into the int field on the dashboard DTOs.
    private static int ToCount(long value)
    {
        return value > int.MaxValue ? int.MaxValue : (int)value;
    }
}
