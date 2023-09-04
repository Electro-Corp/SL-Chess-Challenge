using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        Move move = new Move();
        // Check all values of being captured
        Move bestCapture = new Move();
        foreach(Move m in moves)
        {
            if(m.CapturePieceType != PieceType.None)
            {
                if(m.CapturePieceType > bestCapture.CapturePieceType)
                {
                    bestCapture = m;
                }
            }
        }

        if (!bestCapture.IsNull)
        {
            move = bestCapture;
        }
        else
        {
            move = moves[0];
        }
        return move;
    }
}