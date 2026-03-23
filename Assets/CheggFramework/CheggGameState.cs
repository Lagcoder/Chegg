using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chegg.Framework
{
    [Serializable]
    public sealed class CheggPlayerState
    {
        public CheggPlayer Player;
        public int MaxManaCap = 6;
        public int BaseManaCap = 1;
        public int CurrentMana = 1;
        public int DrainCounter;
        public bool CalledCheggLastTurn;
        public bool IsInCheck;
        public bool IsKingAlive = true;
        public readonly List<CardId> Deck = new List<CardId>();
        public readonly List<CardId> Hand = new List<CardId>();
        public readonly List<CardId> Discard = new List<CardId>();

        public CheggPlayerState(CheggPlayer player)
        {
            Player = player;
        }
    }

    [Serializable]
    public sealed class CheggGameState
    {
        public readonly CheggBoardState Board = new CheggBoardState();
        public readonly List<MinionInstance> Minions = new List<MinionInstance>();
        public readonly List<CardId> GlobalSpellDiscard = new List<CardId>();
        public CheggMatchOutcome Outcome = CheggMatchOutcome.Ongoing;
        public CheggPlayer CurrentTurn = CheggPlayer.Red;
        public int TurnNumber = 1;
        public CheggPlayerState Red = new CheggPlayerState(CheggPlayer.Red);
        public CheggPlayerState Blue = new CheggPlayerState(CheggPlayer.Blue);
        public Guid? MostRecentlyKilledMinionCard;

        public CheggPlayerState GetPlayerState(CheggPlayer player)
        {
            return player == CheggPlayer.Red ? Red : Blue;
        }

        public CheggPlayerState GetOpponentState(CheggPlayer player)
        {
            return player == CheggPlayer.Red ? Blue : Red;
        }

        public IEnumerable<MinionInstance> GetPlayerMinions(CheggPlayer player)
        {
            return Minions.Where(m => m.Owner == player);
        }

        public MinionInstance GetMinionAt(Vector2Int position)
        {
            return Minions.FirstOrDefault(m => m.Position == position);
        }
    }
}
