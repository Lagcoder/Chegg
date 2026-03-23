using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chegg.Framework
{
    public sealed class CheggGameService
    {
        private const int MaxFalseCheggCallsBeforeForfeit = 10;
        private const int ManaCapIncrementPerTurn = 1;
        private const int DrainCounterGracePeriod = 1;
        private const int AttackManaCost = 1;
        private const int DashManaCost = 1;

        public CheggGameState State { get; } = new CheggGameState();
        public ICheggPatternResolver PatternResolver { get; set; } = new EmptyPatternResolver();

        public DeckValidationResult ValidateDeck(IEnumerable<CardId> deck)
        {
            var result = new DeckValidationResult { IsValid = true };
            var list = deck.ToList();
            if (list.Count > 24)
            {
                result.IsValid = false;
                result.Errors.Add("Deck cannot exceed 24 cards.");
            }

            int totalCost = 0;
            foreach (var id in list)
            {
                if (!CheggCatalog.Cards.TryGetValue(id, out CardDefinition def))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Unknown card in deck: {id}");
                    continue;
                }

                if (!def.CanBeInDeck)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Card is token-only and cannot be in deck: {id}");
                }

                totalCost += def.ManaCost;
            }

            if (totalCost > 48)
            {
                result.IsValid = false;
                result.Errors.Add("Deck total mana cost cannot exceed 48.");
            }

            return result;
        }

        public bool TrySpawnMinion(CheggPlayer player, CardId cardId, Vector2Int position, out string reason)
        {
            reason = string.Empty;
            if (!CheggCatalog.Cards.TryGetValue(cardId, out CardDefinition def) || def.Kind != CardKind.Minion)
            {
                reason = "Card is not a minion.";
                return false;
            }

            if (!State.Board.IsOnBoard(position))
            {
                reason = "Position is out of bounds.";
                return false;
            }

            if (State.GetMinionAt(position) != null)
            {
                reason = "Tile is occupied.";
                return false;
            }

            if (!State.Board.IsSpawnZoneFor(position, player))
            {
                reason = "Minion must be spawned in owner's spawn zone.";
                return false;
            }

            var tile = State.Board.GetTile(position);
            if (tile.TileType == CheggTileType.Water && !def.IsSwimming && !def.IsFlying)
            {
                reason = "Cannot spawn non-swimmer/flyer directly on water.";
                return false;
            }

            var playerState = State.GetPlayerState(player);
            if (playerState.CurrentMana < def.ManaCost)
            {
                reason = "Not enough mana.";
                return false;
            }

            playerState.CurrentMana -= def.ManaCost;
            State.Minions.Add(new MinionInstance(cardId, player, position));
            return true;
        }

        public bool ResolveDrowningChoice(CheggPlayer player, DrowningResolution resolution)
        {
            List<MinionInstance> drowning = GetDrowningMinions(player).ToList();
            if (drowning.Count == 0)
            {
                return false;
            }

            if (resolution == DrowningResolution.LetDrownAndContinueTurn)
            {
                foreach (var minion in drowning)
                {
                    RemoveMinion(minion);
                }
                return true;
            }

            foreach (var minion in drowning)
            {
                TryMoveDrowningMinionLaterally(minion);
            }
            EndTurnInternal(skipBecauseDrowning: true);
            return true;
        }

        public void StartTurn(CheggPlayer player)
        {
            var playerState = State.GetPlayerState(player);
            State.CurrentTurn = player;
            RefreshManaForTurn(playerState);

            DrawOne(playerState);

            foreach (var minion in State.GetPlayerMinions(player))
            {
                minion.AttacksUsedThisTurn = 0;
                minion.DashesUsedThisTurn = 0;
                minion.FreeMovesUsedThisTurn = 0;
                if (minion.HasSummoningSickness)
                {
                    minion.HasSummoningSickness = false;
                }
            }
        }

        public void EndTurn(CheggPlayer player, bool calledChegg, bool checkIsValid)
        {
            if (State.CurrentTurn != player || State.Outcome != CheggMatchOutcome.Ongoing)
            {
                return;
            }

            var playerState = State.GetPlayerState(player);
            if (calledChegg)
            {
                if (checkIsValid)
                {
                    playerState.CalledCheggLastTurn = true;
                }
                else
                {
                    playerState.FalseCheggCalls++;
                    playerState.CalledCheggLastTurn = false;
                    if (playerState.FalseCheggCalls >= MaxFalseCheggCallsBeforeForfeit)
                    {
                        State.Outcome = player == CheggPlayer.Red ? CheggMatchOutcome.BlueWins : CheggMatchOutcome.RedWins;
                        return;
                    }
                }
            }
            else
            {
                playerState.CalledCheggLastTurn = false;
            }

            ProcessEndOfTurnEffects(player);
            EndTurnInternal(skipBecauseDrowning: false);
        }

        public void EnterTile(MinionInstance minion, Vector2Int newPosition)
        {
            minion.Position = newPosition;

            if (!State.Board.IsOnBoard(newPosition))
            {
                return;
            }

            CheggTile tile = State.Board.GetTile(newPosition);
            if (tile.ZoneType == (minion.Owner == CheggPlayer.Red ? CheggZoneType.RedSpawn : CheggZoneType.BlueSpawn))
            {
                RemoveEffect(minion, EffectKind.Hunger);
                RemoveEffect(minion, EffectKind.BadOmen);
            }

            if (tile.TileType == CheggTileType.Water)
            {
                if (HasEffect(minion, EffectKind.Fire))
                {
                    RemoveEffect(minion, EffectKind.Fire);
                }
            }
        }

        public void ApplyEffect(MinionInstance minion, EffectKind effect, int? durationOverride = null)
        {
            if (!CheggEffectCatalog.Definitions.TryGetValue(effect, out var def))
            {
                return;
            }

            int duration = durationOverride ?? def.DefaultDurationTurns;
            var existing = minion.Effects.FirstOrDefault(e => e.Kind == effect);
            if (existing != null)
            {
                existing.RemainingTurns = duration;
                existing.Consumed = false;
            }
            else
            {
                minion.Effects.Add(new EffectInstance(effect, duration));
            }
        }

        public bool CanDash(MinionInstance minion)
        {
            return !HasEffect(minion, EffectKind.Slowness) && !HasEffect(minion, EffectKind.Stunned);
        }

        public bool CanAttack(MinionInstance minion)
        {
            return !HasEffect(minion, EffectKind.Weakness) && !HasEffect(minion, EffectKind.Stunned);
        }

        public int GetFreeMoveCount(MinionInstance minion)
        {
            if (HasEffect(minion, EffectKind.Hunger))
            {
                return 0;
            }

            return HasEffect(minion, EffectKind.Speed) ? 2 : 1;
        }

        public int GetAttackCountAllowance(MinionInstance minion)
        {
            return HasEffect(minion, EffectKind.Strength) ? 2 : 1;
        }

        public bool IsKingKillAllowed(MinionInstance attackingMinion)
        {
            return !HasEffect(attackingMinion, EffectKind.BadOmen);
        }

        public bool TryMove(CheggPlayer player, Guid minionId, Vector2Int target, bool spendDash, out string reason)
        {
            reason = string.Empty;
            if (State.CurrentTurn != player)
            {
                reason = "Not this player's turn.";
                return false;
            }

            var minion = State.Minions.FirstOrDefault(m => m.InstanceId == minionId);
            if (minion == null || minion.Owner != player)
            {
                reason = "Minion not found for player.";
                return false;
            }

            if (minion.HasSummoningSickness)
            {
                reason = "Minion cannot act on spawn turn.";
                return false;
            }

            if (HasEffect(minion, EffectKind.Stunned))
            {
                reason = "Minion is stunned.";
                return false;
            }

            if (!State.Board.IsOnBoard(target))
            {
                reason = "Target is out of bounds.";
                return false;
            }

            if (State.GetMinionAt(target) != null)
            {
                reason = "Target tile is occupied.";
                return false;
            }

            if (minion.AttacksUsedThisTurn > 0)
            {
                reason = "Cannot move after attacking this turn.";
                return false;
            }

            var validTargets = PatternResolver.GetValidMoveTargets(State, minion);
            if (!validTargets.Contains(target))
            {
                reason = "Target is not in move pattern.";
                return false;
            }

            if (spendDash)
            {
                if (!CanDash(minion))
                {
                    reason = "Minion cannot dash.";
                    return false;
                }

                var playerState = State.GetPlayerState(player);
                if (playerState.CurrentMana < DashManaCost)
                {
                    reason = "Not enough mana for dash.";
                    return false;
                }

                playerState.CurrentMana -= DashManaCost;
                minion.DashesUsedThisTurn++;
            }
            else
            {
                int freeAllowance = GetFreeMoveCount(minion);
                if (minion.FreeMovesUsedThisTurn >= freeAllowance)
                {
                    reason = "No free move remaining.";
                    return false;
                }

                minion.FreeMovesUsedThisTurn++;
            }

            EnterTile(minion, target);
            minion.IsDrowning = IsDrowning(minion);

            return true;
        }

        public bool TryAttack(CheggPlayer player, Guid attackerId, Vector2Int target, out string reason)
        {
            reason = string.Empty;
            if (State.CurrentTurn != player)
            {
                reason = "Not this player's turn.";
                return false;
            }

            var attacker = State.Minions.FirstOrDefault(m => m.InstanceId == attackerId);
            if (attacker == null || attacker.Owner != player)
            {
                reason = "Attacker not found for player.";
                return false;
            }

            if (attacker.HasSummoningSickness)
            {
                reason = "Minion cannot act on spawn turn.";
                return false;
            }

            if (!CanAttack(attacker))
            {
                reason = "Minion cannot attack right now.";
                return false;
            }

            if (attacker.FreeMovesUsedThisTurn > 0 || attacker.DashesUsedThisTurn > 0)
            {
                reason = "Cannot attack after moving/dashing this turn.";
                return false;
            }

            int attackAllowance = GetAttackCountAllowance(attacker);
            if (attacker.AttacksUsedThisTurn >= attackAllowance)
            {
                reason = "No attack action remaining.";
                return false;
            }

            var playerState = State.GetPlayerState(player);
            if (playerState.CurrentMana < AttackManaCost)
            {
                reason = "Not enough mana for attack.";
                return false;
            }

            var validTargets = PatternResolver.GetValidAttackTargets(State, attacker);
            if (!validTargets.Contains(target))
            {
                reason = "Target is not in attack pattern.";
                return false;
            }

            var defender = State.GetMinionAt(target);
            if (defender == null || defender.Owner == player)
            {
                reason = "No valid enemy target at tile.";
                return false;
            }

            bool defenderIsKing = CheggCatalog.Cards[defender.CardId].IsKing;
            if (defenderIsKing && !IsKingKillAllowed(attacker))
            {
                reason = "Bad Omen prevents king kill/check.";
                return false;
            }

            playerState.CurrentMana -= AttackManaCost;
            attacker.AttacksUsedThisTurn++;
            ResolveAttackResult(attacker, defender);
            return true;
        }

        public bool IsDrowning(MinionInstance minion)
        {
            if (!State.Board.IsOnBoard(minion.Position))
            {
                return false;
            }

            CheggTile tile = State.Board.GetTile(minion.Position);
            if (tile.TileType != CheggTileType.Water)
            {
                return false;
            }

            if (HasEffect(minion, EffectKind.WaterBreathing))
            {
                return false;
            }

            var def = CheggCatalog.Cards[minion.CardId];
            bool survivesWater = def.MovementTraits.HasFlag(MovementTraits.Swim) || def.MovementTraits.HasFlag(MovementTraits.Fly);
            return !survivesWater;
        }

        public bool TryConsumeResistance(MinionInstance minion)
        {
            var resistance = minion.Effects.FirstOrDefault(e => e.Kind == EffectKind.Resistance && !e.Consumed);
            if (resistance == null)
            {
                return false;
            }

            resistance.Consumed = true;
            minion.Effects.Remove(resistance);
            return true;
        }

        public IEnumerable<MinionInstance> GetDrowningMinions(CheggPlayer player)
        {
            foreach (var minion in State.GetPlayerMinions(player))
            {
                minion.IsDrowning = IsDrowning(minion);
                if (minion.IsDrowning)
                {
                    yield return minion;
                }
            }
        }

        private void EndTurnInternal(bool skipBecauseDrowning)
        {
            if (!skipBecauseDrowning)
            {
                var current = State.GetPlayerState(State.CurrentTurn);
                ProcessDrainCounterDelta(current);
            }

            State.CurrentTurn = State.CurrentTurn == CheggPlayer.Red ? CheggPlayer.Blue : CheggPlayer.Red;
            State.TurnNumber++;
            StartTurn(State.CurrentTurn);
        }

        private void ProcessDrainCounterDelta(CheggPlayerState playerState)
        {
            if (playerState.IsInCheck)
            {
                playerState.DrainCounter++;
            }
            else if (playerState.DrainCounter > 0)
            {
                playerState.DrainCounter--;
            }
        }

        private void RefreshManaForTurn(CheggPlayerState playerState)
        {
            playerState.BaseManaCap = Math.Min(playerState.MaxManaCap, playerState.BaseManaCap + ManaCapIncrementPerTurn);

            int drainPenalty = Mathf.Max(0, playerState.DrainCounter - DrainCounterGracePeriod);
            int effectiveCap = Mathf.Max(0, playerState.BaseManaCap - drainPenalty);
            playerState.CurrentMana = effectiveCap;
        }

        private void DrawOne(CheggPlayerState playerState)
        {
            if (playerState.Deck.Count == 0)
            {
                return;
            }

            CardId top = playerState.Deck[0];
            playerState.Deck.RemoveAt(0);
            playerState.Hand.Add(top);
        }

        private void ProcessEndOfTurnEffects(CheggPlayer player)
        {
            var owned = State.GetPlayerMinions(player).ToList();
            foreach (var minion in owned)
            {
                var snapshot = minion.Effects.ToList();
                foreach (var effect in snapshot)
                {
                    if (!CheggEffectCatalog.Definitions.TryGetValue(effect.Kind, out EffectDefinition def))
                    {
                        continue;
                    }

                    if (def.RemovalRule == EffectRemovalRule.DurationEnds || def.RemovalRule == EffectRemovalRule.EnterWaterOrDurationEnds)
                    {
                        if (effect.RemainingTurns > 0)
                        {
                            effect.RemainingTurns--;
                        }
                    }

                    if (IsExpiredLethalEffect(effect))
                    {
                        RemoveMinion(minion);
                        break;
                    }

                    if (def.RemovalRule == EffectRemovalRule.DurationEnds && effect.RemainingTurns <= 0)
                    {
                        minion.Effects.Remove(effect);
                    }
                    else if (def.RemovalRule == EffectRemovalRule.EnterWaterOrDurationEnds)
                    {
                        bool inWater = State.Board.IsOnBoard(minion.Position) && State.Board.IsWater(minion.Position);
                        if (inWater || effect.RemainingTurns <= 0)
                        {
                            minion.Effects.Remove(effect);
                        }
                    }
                }
            }
        }

        private void RemoveEffect(MinionInstance minion, EffectKind kind)
        {
            minion.Effects.RemoveAll(e => e.Kind == kind);
        }

        private bool HasEffect(MinionInstance minion, EffectKind kind)
        {
            return minion.Effects.Any(e => e.Kind == kind && !e.Consumed);
        }

        private void RemoveMinion(MinionInstance minion)
        {
            State.Minions.Remove(minion);
        }

        private void ResolveAttackResult(MinionInstance attacker, MinionInstance defender)
        {
            bool blockedByResistance = TryConsumeResistance(defender);
            if (blockedByResistance)
            {
                return;
            }

            RemoveMinion(defender);
            if (CheggCatalog.Cards[defender.CardId].IsKing)
            {
                State.Outcome = defender.Owner == CheggPlayer.Red ? CheggMatchOutcome.BlueWins : CheggMatchOutcome.RedWins;
            }
        }

        private static bool IsExpiredLethalEffect(EffectInstance effect)
        {
            if (effect.RemainingTurns > 0)
            {
                return false;
            }

            return effect.Kind == EffectKind.Poison || effect.Kind == EffectKind.Fire;
        }

        private void TryMoveDrowningMinionLaterally(MinionInstance minion)
        {
            Vector2Int[] lateral =
            {
                new Vector2Int(-1, 0),
                new Vector2Int(1, 0)
            };

            foreach (var offset in lateral)
            {
                var candidate = minion.Position + offset;
                if (!State.Board.IsOnBoard(candidate))
                {
                    continue;
                }

                if (State.GetMinionAt(candidate) != null)
                {
                    continue;
                }

                minion.Position = candidate;
                minion.IsDrowning = IsDrowning(minion);
                return;
            }
        }
    }
}
