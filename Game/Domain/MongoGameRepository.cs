using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace Game.Domain
{
    // TODO Сделать по аналогии с MongoUserRepository
    public class MongoGameRepository : IGameRepository
    {
        public const string CollectionName = "games";
        private readonly IMongoCollection<GameEntity> _gameCollection;

        public MongoGameRepository(IMongoDatabase database)
        {
            _gameCollection = database.GetCollection<GameEntity>(CollectionName);
        }

        public GameEntity Insert(GameEntity game)
        {
            _gameCollection.InsertOne(game);
            return game;
        }

        public GameEntity FindById(Guid gameId) => _gameCollection.Find(g => g.Id == gameId).FirstOrDefault();

        public void Update(GameEntity game) => _gameCollection.ReplaceOne(g => g.Id == game.Id, game);

        public IList<GameEntity> FindWaitingToStart(int limit) => _gameCollection
            .Find(g => g.Status == GameStatus.WaitingToStart).Limit(limit).ToList();

        public bool TryUpdateWaitingToStart(GameEntity game)
        {
            var result = _gameCollection.ReplaceOne(g => g.Id == game.Id && g.Status == GameStatus.WaitingToStart, game);
            return result.IsAcknowledged && result.ModifiedCount == 1;
        }
    }
}