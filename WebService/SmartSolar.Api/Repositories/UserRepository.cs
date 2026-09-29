/*
 * File: UserRepository.cs
 * Description: MongoDB access for Users. No business rules live here.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolar.Api.Common;
using SmartSolar.Api.Data;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoContext _context;

    // Stores the shared Mongo context used by every user query.
    public UserRepository(MongoContext context)
    {
        _context = context;
    }

    // Returns how many user documents exist (used by the seeder guard).
    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return _context.Users.CountDocumentsAsync(FilterDefinition<User>.Empty, cancellationToken: cancellationToken);
    }

    // Loads a user by document id (staff username or prosumer NIC).
    public async Task<User?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.Find(user => user.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    // Resolves a login value against either username or NIC.
    public async Task<User?> FindByUsernameOrNicAsync(string login, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Eq(user => user.Username, login),
            Builders<User>.Filter.Eq(user => user.Nic, login));

        return await _context.Users.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    // Returns every user document for Backoffice listing.
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.Find(FilterDefinition<User>.Empty).ToListAsync(cancellationToken);
    }

    // Returns staff accounts only (Backoffice and GridOperator).
    public async Task<IReadOnlyList<User>> GetStaffAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.In(user => user.Role, new[] { RoleNames.Backoffice, RoleNames.GridOperator });
        return await _context.Users.Find(filter).ToListAsync(cancellationToken);
    }

    // Returns Prosumer documents matching optional status and NIC/name/phone/username text.
    public async Task<IReadOnlyList<User>> SearchProsumersAsync(string? status, string? query, CancellationToken cancellationToken = default)
    {
        var filters = new List<FilterDefinition<User>>
        {
            Builders<User>.Filter.Eq(user => user.Role, RoleNames.Prosumer)
        };

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add(Builders<User>.Filter.Eq(user => user.Status, status.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var escaped = Regex.Escape(query.Trim());
            var pattern = new BsonRegularExpression(escaped, "i");
            filters.Add(Builders<User>.Filter.Or(
                Builders<User>.Filter.Regex(user => user.Nic, pattern),
                Builders<User>.Filter.Regex(user => user.Username, pattern),
                Builders<User>.Filter.Regex(user => user.FullName, pattern),
                Builders<User>.Filter.Regex(user => user.Phone, pattern)));
        }

        return await _context.Users.Find(Builders<User>.Filter.And(filters))
            .SortBy(user => user.FullName)
            .ToListAsync(cancellationToken);
    }

    // Inserts a new user document.
    public Task InsertAsync(User user, CancellationToken cancellationToken = default)
    {
        return _context.Users.InsertOneAsync(user, cancellationToken: cancellationToken);
    }

    // Replaces an existing user document by id.
    public Task ReplaceAsync(User user, CancellationToken cancellationToken = default)
    {
        return _context.Users.ReplaceOneAsync(existing => existing.Id == user.Id, user, cancellationToken: cancellationToken);
    }
}
