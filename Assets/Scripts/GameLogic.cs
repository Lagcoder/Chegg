using System;
using System.Collections.Generic;
using System.Linq;
using GameData;
using UnityEngine; 

namespace GameLogic
{
    public class GameEngine : MonoBehaviour
    {
        public const int BOARD_WIDTH = 10;
        public const int BOARD_HEIGHT = 8;
        public static readonly Vector2Int INVALID_VECTOR = new Vector2Int(-1, -1); // Corrected Y to be -1

        private Tile[,] Board;
        public PlayerData PlayerWhite { get; private set; } 
        public PlayerData PlayerBlack { get; private set; } 
        public PlayerColor CurrentTurn { get; private set; } 
        public List<Vector2Int> AvailableMoves { get; private set; } = new List<Vector2Int>(); 
        public List<Vector2Int> AvailableAttacks { get; private set; } = new List<Vector2Int>(); 

        public Vector2Int SelectedPosition { get; private set; } = INVALID_VECTOR; 
        
        // Helper property for getting the full PlayerData object for the current turn.
        public PlayerData CurrentPlayer => (CurrentTurn == PlayerColor.WHITE) ? PlayerWhite : PlayerBlack;
        public Piece SelectedPiece => GetPieceAt(SelectedPosition); 

        public GameEngine()
        {
            InitializeBoard();
            InitializePieces();
            CurrentTurn = PlayerColor.WHITE; 
        }

        private void InitializeBoard()
        {
            Board = new Tile[BOARD_HEIGHT, BOARD_WIDTH];
            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                for (int x = 0; x < BOARD_WIDTH; x++)
                {
                    Board[y, x] = new Tile { IsOccupied = false, TerrainType = TerrainType.PLAIN };
                }
            }
            // Add any non-PLAIN terrain here, e.g.:
            // Board[0, 0].TerrainType = TerrainType.WATER;
        }

        private void InitializePieces()
        {
            PlayerWhite = new PlayerData(PlayerColor.WHITE); 
            PlayerBlack = new PlayerData(PlayerColor.BLACK); 

            // Initialize pieces for both players
            PlacePiece(new Villager(new Vector2Int(4, 7), PlayerWhite), PlayerWhite); 
            PlacePiece(new Villager(new Vector2Int(4, 0), PlayerBlack), PlayerBlack); 
            PlacePiece(new Zombie(new Vector2Int(3, 7), PlayerWhite), PlayerWhite); 
            PlacePiece(new Zombie(new Vector2Int(5, 0), PlayerBlack), PlayerBlack); 
        }

        // --- CORE ACTION HANDLERS (DISPATCHER) ---

        // The public method that GameServer calls.
        public bool HandleSquareClick(Vector2Int target, GameData.MinionData? cardToSummon)
        {
            // If card data is provided, route to the summoning handler
            if (cardToSummon.HasValue)
            {
                return HandleSummon(target, cardToSummon.Value);
            }
            else
            {
                // Otherwise, route to the piece movement/selection handler
                return HandlePieceAction(target);
            }
        }

        // --- DEDICATED ACTION LOGIC ---

        public bool HandlePieceAction(Vector2Int target) 
        {
            // If a piece is currently selected:
            if (SelectedPosition != INVALID_VECTOR)
            {
                var selectedPiece = GetPieceAt(SelectedPosition);
                
                if (selectedPiece == null) 
                {
                    DeselectPiece(); 
                    return false;
                }

                // Determine Action Type
                var targetPiece = GetPieceAt(target);
                var validMoves = selectedPiece.GetValidAttacks(this); // Assumes this includes capture spots

                // A. CAPTURE: Target is an opponent's piece and is a valid move
                if (targetPiece != null && targetPiece.player != CurrentPlayer && validMoves.Any(move => move == target))
                {
                    bool success = ExecuteCapture(selectedPiece, target);
                    DeselectPiece();
                    return success;
                }
                
                // B. MOVE: Target is an empty spot and is a valid move
                if (targetPiece == null && validMoves.Any(move => move == target))
                {
                    ExecuteMove(selectedPiece, target);
                    DeselectPiece();
                    return true;
                }
                
                // C. DESELECT / RESELECT: Clicked on a friendly piece or an invalid spot
                DeselectPiece();
            }
            
            // --- Selection Check ---
            if (IsOwnedByCurrentPlayer(target))
            {
                SelectPiece(target);
                return true;
            }

            return false; // Invalid click/action.
        } 
        
        public bool HandleSummon(Vector2Int target, GameData.MinionData cardToSummon)
        {
            // 1. Check if the target tile is valid (e.g., empty, not water)
            if (GetPieceAt(target) != null || GetTileAt(target).TerrainType == GameData.TerrainType.WATER) 
            {
                Console.WriteLine($"Summon Failed: Tile {target} is occupied or invalid terrain.");
                return false; 
            }
            
            // 2. Check Mana cost
            if (CurrentPlayer.Mana < cardToSummon.Cost)
            {
                Console.WriteLine($"Summon Failed: Not enough mana. Needed: {cardToSummon.Cost}, Have: {CurrentPlayer.Mana}.");
                return false; 
            }

            // 3. Create the piece instance, passing the CurrentPlayer
            Piece newPiece;
            switch (cardToSummon.Type)
            {
                case GameData.MinionType.VILLAGER:
                    newPiece = new Villager(target, CurrentPlayer);
                    break;
                case GameData.MinionType.ZOMBIE:
                    newPiece = new Zombie(target, CurrentPlayer);
                    break;
                case GameData.MinionType.TRADER:
                    Console.WriteLine($"Summon Failed: {cardToSummon.Type} implementation missing.");
                    return false;
                default:
                    Console.WriteLine($"Summon Failed: Unknown MinionType {cardToSummon.Type}.");
                    return false;
            }

            // 4. Subtract Mana and remove card from hand
            CurrentPlayer.Mana -= cardToSummon.Cost;
            CurrentPlayer.Hand.RemoveAll(c => c.Type == cardToSummon.Type); 

            // 5. Place the piece (updates board tile and player list)
            PlacePiece(newPiece, CurrentPlayer);

            Console.WriteLine($"[SERVER] Player {CurrentPlayer.Color} successfully summoned a {cardToSummon.Type} at {target}.");
            
            return true;
        }

