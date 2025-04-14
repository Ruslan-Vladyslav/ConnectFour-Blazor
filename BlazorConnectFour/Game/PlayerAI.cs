using BlazorConnectFour.Game;

public class PlayerAI
{
    private GameState _state;
    public int AiDifficulty { get; set; } = 1;


    public PlayerAI(GameState gameState)
    {
        _state = gameState;
    }


    public string GetDifficulty()
    {
        return AiDifficulty switch
        {
            1 => "Easy",
            3 => "Moderate",
            5 => "Hard",
            _ => "Unknown"
        };
    }

    public byte GetBestMove(int depth)
    {
        var bestScore = int.MinValue;
        int alpha = int.MinValue;
        int beta = int.MaxValue;
        byte bestMove = 0;

        foreach (var col in _state.GetAvailableColumns(_state).OrderBy(c => Math.Abs(3 - c)))
        {
            var copyState = _state.Clone();

            if (!copyState.TryPlayPiece(col, out byte _))
                continue;

            int score = Minimax(copyState, depth - 1, false, alpha, beta);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = col;
            }
        }

        return bestMove;
    }

    private int Minimax(GameState state, int depth, bool isMaximizing, int alpha, int beta)
    {
        var result = state.CheckForWin();

        if (result == GameState.WinState.Player2_Wins) return 100000;
        if (result == GameState.WinState.Player1_Wins) return -100000;
        if (result == GameState.WinState.Tie || depth == 0) return EvaluateBoard(state);

        var availableMoves = state.GetAvailableColumns(state);

        if (isMaximizing)
        {
            int bestScore = int.MinValue;

            foreach (var move in availableMoves)
            {
                var clone = state.Clone();
                clone.TryPlayPiece(move, out byte _);

                int score = Minimax(clone, depth - 1, false, alpha, beta);

                bestScore = Math.Max(score, bestScore);
                alpha = Math.Max(alpha, bestScore);

                if (beta <= alpha) break;
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;

            foreach (var move in availableMoves)
            {
                var clone = state.Clone();
                clone.TryPlayPiece(move, out byte _);

                int score = Minimax(clone, depth - 1, true, alpha, beta);

                bestScore = Math.Min(score, bestScore);
                beta = Math.Min(beta, bestScore);

                if (beta <= alpha) break;
            }
            return bestScore;
        }
    }

    private int EvaluateBoard(GameState state)
    {
        int score = 0;

        foreach (var group in GameState.WinningPlaces)
        {
            var line = group.Select(pos => state.TheBoard[pos]).ToList();

            int player2Count = line.Count(p => p == 2);
            int player1Count = line.Count(p => p == 1);

            if (player2Count > 0)
            {
                if (player2Count == 3) score += 1000;
                else if (player2Count == 2) score += 100;
                else score += 10;
            }
            else if (player1Count > 0)
            {
                if (player1Count == 3) score -= 1200;
                else if (player1Count == 2) score -= 120;
                else score -= 10;
            }
        }

        return score;
    }
}
