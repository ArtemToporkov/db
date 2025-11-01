using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Game.Domain;

[BsonIgnoreExtraElements]
public class GameTurnEntity
{
    [BsonElement] public Guid GameId { get; }
    [BsonElement] public int TurnIndex { get; }
    [BsonElement] public PlayerDecisionInfo[] PlayersDecisions { get; }
    [BsonElement] public Guid? WinnerId { get; }
        
    [BsonConstructor]
    public GameTurnEntity(Guid gameId, int turnIndex, Guid? winnerId = null, params PlayerDecisionInfo[] playersDecisions)
    {
        GameId = gameId;
        TurnIndex = turnIndex;
        PlayersDecisions = playersDecisions;
        WinnerId = winnerId;
    }
}