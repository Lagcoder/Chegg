using System.Collections.Generic;

namespace Chegg.Framework
{
    public static class CheggCatalog
    {
        public static readonly Dictionary<CardId, CardDefinition> Cards = BuildCards();

        private static Dictionary<CardId, CardDefinition> BuildCards()
        {
            var cards = new Dictionary<CardId, CardDefinition>();

            AddMinion(cards, CardId.VILLAGER, "Villager", 0, "King. Teamwork Synergy. Costs 1 mana to move. Dash total 2 mana.");
            AddMinion(cards, CardId.TRADER_KING, "Trader King", 0, "Alternate king form. Starts at -2 mana tempo. Trade ability in spawn.");
            AddMinion(cards, CardId.ZOMBIE_KING, "Zombie King", 0, "Alternate king form. Call Undead ability and gravestone interactions.");
            AddMinion(cards, CardId.PILLAGER_KING, "Pillager King", 0, "Alternate king form. Punishment and check-drain interactions.");

            AddMinion(cards, CardId.ZOMBIE, "Zombie", 1, string.Empty);
            AddMinion(cards, CardId.HUSK, "Husk", 0, "Zombie evolution when reaching opposite end.");
            AddMinion(cards, CardId.CREEPER, "Creeper", 1, "One-time self-destruct attack with friendly fire.");
            AddMinion(cards, CardId.PIG, "Pig", 1, "On spawn draw 1, on death draw 1. Spawn constraints on own side.");
            AddMinion(cards, CardId.SILVERFISH, "Silverfish", 1, "Tunnel movement. Death mana penalty. Hidden trap mode.");
            AddMinion(cards, CardId.RABBIT, "Rabbit", 2, "No attack/dash. Jump chaining. Becomes killer bunny in enemy spawn.");
            AddMinion(cards, CardId.PUFFER_FISH, "Puffer-Fish", 2, "Can swim. Simultaneous diagonal attack.");
            AddMinion(cards, CardId.IRON_GOLEM, "Iron Golem", 2, "Sweeping three-tile attack.");
            AddMinion(cards, CardId.FROG, "Frog", 2, "Can swim/jump. Pulls any minion 1 tile for 1 mana.");
            AddMinion(cards, CardId.DROWNED, "Drowned", 2, "Placed on drowning zombie. Water-only movement.");
            AddMinion(cards, CardId.GUARDIAN, "Guardian", 2, "Can swim. Laser push attack.");
            AddMinion(cards, CardId.LLAMA, "Llama", 2, "Forces random opponent card play/discard.");
            AddMinion(cards, CardId.TRADER_LLAMA, "Trader Llama", 3, "Llama form with controlled 1-of-9 choice.");
            AddMinion(cards, CardId.ZOMBIE_PIGMAN, "Zombie Pigman", 2, string.Empty);
            AddMinion(cards, CardId.SKELETON, "Skeleton", 3, "Ranged diagonal attacks.");
            AddMinion(cards, CardId.BLAZE, "Blaze", 3, "Instantly drowns in water.");
            AddMinion(cards, CardId.PHANTOM, "Phantom", 3, "Flying. Dark-tile spawn/move/attack restriction.");
            AddMinion(cards, CardId.DOLPHIN, "Dolphin", 3, "Swimming support bridge-like ally movement boost.");
            AddMinion(cards, CardId.PIGLIN, "Piglin", 3, "Places gold ingots; movement interaction trap.");
            AddMinion(cards, CardId.PIGLIN_BRUTE, "Piglin Brute", 0, "Piglin form that can attack movement spaces.");
            AddMinion(cards, CardId.PIGMAN_HORDE, "Pigman Horde", 0, "Emergent 2x2 formation behavior.");
            AddMinion(cards, CardId.SHEEP, "Sheep", 3, "On kill grants owner +4 next-turn mana, over-cap.");
            AddMinion(cards, CardId.ENDERMAN, "Enderman", 4, "Teleport swap ability. Instantly drowns in water.");
            AddMinion(cards, CardId.SLIME, "Slime", 4, "Jumping move-attacks with mana cost.");
            AddMinion(cards, CardId.SHULKER_BOX, "Shulker-Box", 4, "Line-of-fire constrained ranged attack.");
            AddMinion(cards, CardId.GHAST, "Ghast", 4, "Auto-forward each turn. Opponent mana suppression aura.");
            AddMinion(cards, CardId.HORSE, "Horse", 4, "Not a jump. Cannot travel over water.");
            AddMinion(cards, CardId.ZOMBIE_HORSE, "Zombie Horse", 0, "Horse-zombie transformation.");
            AddMinion(cards, CardId.SKELETON_HORSE, "Skeleton Horse", 0, "Exact 3-tile linear move. Can swim.");
            AddMinion(cards, CardId.ELDER_GUARDIAN, "Elder Guardian", 5, "Swimming. Opponent mining fatigue draw lock.");
            AddMinion(cards, CardId.EVOKER, "Evoker", 5, "Fangs attack and vex summon ability.");
            AddMinion(cards, CardId.VEX, "Vex", 0, "Evoker temporary summon token.");
            AddMinion(cards, CardId.PARROT, "Parrot", 5, "Flying. Copies adjacent minion attack.");
            AddMinion(cards, CardId.CAT, "Cat", 5, "Static support; grants +1 temporary mana each turn.");
            AddMinion(cards, CardId.SNIFFER, "Sniffer", 5, "Deck theft draws and death discard penalty.");
            AddMinion(cards, CardId.CREAKING, "Creaking", 5, "Heart-bound immortal minion with sight restrictions.");
            AddMinion(cards, CardId.CREAKING_HEART, "Creaking Heart", 0, "External objective tied to Creaking.");
            AddMinion(cards, CardId.SILVERFISH_SPAWNER, "Silverfish Spawner", 5, "Spawns silverfish once per turn.");
            AddMinion(cards, CardId.GRAVESTONE, "Gravestone", 0, "Zombie king token. Blocks movement.");
            AddMinion(cards, CardId.GOLD_INGOT, "Gold Ingot", 0, "Piglin token for movement interaction.");
            AddMinion(cards, CardId.WITHER, "Wither", 6, "Flying. 2 mana ranged splash attack.");
            AddMinion(cards, CardId.ENDER_DRAGON, "Ender Dragon", 6, "Uncontrollable temporary 2x2 flying sweeper.");

            AddSpell(cards, CardId.WEB_SHOT, "Web Shot", 2, "Area movement block and ensnare.");
            AddSpell(cards, CardId.WIND_CHARGE, "Wind Charge", 2, "2x2 push with edge stun and water breathing rider.");
            AddSpell(cards, CardId.LIGHTING, "Lighting", 3, "Single-target 2-turn stun.");
            AddSpell(cards, CardId.TNT, "TNT", 3, "Delayed 3x3 detonation blocker.");
            AddSpell(cards, CardId.ECHO_SHARD, "Echo Shard", 3, "Resurrect most recently killed minion in spawn.");
            AddSpell(cards, CardId.CHORUS_FRUIT, "Chorus Fruit", 3, "Teleport own minion to spawn and cleanse utility.");
            AddSpell(cards, CardId.FIREBALL, "Fireball", 4, "Target-two pressure; opponent saves one.");
            AddSpell(cards, CardId.WIND_BLAST, "Wind Blast", 4, "3x3 push with center stun.");
            AddSpell(cards, CardId.OMINOUS_BOTTLE, "Ominous Bottle", 4, "Applies Bad Omen king-safety lock.");
            AddSpell(cards, CardId.TRIDENT, "Trident", 6, "Large area stun and delayed center re-stun.");

            return cards;
        }

