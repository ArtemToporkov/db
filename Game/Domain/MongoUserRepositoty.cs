using System;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
            var indexKeysDefinition = Builders<UserEntity>.IndexKeys.Ascending(nameof(UserEntity.Login));
            var indexOptions = new CreateIndexOptions { Unique = true };
            userCollection.Indexes.CreateOne(new  CreateIndexModel<UserEntity>(indexKeysDefinition, indexOptions));
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            return userCollection.Find(x => x.Id == id).FirstOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            var user = userCollection.Find(x => x.Login == login).FirstOrDefault();
            if (user is not null)
                return user;
            user = new UserEntity(Guid.NewGuid()) { Login = login };
            userCollection.InsertOne(user);
            return user;
        }

        public void Update(UserEntity user) => userCollection.ReplaceOne(u => u.Id == user.Id, user);

        public void Delete(Guid id) => userCollection.DeleteOne(u => u.Id == id);

        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var toSkip = (pageNumber - 1) * pageSize;
            var totalCount = userCollection.CountDocuments(_ => true);
            var users = userCollection
                .Find(_ => true)
                .SortBy(u => u.Login)
                .Skip(toSkip)
                .Limit(pageSize)
                .ToList();
            return new PageList<UserEntity>(users, totalCount, pageNumber, pageSize);
        }

        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}