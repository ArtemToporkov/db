using System;
using System.Collections.Generic;

namespace Game.Domain
{
    public interface IGameTurnRepository
    {
        public GameTurnEntity Insert(GameTurnEntity gameTurn);

        public IEnumerable<GameTurnEntity> GetLastTurns(Guid gameId, int limit);
    }
}