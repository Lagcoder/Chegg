using System;
using System.Collections.Generic;
using System.Linq;
using GameData;
using GameLogic;
using UnityEngine; // Essential for MonoBehaviour and Debug.Log

// IMPORTANT: This class MUST inherit from MonoBehaviour to run correctly in Unity.
public class GameServer : MonoBehaviour
{
    private GameEngine game;
    // Removed the 'isRunning' flag and the synchronous loop.
    
    // Simulates mapping network clients to their PlayerData
    private PlayerData clientWhite;
    private PlayerData clientBlack;
    
    // The constructor is replaced by the Start method in Unity
    void Start()
    {
        // Changed Console.WriteLine to Debug.Log
        Debug.Log("Initializing GameEngine and Authoritative Server...");
        game = new GameEngine();
        clientWhite = game.PlayerWhite; 
        clientBlack = game.PlayerBlack; 
        
        // Display initial prompt instructions
        DisplayPrompt();
    }

    // This method is the public entry point for processing simulated client commands.
    public void ExecuteCommand(string input)
    {
        ProcessClientCommand(input);
        // Display prompt again after processing, so the user sees the new state/turn.
        DisplayPrompt(); 
    }
    
    private void DisplayPrompt()
    {
        // Changed Console.WriteLine to Debug.Log
        Debug.Log($"\n--- Server State ---");
        Debug.Log($"Current Player: {game.CurrentPlayer.Color}. Mana: {game.CurrentPlayer.Mana}");
        // Use a single Debug.Log for the instructions
        Debug.Log("Enter Command (Format: PLAYER COMMAND [PARAMS]): e.g., WHITE select 4,0; BLACK end; WHITE summon ZOMBIE 4,1"); 
    }

    private void ProcessClientCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        string[] parts = input.Trim().Split(' ');
        if (parts.Length < 2) return;

        // 1. Determine which client sent the command
        string playerKey = parts[0].ToUpper();
        PlayerColor sendingPlayerColor;
        
        if (playerKey == "WHITE") sendingPlayerColor = PlayerColor.WHITE; 
        else if (playerKey == "BLACK") sendingPlayerColor = PlayerColor.BLACK;
        else { Debug.Log("Invalid player key (Must be WHITE or BLACK)."); return; } // Changed to Debug.Log
        
        PlayerData sendingPlayer = (sendingPlayerColor == PlayerColor.WHITE) ? clientWhite : clientBlack;
        
        // 2. Authorize the command (Must be the current player's turn)
        if (sendingPlayer != game.CurrentPlayer) 
        {
            // Changed to Debug.LogError for visibility
            Debug.LogError($"AUTHORITY CHECK FAILED: Player {sendingPlayerColor} cannot move, it is Player {game.CurrentPlayer.Color}'s turn.");
            return;
        }

        string command = parts[1].ToLower();
        string[] commandArgs = parts.Skip(2).ToArray(); 

        try
        {
            GameStatePacket packet = null;

            switch (command)
            {
                case "end":
                    game.EndTurn();
                    break;
                
                case "select":
                    ProcessSelection(commandArgs);
                    // Changed to Debug.Log
                    Debug.Log($"Server logged selection for Player {sendingPlayerColor}."); 
                    break;
                
                case "move":
                case "attack":
                    packet = ProcessAction(commandArgs, command);
                    break;
                    
                case "summon":
                    packet = ProcessSummon(commandArgs);
                    break;
                    
                case "exit":
                    // Removed isRunning=false since there is no loop, replaced with log
                    Debug.Log("Server simulation stopped."); 
                    break;
                    
                default:
                    Debug.Log("Unknown command."); // Changed to Debug.Log
                    break;
            }
            
            if (packet != null)
            {
                SimulateClientBroadcast(packet);
            }
        }
        catch (ArgumentException ex)
        {
            // Changed to Debug.LogError
            Debug.LogError($"SERVER ERROR: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Changed to Debug.LogError
            Debug.LogError($"UNEXPECTED SERVER EXCEPTION: {ex.Message}");
        }
    }
    
    // --- Command Processing & Logic Calls ---

    private Vector2Int ParsePosition(string posString)
    {
        string[] coords = posString.Split(',');
        if (coords.Length != 2 || 
            !int.TryParse(coords[0], out int x) || 
            !int.TryParse(coords[1], out int y))
        {
            throw new ArgumentException("Invalid position format. Use X,Y (e.g., 4,0).");
        }
        return new Vector2Int(x, y);
    }
    
    private void ProcessSelection(string[] commandArgs)
    {
        if (commandArgs.Length < 1) throw new ArgumentException("Select command requires a position.");
        Vector2Int pos = ParsePosition(commandArgs[0]);
        game.HandleSquareClick(pos, null); 
    }


    private GameStatePacket ProcessAction(string[] commandArgs, string actionType)
    {
        if (game.SelectedPiece == null) 
        {
            throw new ArgumentException("No piece currently selected on the server. Cannot execute action.");
        }
        if (commandArgs.Length < 1) throw new ArgumentException($"{actionType} command requires a target position.");
        
        Vector2Int targetPos = ParsePosition(commandArgs[0]);
        Vector2Int sourcePos = game.SelectedPiece.position;
        
        // --- Execute Logic on the Server (Triggers Move or Capture) ---
        game.HandleSquareClick(targetPos, null); 
        
        // --- Prepare Packet for Broadcast ---
        GameStatePacket packet = new GameStatePacket(
            actionType.ToUpper() == "MOVE" ? UpdateType.MOVEMENT : UpdateType.ATTACK, 
            game.CurrentPlayer.Color, 
            sourcePos, 
            targetPos
        );
        
        return packet;
    }
    
    private GameStatePacket ProcessSummon(string[] commandArgs)
    {
        if (commandArgs.Length < 2) throw new ArgumentException("Summon command requires type and position.");

        string typeString = commandArgs[0].ToUpper(); 
        Vector2Int pos = ParsePosition(commandArgs[1]);

        MinionData? minionToSummon = game.CurrentPlayer.Hand
            .Cast<MinionData?>()
            .FirstOrDefault(m => m.Value.Type.ToString().Equals(typeString, StringComparison.OrdinalIgnoreCase));

        if (!minionToSummon.HasValue)
        {
            throw new ArgumentException($"Player does not have a '{typeString}' card in hand on the server.");
        }

        // --- Execute Logic on the Server ---
        game.HandleSquareClick(pos, minionToSummon.Value);
        
        // --- Prepare Packet for Broadcast ---
        GameStatePacket packet = new GameStatePacket(UpdateType.SPAWN, game.CurrentPlayer.Color, pos, pos);
        packet.MinionType = minionToSummon.Value.Type;
        
        return packet;
    }

    // --- Communication Simulation (Dummy Network Layer) ---
    
    private void SimulateClientBroadcast(GameStatePacket packet)
    {
        // Now using Unity's JsonUtility
        string json = JsonUtility.ToJson(packet, true); 
        
        // Changed Console.WriteLine to Debug.Log
        Debug.Log("\n--- AUTHORITATIVE BROADCAST ---");
        Debug.Log($"ACTION: {packet.UpdateType}");
        Debug.Log($"EXECUTED BY: {packet.PlayerExecutingAction}");
        Debug.Log("------------------------------------\n");
    }
}
