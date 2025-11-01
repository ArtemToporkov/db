using System;

namespace Game.Domain;

public record PlayerDecisionInfo(Guid PlayerId, PlayerDecision Decision, int ScoreAfterDecision);