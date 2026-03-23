using System.Collections.Generic;
using UnityEngine;

namespace Chegg.Framework
{
    public interface ICheggPatternResolver
    {
        List<Vector2Int> GetValidMoveTargets(CheggGameState state, MinionInstance minion);
        List<Vector2Int> GetValidAttackTargets(CheggGameState state, MinionInstance minion);
    }

    public sealed class EmptyPatternResolver : ICheggPatternResolver
    {
        public List<Vector2Int> GetValidMoveTargets(CheggGameState state, MinionInstance minion)
        {
            return new List<Vector2Int>();
        }

        public List<Vector2Int> GetValidAttackTargets(CheggGameState state, MinionInstance minion)
        {
            return new List<Vector2Int>();
        }
    }
}