        private static void AddMinion(Dictionary<CardId, CardDefinition> cards, CardId id, string displayName, int manaCost, string rulesText)
        {
            cards[id] = new CardDefinition
            {
                Id = id,
                DisplayName = displayName,
                Kind = CardKind.Minion,
                ManaCost = manaCost,
                IsKing = id == CardId.VILLAGER,
                IsAlternateKingForm = id == CardId.TRADER_KING || id == CardId.ZOMBIE_KING || id == CardId.PILLAGER_KING,
                IsFlying = id == CardId.PHANTOM || id == CardId.PARROT || id == CardId.WITHER || id == CardId.ENDER_DRAGON,
                IsSwimming = id == CardId.PUFFER_FISH || id == CardId.FROG || id == CardId.GUARDIAN || id == CardId.DOLPHIN || id == CardId.SKELETON_HORSE || id == CardId.ELDER_GUARDIAN,
                MovementTraits = ResolveMovementTraits(id),
                MovementPattern = string.Empty,
                AttackPattern = string.Empty,
                RulesText = rulesText,
                CanBeInDeck = IsDeckCard(id),
                IsTokenOnly = !IsDeckCard(id)
            };
        }

        private static void AddSpell(Dictionary<CardId, CardDefinition> cards, CardId id, string displayName, int manaCost, string rulesText)
        {
            cards[id] = new CardDefinition
            {
                Id = id,
                DisplayName = displayName,
                Kind = CardKind.Spell,
                ManaCost = manaCost,
                MovementPattern = string.Empty,
                AttackPattern = string.Empty,
                RulesText = rulesText
            };
        }

        private static bool IsDeckCard(CardId id)
        {
            switch (id)
            {
                case CardId.HUSK:
                case CardId.TRADER_LLAMA:
                case CardId.PIGLIN_BRUTE:
                case CardId.PIGMAN_HORDE:
                case CardId.ZOMBIE_HORSE:
                case CardId.SKELETON_HORSE:
                case CardId.VEX:
                case CardId.CREAKING_HEART:
                case CardId.GRAVESTONE:
                case CardId.GOLD_INGOT:
                    return false;
                default:
                    return true;
            }
        }

        private static MovementTraits ResolveMovementTraits(CardId id)
        {
            MovementTraits traits = MovementTraits.None;
            if (id == CardId.PHANTOM || id == CardId.PARROT || id == CardId.WITHER || id == CardId.ENDER_DRAGON)
            {
                traits |= MovementTraits.Fly;
            }

            if (id == CardId.PUFFER_FISH || id == CardId.FROG || id == CardId.GUARDIAN || id == CardId.DOLPHIN || id == CardId.SKELETON_HORSE || id == CardId.ELDER_GUARDIAN || id == CardId.DROWNED)
            {
                traits |= MovementTraits.Swim;
            }

            if (id == CardId.RABBIT || id == CardId.FROG || id == CardId.SLIME)
            {
                traits |= MovementTraits.Jump;
            }

            if (id == CardId.SILVERFISH)
            {
                traits |= MovementTraits.Tunnel;
            }

            return traits;
        }
    }
}
