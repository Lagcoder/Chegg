using System.Collections.Generic;
using GameData;
using GameLogic;
using UnityEngine;

// This file defines the base Piece structure and all unit types.

public abstract class Piece
{
    // --- Core Properties (State) ---
    public string type;
    public PlayerData player;
    public Vector2Int position;
    public int health; 
    public int attack; 
    public int movement; 

    // --- State Flags (Resets at end of turn by GameEngine) ---
    public bool hasFreeMove = true; 
    public bool isStunned = false; 
    public bool CanAct { get; set; }

    // --- Constructor ---
    public Piece(string type, Vector2Int position, PlayerData player, int health, int attack, int movement)
    {
        this.type = type;
        this.position = position;
        this.player = player;
        this.health = health;
        this.attack = attack;
        this.movement = movement;
        this.CanAct = true; // All pieces start ready to act
    }

    // --- Abstract Methods (Contracts that must be implemented by children) ---

    public abstract List<Vector2Int> GetValidMoves(GameEngine logic); 
    public abstract List<Vector2Int> GetValidAttacks(GameEngine logic);
    
    // Defense Contract: Called when the piece is targeted.
    // MUST return TRUE if the piece was successfully hit (triggers attacker's cost/action loss).
    // MUST return FALSE if the piece was immune, dodged, or the attack failed (attacker retains cost/action).
    public abstract bool OnAttacked(Piece attacker, string attackType, GameEngine logic);
    
    // --- Shared Movement Helpers ---
    
    protected List<Vector2Int> Get8MoveSet(GameEngine logic)
    {
        Vector2Int[] cardinalDiagonalOffsets = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(0, -1),  // Down
        new Vector2Int(1, 0),   // Right
        new Vector2Int(-1, 0),   // Left
        new Vector2Int(1, 1),   // Up-Right
        new Vector2Int(-1, 1),  // Up-Left
        new Vector2Int(1, -1),  // Down-Right
        new Vector2Int(-1, -1)  // Down-Left
    };

    List<Vector2Int> potentialPositions = new List<Vector2Int>();

    foreach (Vector2Int offset in cardinalDiagonalOffsets)
    {
        Vector2Int newPos = position + offset;

        if (logic.IsOnBoard(newPos))
        {
            // Perform terrain check
            Tile tile = logic.GetTileAt(newPos);
            potentialPositions.Add(newPos);
        }
    }
    
    return potentialPositions;
    }

    protected List<Vector2Int> Get4MoveSet(GameEngine logic)
    {
    // Define the 4 cardinal directions (Up, Down, Left, Right)
    Vector2Int[] cardinalOffsets = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(0, -1),  // Down
        new Vector2Int(1, 0),   // Right
        new Vector2Int(-1, 0)   // Left
    };

    List<Vector2Int> potentialPositions = new List<Vector2Int>();

    foreach (Vector2Int offset in cardinalOffsets)
    {
        Vector2Int newPos = position + offset;

        if (logic.IsOnBoard(newPos))
        {
            // Perform terrain check
            Tile tile = logic.GetTileAt(newPos);
            
            potentialPositions.Add(newPos);
        }
    }
    
    return potentialPositions;
}
    protected List<Vector2Int> GetCustomMoveSet(GameEngine logic, Vector2Int[] CustomMoves)
    {

    List<Vector2Int> potentialPositions = new List<Vector2Int>();

    foreach (Vector2Int offset in CustomMoves)
    {
        Vector2Int newPos = position + offset;

        if (logic.IsOnBoard(newPos))
        {
            // Perform terrain check
            Tile tile = logic.GetTileAt(newPos);
            potentialPositions.Add(newPos);
        }
    }
    
    return potentialPositions;
    }
}

// --- Specific Piece Implementations ---

public class Villager : Piece
{
    // Health: 4, Attack: 1, Movement: 1
    public Villager(Vector2Int pos, PlayerData player) 
        : base("VILLAGER", pos, player, 4, 1, 1) {}

    public override List<Vector2Int> GetValidMoves(GameEngine logic)
    {
        if (!this.hasFreeMove || this.isStunned) return new List<Vector2Int>();
        List<Vector2Int> potentialMoves = Get8MoveSet(logic); 
        potentialMoves.RemoveAll(pos => logic.GetPieceAt(pos) != null);
        return potentialMoves;
    }

    public override List<Vector2Int> GetValidAttacks(GameEngine logic)
    {
        if (!this.CanAct || this.isStunned) return new List<Vector2Int>();
        List<Vector2Int> potentialTargets = Get8MoveSet(logic);
        potentialTargets.RemoveAll(pos => logic.GetPieceAt(pos) == null || logic.GetPieceAt(pos).player == this.player);
        return potentialTargets;
    }

