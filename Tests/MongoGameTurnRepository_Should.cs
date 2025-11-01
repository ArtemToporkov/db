using System;
using System.Linq;
using FluentAssertions;
using Game.Domain;
using MongoDB.Driver;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class MongoGameTurnRepository_Should
{
    // ReSharper disable once NotAccessedField.Local
    private MongoGameTurnRepository repo;
    private IMongoDatabase db;

    [SetUp]
    public void SetUp()
    {
        db = TestMongoDatabase.Create();
        repo = new MongoGameTurnRepository(db);
    }

    [Test]
    public void Should_Have_GameId_TurnIndex_Index()
    {
        var collection = db.GetCollection<GameTurnEntity>(MongoGameTurnRepository.CollectionName);
        var indexes = collection.Indexes.List().ToList();
        const string gameIdName = nameof(GameTurnEntity.GameId);
        const string turnIndexName = nameof(GameTurnEntity.TurnIndex);

        var hasIndex = indexes.Any(index =>
        {
            var keyDoc = index["key"].AsBsonDocument;
            return keyDoc.ElementCount == 2 &&
                   keyDoc.Contains(gameIdName) && keyDoc[gameIdName].ToInt32() == 1 &&
                   keyDoc.Contains(turnIndexName) && keyDoc[turnIndexName].ToInt32() == -1;
        });

        hasIndex.Should().BeTrue($"Индекс ({gameIdName} ASC, {turnIndexName} DESC) должен существовать");
    }

    [Test]
    [Explicit($"Проверьте реализацию метода {nameof(MongoGameTurnRepository.GetLastTurns)} для корректности теста, " +
              $"она должна совпадать с той, что в тесте")]
    public void Should_Use_GameId_TurnIndex_Index()
    {
        var gameId = Guid.NewGuid();
        const int limit = 5;

        var command = new MongoDB.Bson.BsonDocument
        {
            { "explain", new MongoDB.Bson.BsonDocument
                {
                    { "find", MongoGameTurnRepository.CollectionName },
                    { "filter", new MongoDB.Bson.BsonDocument("GameId", gameId) },
                    { "sort", new MongoDB.Bson.BsonDocument("TurnIndex", -1) },
                    { "limit", limit }
                }
            }
        };

        var explanation = db.RunCommand<MongoDB.Bson.BsonDocument>(command);

        var winningPlan = explanation["queryPlanner"]["winningPlan"].AsBsonDocument;
        CheckIfUsesIndexRecursively(winningPlan).Should().BeTrue("Запрос должен использовать индекс по GameId + TurnIndex");
        return;

        bool CheckIfUsesIndexRecursively(MongoDB.Bson.BsonDocument plan)
        {
            if (plan.Contains("stage") && plan["stage"] == "IXSCAN") 
                return true;

            foreach (var child in new[] { "inputStage", "inputStages", "outerStage", "innerStage" })
            {
                if (!plan.Contains(child))
                    continue;

                var val = plan[child];
                switch (val)
                {
                    case MongoDB.Bson.BsonDocument doc when CheckIfUsesIndexRecursively(doc):
                        return true;
                    case MongoDB.Bson.BsonArray arr:
                    {
                        if (arr.OfType<MongoDB.Bson.BsonDocument>().Any(CheckIfUsesIndexRecursively))
                            return true;
                        break;
                    }
                }
            }

            return false;
        }
    }
}