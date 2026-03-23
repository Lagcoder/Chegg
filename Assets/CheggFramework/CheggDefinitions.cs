using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chegg.Framework
{
    public enum CheggPlayer
    {
        Red,
        Blue
    }

    public enum CardKind
    {
        Minion,
        Spell
    }

    public enum CheggTileType
    {
        Ground,
        Water,
        Bridge
    }

    public enum CheggZoneType
    {
        Neutral,
        RedSpawn,
        BlueSpawn
    }

    public enum CheggMatchOutcome
    {
        Ongoing,
        RedWins,
        BlueWins,
        Draw
    }

    public enum DrowningResolution
    {
        SkipTurnAndMoveLaterally,
        LetDrownAndContinueTurn
    }

    [Flags]
    public enum MovementTraits
    {
        None = 0,
        Swim = 1 << 0,
        Fly = 1 << 1,
        Jump = 1 << 2,
        Tunnel = 1 << 3
    }

    public enum CardId
    {
        VILLAGER,
        TRADER_KING,
        ZOMBIE_KING,
        PILLAGER_KING,
        ZOMBIE,
        HUSK,
        CREEPER,
        PIG,
        SILVERFISH,
        RABBIT,
        PUFFER_FISH,
        IRON_GOLEM,
        FROG,
        DROWNED,
        GUARDIAN,
        LLAMA,
        TRADER_LLAMA,
        ZOMBIE_PIGMAN,
        SKELETON,
        BLAZE,
        PHANTOM,
        DOLPHIN,
        PIGLIN,
        PIGLIN_BRUTE,
        PIGMAN_HORDE,
        SHEEP,
        ENDERMAN,
        SLIME,
        SHULKER_BOX,
        GHAST,
        HORSE,
        ZOMBIE_HORSE,
        SKELETON_HORSE,
        ELDER_GUARDIAN,
        EVOKER,
        VEX,
        PARROT,
        CAT,
        SNIFFER,
        CREAKING,
        CREAKING_HEART,
        SILVERFISH_SPAWNER,
        GRAVESTONE,
        GOLD_INGOT,
        WITHER,
        ENDER_DRAGON,
        WEB_SHOT,
        WIND_CHARGE,
        LIGHTING,
        TNT,
        ECHO_SHARD,
        CHORUS_FRUIT,
        FIREBALL,
        WIND_BLAST,
        OMINOUS_BOTTLE,
        TRIDENT
    }

    [Serializable]
    public sealed class CardDefinition
    {
        public CardId Id;
        public string DisplayName = string.Empty;
        public CardKind Kind;
        public int ManaCost;
        public bool IsKing;
        public bool IsAlternateKingForm;
        public bool IsFlying;
        public bool IsSwimming;
        public MovementTraits MovementTraits;
        public bool CanBeInDeck = true;
        public bool IsTokenOnly;
        public string MovementPattern = string.Empty;
        public string AttackPattern = string.Empty;
        public string RulesText = string.Empty;
    }

    [Serializable]
    public sealed class MinionInstance
    {
        public Guid InstanceId = Guid.NewGuid();
        public CardId CardId;
        public CheggPlayer Owner;
        public Vector2Int Position;
        public bool HasSummoningSickness = true;
        public bool HasFreeMove = true;
        public bool HasAttackedThisTurn;
        public bool HasDashedThisTurn;
        public bool IsDrowning;
        public bool IsStunned;
        public int StunnedTurnsRemaining;
        public bool HasBadOmen;
        public readonly List<EffectInstance> Effects = new List<EffectInstance>();

        public MinionInstance(CardId cardId, CheggPlayer owner, Vector2Int position)
        {
            CardId = cardId;
            Owner = owner;
            Position = position;
        }
    }

    [Serializable]
    public sealed class DeckValidationResult
    {
        public bool IsValid;
        public readonly List<string> Errors = new List<string>();
    }
}