        // --- EXECUTION METHODS (CHESS-LIKE) ---
        
        public bool ExecuteMove(Piece pieceToMove, Vector2Int targetPosition)
        {
            // 1. Update the old tile
            Vector2Int oldPosition = pieceToMove.position;
            Board[oldPosition.y, oldPosition.x].IsOccupied = false;

            // 2. Update the piece's internal position and the new tile
            pieceToMove.position = targetPosition;
            Board[targetPosition.y, targetPosition.x].IsOccupied = true;
            
            // 3. Update action status
            pieceToMove.CanAct = false; 
            
            Console.WriteLine($"[SERVER] {pieceToMove.type} moved from {oldPosition} to {targetPosition}.");
            return true;
        }
        
        public bool ExecuteCapture(Piece capturingPiece, Vector2Int targetPosition)
        {
            // 1. Identify and eliminate the captured piece
            Piece capturedPiece = GetPieceAt(targetPosition);
            EliminatePiece(capturedPiece); 

            // 2. Update the old tile status
            Vector2Int oldPosition = capturingPiece.position;
            Board[oldPosition.y, oldPosition.x].IsOccupied = false;

            // 3. Move the capturing piece to the new position
            capturingPiece.position = targetPosition;
            
            // 4. Update the new tile status
            Board[targetPosition.y, targetPosition.x].IsOccupied = true;
            
            // 5. Update action status
            capturingPiece.CanAct = false; 

            Console.WriteLine($"[SERVER] {capturingPiece.type} captured {capturedPiece.type} at {targetPosition}.");

            return true;
        }

        // --- HELPER METHODS ---

        public void EliminatePiece(Piece piece)
        {
            var owner = piece.player.Color == PlayerColor.WHITE ? PlayerWhite : PlayerBlack;
            if (owner.Pieces.Remove(piece))
            {
                Board[piece.position.y, piece.position.x].IsOccupied = false; 
            }
        }
        
        public void EndTurn()
        {
            DeselectPiece();
            CurrentTurn = CurrentTurn == PlayerColor.WHITE ? PlayerColor.BLACK : PlayerColor.WHITE;
        }
        
        private void PlacePiece(Piece piece, PlayerData player)
        {
            if (IsOnBoard(piece.position)) 
            {
                Board[piece.position.y, piece.position.x].IsOccupied = true; 
                player.Pieces.Add(piece);
            }
        }
        
        public Piece GetPieceAt(Vector2Int pos) 
        {
            if (!IsOnBoard(pos))
            {
                return null;
            }

            var allPieces = PlayerWhite.Pieces.Concat(PlayerBlack.Pieces);
            return allPieces.FirstOrDefault(p => p.position == pos);
        }
        
        public Tile GetTileAt(Vector2Int pos)
        {
            if (!IsOnBoard(pos))
            {
                throw new IndexOutOfRangeException($"Position {pos} is off-board.");
            }
            // Array indexing is [row, column] which corresponds to [y, x]
            return Board[pos.y, pos.x];
        }

        public bool IsOnBoard(Vector2Int pos) 
        {
            return pos.x >= 0 && pos.x < BOARD_WIDTH && pos.y >= 0 && pos.y < BOARD_HEIGHT;
        }

        private void SelectPiece(Vector2Int pos) 
        {
            var piece = GetPieceAt(pos);
            if (piece != null && piece.player.Color == CurrentTurn) 
            {
                SelectedPosition = pos;
                AvailableMoves = piece.GetValidAttacks(this);
                AvailableAttacks.Clear(); 
            }
        }
        
        public void DeselectPiece()
        {
            SelectedPosition = INVALID_VECTOR;
            AvailableMoves.Clear();
            AvailableAttacks.Clear();
        }

        private bool IsOwnedByCurrentPlayer(Vector2Int pos) 
        {
            var piece = GetPieceAt(pos);
            return piece != null && piece.player.Color == CurrentTurn; 
        }

        private List<Piece> GetCurrentPlayerPieces() => CurrentTurn == PlayerColor.WHITE ? PlayerWhite.Pieces : PlayerBlack.Pieces;
        private List<Piece> GetOpponentPlayerPieces() => CurrentTurn == PlayerColor.WHITE ? PlayerBlack.Pieces : PlayerWhite.Pieces;
        
        public GameStatePacket GetFullStateSnapshot()
        {
            var pieceDataList = new List<BoardPieceData>();
            var allPieces = PlayerWhite.Pieces.Concat(PlayerBlack.Pieces);

            foreach (var piece in allPieces)
            {
                pieceDataList.Add(new BoardPieceData
                {
                    Position = piece.position,
                    Type = (MinionType)Enum.Parse(typeof(MinionType), piece.type, true),
                    Color = piece.player.Color,
                    // Health is included but should be fixed at a constant value for chess-like pieces
                    Health = piece.health, 
                    CanAct = true 
                });
            }

            return new GameStatePacket
            {
                PiecesOnBoard = pieceDataList,
                PlayerWhite = PlayerWhite,
                PlayerBlack = PlayerBlack,
                CurrentTurn = CurrentTurn,
                IsGameOver = false,
                SelectedPosition = this.SelectedPosition 
            };
        }
    }
}