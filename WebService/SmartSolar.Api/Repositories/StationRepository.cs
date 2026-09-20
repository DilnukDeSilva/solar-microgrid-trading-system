/*
 * File: StationRepository.cs
 * Description: MongoDB access for SolarStationInfo.
 * Author: Member 1
 * Created: 20/09/2026
 */

using MongoDB.Driver;
using SmartSolar.Api.Data;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public class StationRepository : IStationRepository
{
    private readonly MongoContext _context;

    // Stores the shared Mongo context used by station queries.
    public StationRepository(MongoContext context)
    {
        _context = context;
    }

    // Returns every station document.
    public async Task<IReadOnlyList<Station>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stations.Find(FilterDefinition<Station>.Empty).ToListAsync(cancellationToken);
    }

    // Loads one station by id.
    public async Task<Station?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Stations.Find(station => station.Id == id).FirstOrDefaultAsync(cancellationToken);
    }
}
