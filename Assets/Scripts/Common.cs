using System;
using System.Collections.Generic;
using UnityEngine; // For Vector2Int (Unity's integer vector)

// --- GameData Namespace: Holds Core Data Structures ---
namespace GameData
{
    // A simple container for player-specific data
    public class PlayerData
    {
        public PlayerColor Color { get; private set; }
        public int Mana { get; set; }
        public List<MinionData> Hand { get; private set; }
        public List<Piece> Pieces { get; private set; }
        public PlayerData(PlayerColor color)
        {
            Color = color;
            Mana = 0;
            Hand = new List<MinionData>();
            Pieces = new List<Piece>();
        }
    }
    
    // Structure to represent a Minion Card in a player's hand
    public struct MinionData
    {
        public MinionType Type;
        public int Cost;
        // Cost is implicit for now, but would be added here later
    }

    public enum PlayerColor { WHITE, BLACK }
    
    public enum TerrainType { PLAIN, WATER }
    
    public struct Tile
    {
        public TerrainType TerrainType;
        public bool IsOccupied { get; set; }
        // Tile doesn't hold the piece reference (GameEngine does)

        public Tile(TerrainType type)
        {
            this.TerrainType = type;
            this.IsOccupied = false;
        }
    }
    
    public enum MinionType
    {
        VILLAGER,
        ZOMBIE,
        CREEPER,
        TRADER,
        // Add more units here
    }

    public enum UpdateType
    {
        MOVEMENT,
        ATTACK,
        SPAWN,
        END_TURN,
        ELIMINATION
    }

    [System.Serializable] // Important for Unity/networking
    public class BoardPieceData
    {
        public Vector2Int Position;
        // NOTE: If you changed to MinionType, this should be MinionType
        public MinionType Type; 
        public PlayerColor Color;
        public int Health;
        public bool CanAct;
        // Add any other properties you want to send to the client
    }
    // Packet sent to clients after a validated server action
    [System.Serializable]
public class GameStatePacket
{
    // FIX: Properties required by GetFullStateSnapshot() on lines 315-320
    public List<BoardPieceData> PiecesOnBoard; // CS0117 fix
    public PlayerData PlayerWhite;             // CS0117 fix
    public PlayerData PlayerBlack;             // CS0117 fix
    public PlayerColor CurrentTurn;            // CS0117 fix
    public bool IsGameOver;                   // CS0117 fix
    public Vector2Int SelectedPosition;       // CS0117 fix
    
    // The UpdateType and coordinates fields are likely for single action packets, 
    // but they can remain for a full snapshot, too.
    public UpdateType UpdateType;
    public PlayerColor PlayerExecutingAction;
    public Vector2Int SourcePos;
    public Vector2Int TargetPos;
    public MinionType MinionType;
    
    // FIX: Provide a parameterless constructor for the GetFullStateSnapshot method 
    // to use the object initializer syntax (the { ... } block).
    public GameStatePacket() { } 

    // The existing constructor for single actions:
    public GameStatePacket(UpdateType type, PlayerColor player, Vector2Int source, Vector2Int target)
    {
        this.UpdateType = type;
        this.PlayerExecutingAction = player;
        this.SourcePos = source;
        this.TargetPos = target;
    }
}
}