    public override bool OnAttacked(Piece attacker, string attackType, GameEngine logic)
    {
        // Check if the attacker and the target belong to the same team.
        if (attacker.player.Color == this.player.Color)
        {
            // The user requested immunity to its own piece.
            // If the GameEngine allowed a friendly attack (e.g., AOE), the target piece ignores it.
            Debug.Log($"Villager at {this.position} used Teamwork Synergy against it's own {attacker.type}.");
            return false; // Attack fails, attacker retains action.
        }

        // Hostile Attack: The current game rule is immediate elimination upon a successful hit.
        Debug.Log($"{this.type} at {this.position} was hit and eliminated by {attacker.type}. Cheggmate!");
        logic.EliminatePiece(this);
        
        // Return true to signal the GameEngine that the attack succeeded, 
        // thus consuming the attacker's action.
        return true; 
    }
}

public class Zombie : Piece
{
    // Health: 3, Attack: 2, Movement: 1
    public Zombie(Vector2Int pos, PlayerData player) 
        : base("ZOMBIE", pos, player, 3, 2, 1) {}

    public override List<Vector2Int> GetValidMoves(GameEngine logic)
    {
        if (!this.hasFreeMove || this.isStunned) return new List<Vector2Int>();
        List<Vector2Int> potentialMoves = Get4MoveSet(logic); 
        potentialMoves.RemoveAll(pos => logic.GetPieceAt(pos) != null);
        return potentialMoves;
    }

    public override List<Vector2Int> GetValidAttacks(GameEngine logic)
    {
        if (!this.CanAct || this.isStunned) return new List<Vector2Int>();
        Vector2Int[] offsets = { new Vector2Int(0,1)};
        List<Vector2Int> potentialTargets = GetCustomMoveSet(logic, offsets); 
        potentialTargets.RemoveAll(pos => logic.GetPieceAt(pos) == null || logic.GetPieceAt(pos).player == this.player);
        return potentialTargets;
    }

    public override bool OnAttacked(Piece attacker, string attackType, GameEngine logic)
    {
        // Example logic: Zombies are immune to 'STUN' attacks
        if (attackType == "STUN")
        {
            Debug.Log($"Zombie is immune to STUN attacks!");
            return false; // Attack failed, attacker keeps action/mana
        }
        
        this.health -= attacker.attack;
        Debug.Log($"{this.type} at {this.position} took {attacker.attack} damage from {attacker.type}. Health remaining: {this.health}");

        if (this.health <= 0)
        {
            logic.EliminatePiece(this);
        }
        
        return true; // Attack landed successfully.
    }
}

public class Creeper : Piece
{
    // Health: 2, Attack: 1, Movement: 1
    public Creeper(Vector2Int pos, PlayerData player) 
        : base("CREEPER", pos, player, 2, 1, 1) {}
    
    public override List<Vector2Int> GetValidMoves(GameEngine logic)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        if (!this.hasFreeMove || this.isStunned) return moves;

        // Creeper moves diagonally
        Vector2Int[] offsets = { new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

        foreach (var offset in offsets)
        {
            Vector2Int newPos = position + offset;
            
            if (logic.IsOnBoard(newPos))
            {
                if (logic.GetPieceAt(newPos) == null) 
                {
                    moves.Add(newPos);
                }
            }
        }
        return moves;
    }
    
    public override List<Vector2Int> GetValidAttacks(GameEngine logic)
    {
        if (!this.CanAct || this.isStunned) return new List<Vector2Int>();
        
        List<Vector2Int> potentialTargets = new List<Vector2Int>();
        Vector2Int[] offsets = { new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

        foreach (var offset in offsets)
        {
            Vector2Int newPos = position + offset;
            Piece piece = logic.GetPieceAt(newPos); 

            if (logic.IsOnBoard(newPos) && piece != null && piece.player != this.player)
            {
                 potentialTargets.Add(newPos);
            }
        }
        return potentialTargets;
    }
    
    public override bool OnAttacked(Piece attacker, string attackType, GameEngine logic)
    {
        // Default damage model
        this.health -= attacker.attack;
        Debug.Log($"{this.type} at {this.position} took {attacker.attack} damage from {attacker.type}. Health remaining: {this.health}");

        if (this.health <= 0)
        {
            logic.EliminatePiece(this);
        }
        
        return true; // Attack landed successfully.
    }
}
