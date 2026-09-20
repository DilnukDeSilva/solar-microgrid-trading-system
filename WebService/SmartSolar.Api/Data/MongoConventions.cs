/*
 * File: MongoConventions.cs
 * Description: Registers camelCase + ignore-extra-elements conventions for the C# driver.
 * Author: Member 1
 * Created: 20/09/2026
 *
 * Adapted from the MongoDB C# Driver documentation:
 * https://www.mongodb.com/docs/drivers/csharp/current/
 */

using MongoDB.Bson.Serialization.Conventions;

namespace SmartSolar.Api.Data;

public static class MongoConventions
{
    private static int _registered;

    // Registers driver conventions once so documents match the API camelCase contract.
    public static void Register()
    {
        if (Interlocked.Exchange(ref _registered, 1) == 1)
        {
            return;
        }

        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };

        ConventionRegistry.Register("smart-solar", pack, _ => true);
    }
}
