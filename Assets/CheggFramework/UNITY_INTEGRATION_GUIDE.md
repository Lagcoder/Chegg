# CHEGG Framework Unity Integration Guide

This guide explains how to integrate the new `Chegg.Framework` rules/data framework into this Unity project.

## 1) What was added

Folder: `Assets/CheggFramework/`

- `CheggDefinitions.cs`  
  Core enums/models for cards, minions, board/game outcome, deck validation result.
- `CheggCatalog.cs`  
  Full card catalog (minions + spells) from the design doc, with movement/attack patterns intentionally left blank.
- `CheggBoard.cs`  
  10x8 board with spawn zones, 2-row water center, and 2x2 center bridge.
- `CheggEffects.cs`  
  Full effects catalog and effect metadata (type/removal/duration/mechanics).
- `CheggGameState.cs`  
  Serializable game/player state container.
- `CheggGameService.cs`  
  Turn/mana/drain/drowning/effect-processing framework service.

---

## 2) Design scope

This framework intentionally **does not implement specific move/attack patterns** yet.  
It provides the complete rule scaffolding so pattern logic can be plugged in later.

Implemented now:

- board topology and tile zones
- deck constraints and validation
- mana progression and drain counter cap restrictions
- turn start/end flow (draw + refresh + effect processing)
- drowning detection and drowning turn resolution choices
- chegg-call bookkeeping and false-chegg forfeiture
- all specified effects:
  - Poison
  - Fire
  - Slowness
  - Weakness
  - Hunger
  - Bad Omen
  - Stunned
  - Speed
  - Strength
  - Resistance
  - Water Breathing

---

## 3) Unity setup steps

1. Open the project in Unity (`6000.2.10f1`).
2. Let Unity import scripts.
3. No package changes are required.
4. Create an integration MonoBehaviour script in `Assets/Scripts/` (example below).
5. Add the script to an empty GameObject in your scene.
6. In Play Mode, call framework functions from your current input/network flow.

---

## 4) Minimal integration example

Create file: `Assets/Scripts/CheggFrameworkBridge.cs`

```csharp
using UnityEngine;
using Chegg.Framework;

public class CheggFrameworkBridge : MonoBehaviour
{
    private CheggGameService _service;

    private void Awake()
    {
        _service = new CheggGameService();

        // Initialize first turn explicitly
        _service.StartTurn(CheggPlayer.Red);
    }

    // Example wrappers for your existing command/input pipeline:
    public bool SpawnRedZombie(Vector2Int boardPos, out string reason)
    {
        return _service.TrySpawnMinion(CheggPlayer.Red, CardId.ZOMBIE, boardPos, out reason);
    }

    public bool ResolveRedDrowningSkip()
    {
        return _service.ResolveDrowningChoice(CheggPlayer.Red, DrowningResolution.SkipTurnAndMoveLaterally);
    }

    public bool ResolveRedDrowningDeath()
    {
        return _service.ResolveDrowningChoice(CheggPlayer.Red, DrowningResolution.LetDrownAndContinueTurn);
    }
}
```

---

## 5) Mapping from existing server/game scripts

Your current scripts:

- `Assets/Scripts/GameServer.cs`
- `Assets/Scripts/GameLogic.cs`
- `Assets/Scripts/Piece.cs`

Recommended migration path:

1. Keep old gameplay scripts active while wiring read-only state from `CheggGameService`.
2. Route summon validation to `TrySpawnMinion(...)`.
3. At turn transitions:
   - call `StartTurn(currentPlayer)` at turn start
   - call `EndTurn(currentPlayer, calledChegg, checkIsValid)` at turn end
4. Before allowing normal turn actions, check drowning:
   - `GetDrowningMinions(currentPlayer)` and force resolution choice in UI
5. Apply effects centrally with `ApplyEffect(...)`.
6. Query effect restrictions via:
   - `CanDash(...)`
   - `CanAttack(...)`
   - `GetFreeMoveCount(...)`
   - `GetAttackCountAllowance(...)`
   - `IsKingKillAllowed(...)`

---

## 6) Effects behavior reference (implemented)

- **Poison**: remove minion at end of its next turn.
- **Fire**: remove after 2 turns unless unit enters water (water removes Fire immediately).
- **Slowness**: disables dash.
- **Weakness**: disables attacks.
- **Hunger**: no free move until own spawn zone is re-entered.
- **Bad Omen**: cannot check/kill king until own spawn zone is re-entered.
- **Stunned**: no move/attack/ability actions.
- **Speed**: free move allowance becomes 2.
- **Strength**: attack allowance becomes 2.
- **Resistance**: consumes itself on next lethal enemy hit (helper: `TryConsumeResistance`).
- **Water Breathing**: blocks drowning while active.

---

## 7) Data model usage notes

- Use `CheggCatalog.Cards` as authoritative card metadata.
- Use `CheggEffectCatalog.Definitions` as authoritative effect metadata.
- Keep per-instance state in `MinionInstance`.
- Keep full match state in `CheggGameState`.
- Serialize `CheggGameState` for save/network snapshots.

---

## 8) What to implement next (move/attack patterns)

When ready, add:

1. `ICheggPatternResolver` interface for move/attack generation.
2. Resolver implementations per minion card.
3. Validation layers:
   - board bounds
   - occupancy
   - water crossing/jump/fly constraints
   - action economy (free move vs dash vs attack)
   - check + chegg capture legality.

Keep this in a separate file set so the framework’s core state/rules remain stable and testable.
