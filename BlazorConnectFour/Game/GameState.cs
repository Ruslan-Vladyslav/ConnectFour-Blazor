﻿namespace BlazorConnectFour.Game;

public class GameState
{

    static GameState()
    {
        CalculateWinningPlaces();
    }

    /// <summary>
    /// Indicate whether a player has won, the game is a tie, or game in ongoing
    /// </summary>
    public enum WinState
    {
        No_Winner = 0,
        Player1_Wins = 1,
        Player2_Wins = 2,
        Tie = 3
    }


    public int startingPlayer = 1;

    public void ChangePlayerMove()
    {
        startingPlayer = (startingPlayer == 1) ? 2 : 1;
    }

    /// <summary>
    /// The player whose turn it is.  By default, player 1 starts first
    /// </summary>
    //public int PlayerTurn => TheBoard.Count(x => x != 0) % 2 + 1;
    public int PlayerTurn => TheBoard.Count(x => x != 0) % 2 == 0 ? startingPlayer : 3 - startingPlayer;

    /// <summary>
    /// Number of turns completed and pieces played so far in the game
    /// </summary>
    public int CurrentTurn { get { return TheBoard.Count(x => x != 0); } }

    public static readonly List<int[]> WinningPlaces = new();

    public static void CalculateWinningPlaces()
    {
        // Horizontal rows
        for (byte row = 0; row < 6; row++)
        {
            byte rowCol1 = (byte)(row * 7);
            byte rowColEnd = (byte)((row + 1) * 7 - 1);
            byte checkCol = rowCol1;
            while (checkCol <= rowColEnd - 3)
            {
                WinningPlaces.Add(new int[] {
                    checkCol,
                    (byte)(checkCol + 1),
                    (byte)(checkCol + 2),
                    (byte)(checkCol + 3)
                    });
                checkCol++;
            }
        }

        // Vertical Columns
        for (byte col = 0; col < 7; col++)
        {
            byte colRow1 = col;
            byte colRowEnd = (byte)(35 + col);
            byte checkRow = colRow1;

            while (checkRow <= 14 + col)
            {
                WinningPlaces.Add(new int[] {
                    checkRow,
                    (byte)(checkRow + 7),
                    (byte)(checkRow + 14),
                    (byte)(checkRow + 21)
                    });
                checkRow += 7;
            }
        }

        // forward slash diagonal "/"
        for (byte col = 0; col < 4; col++)
        {
            // starting column must be 0-3
            byte colRow1 = (byte)(21 + col);
            byte colRowEnd = (byte)(35 + col);
            byte checkPos = colRow1;
            while (checkPos <= colRowEnd)
            {
                WinningPlaces.Add(new int[] {
                    checkPos,
                    (byte)(checkPos - 6),
                    (byte)(checkPos - 12),
                    (byte)(checkPos - 18)
                    });
                checkPos += 7;
            }
        }

        // back slash diaganol "\"
        for (byte col = 0; col < 4; col++)
        {
            // starting column must be 0-3
            byte colRow1 = (byte)(0 + col);
            byte colRowEnd = (byte)(14 + col);
            byte checkPos = colRow1;
            while (checkPos <= colRowEnd)
            {
                WinningPlaces.Add(new int[] {
                    checkPos,
                    (byte)(checkPos + 8),
                    (byte)(checkPos + 16),
                    (byte)(checkPos + 24)
                    });
                checkPos += 7;
            }
        }
    }

    /// <summary>
    /// Check the state of the board for a winning scenario
    /// </summary>
    /// <returns>0 - no winner, 1 - player 1 wins, 2 - player 2 wins, 3 - draw</returns>
    public WinState CheckForWin()
    {
        // Exit immediately if less than 7 pieces are played
        if (TheBoard.Count(x => x != 0) < 7) return WinState.No_Winner;

        foreach (var scenario in WinningPlaces)
        {
            int player = TheBoard[scenario[0]];

            if (player != 0 &&
                TheBoard[scenario[1]] == player &&
                TheBoard[scenario[2]] == player &&
                TheBoard[scenario[3]] == player)
            {
                return (WinState)player;
            }
        }

        if (TheBoard.Count(x => x != 0) == 42) return WinState.Tie;

        return WinState.No_Winner;
    }

    /// <summary>
    /// Takes the current turn and places a piece in the 0-indexed column requested
    /// </summary>
    /// <param name="column">0-indexed column to place the piece into</param>
    /// <returns>The final array index where the piece resides</returns>
    public byte PlayPiece(int column)
    {
        // Check for a current win
        if (CheckForWin() != 0) throw new ArgumentException("Game is over");

        // Check the column
        if (TheBoard[column] != 0) throw new ArgumentException($"Column {column + 1} is full");

        // Drop the piece in
        var landingSpot = column;
        for (var i = column; i < 42; i += 7)
        {
            if (TheBoard[landingSpot + 7] != 0) break;
            landingSpot = i;
        }

        TheBoard[landingSpot] = PlayerTurn;

        return ConvertLandingSpotToRow(landingSpot);
    }

    public List<int> TheBoard { get; private set; } = Enumerable.Repeat(0, 42).ToList();

    public void ResetBoard()
    {
        TheBoard = new List<int>(new int[42]);
    }

    private byte ConvertLandingSpotToRow(int landingSpot)
    {
        return (byte)(Math.Floor(landingSpot / (decimal)7) + 1);
    }

    public GameState Clone()
    {
        return new GameState
        {
            TheBoard = new List<int>(TheBoard)
        };
    }

    public bool TryPlayPiece(int column, out byte landingRow)
    {
        landingRow = 0;

        if (CheckForWin() != WinState.No_Winner)
            return false;

        if (TheBoard[column] != 0)
            return false;

        var landingSpot = column;

        for (var i = column; i < 42; i += 7)
        {
            if (landingSpot + 7 >= 42 || TheBoard[landingSpot + 7] != 0)
                break;
            landingSpot = i + 7;
        }

        TheBoard[landingSpot] = PlayerTurn;
        landingRow = ConvertLandingSpotToRow(landingSpot);

        return true;
    }

    public int EvaluateBoard(GameState state)
    {
        int score = 0;

        foreach (var group in WinningPlaces)
        {
            var line = group.Select(pos => state.TheBoard[pos]).ToList();

            int player2Count = line.Count(p => p == 2);
            int player1Count = line.Count(p => p == 1);


            if (player2Count > 0 && player1Count == 0)
            {
                //score += (int)Math.Pow(10, player2Count);
                score += player2Count * 10;
            }
            else if (player1Count > 0 && player2Count == 0)
            {
                //score -= (int)Math.Pow(10, player1Count);
                score -= player1Count * 10;
            }
        }

        return score;
    }

    public List<byte> GetAvailableColumns(GameState state)
    {
        var freeColumn = new List<byte>();

        for (byte col = 0; col < 7; col++)
        {
            if (state.TheBoard[col] == 0)
            {
                freeColumn.Add(col);
            }
        }

        return freeColumn;
    }
}