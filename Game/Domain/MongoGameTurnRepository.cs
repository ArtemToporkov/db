using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace Game.Domain;

public class MongoGameTurnRepository : IGameTurnRepository
{
    public const string CollectionName = "turns";
    private readonly IMongoCollection<GameTurnEntity> gameTurnCollection;
        
    public MongoGameTurnRepository(IMongoDatabase database)
    {
        gameTurnCollection = database.GetCollection<GameTurnEntity>(CollectionName);
    }

    public GameTurnEntity Insert(GameTurnEntity gameTurn)
    {
        gameTurnCollection.InsertOne(gameTurn);
        return gameTurn;
    }

    public IEnumerable<GameTurnEntity> GetLastTurns(Guid gameId, int limit)
    {
        var last = gameTurnCollection
            .Find(t => t.GameId == gameId)
            .SortByDescending(t => t.TurnIndex)
            .Limit(limit)
            .ToList();
        return last.OrderBy(t => t.TurnIndex).ToList();
    }
}