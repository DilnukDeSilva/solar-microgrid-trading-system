/*
 * File: MongoContext.cs
 * Description: Shared MongoDB database and the four assignment collections.
 * Author: Member 1
 * Created: 20/09/2026
 *
 * Adapted from the MongoDB C# Driver quick start:
 * https://www.mongodb.com/docs/drivers/csharp/current/quick-start/
 */

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SmartSolar.Api.Configuration;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Data;

public class MongoContext
{
    public IMongoDatabase Database { get; }

    public IMongoCollection<User> Users { get; }

    public IMongoCollection<Station> Stations { get; }

    public IMongoCollection<Slot> Slots { get; }

    public IMongoCollection<Reservation> Reservations { get; }

    // Builds a MongoClient from configuration and exposes typed collections.
    public MongoContext(IOptions<MongoDbSettings> options)
    {
        MongoConventions.Register();

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException("MongoDb:ConnectionString is missing. Set it in configuration or the MongoDb__ConnectionString environment variable.");
        }

        if (string.IsNullOrWhiteSpace(settings.DatabaseName))
        {
            throw new InvalidOperationException("MongoDb:DatabaseName is missing.");
        }

        var client = new MongoClient(settings.ConnectionString);
        Database = client.GetDatabase(settings.DatabaseName);
        Users = Database.GetCollection<User>("Users");
        Stations = Database.GetCollection<Station>("SolarStationInfo");
        Slots = Database.GetCollection<Slot>("EnergyBookingSlots");
        Reservations = Database.GetCollection<Reservation>("EnergyReservation");
    }
}
