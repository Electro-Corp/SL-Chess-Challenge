using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class nowhere : IChessBot
{
    public static int searchCap = 0;
    private int searchDepth = 50;
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    public Move initMove(Board b)
    {
        //Console.WriteLine("This is how i got the board fr fr: ");
        //Console.WriteLine(b.CreateDiagram());
        bool isWhite = b.IsWhiteToMove;
        Move m = new Move();
        Move[] moves = b.GetLegalMoves();

        Move bestCapture = new Move();
        foreach (Move move in moves)
        {
            // Test checkmate
            b.MakeMove(move);
            if (b.IsInCheckmate())
            {
                b.UndoMove(move);
                return move;
            }
            b.UndoMove(move);
            if (move.CapturePieceType != PieceType.None)
            {
                if (pieceValues[(int)move.CapturePieceType] >= pieceValues[(int)bestCapture.CapturePieceType])
                {
                    Console.WriteLine("we captuin??");
                    // do some tests
                    bool bad = false;
                    b.MakeMove(move);
                    var values = Enum.GetValues(typeof(PieceType));
                    foreach (PieceType pieceType in values)
                    {
                        PieceList pList = b.GetPieceList(pieceType, !isWhite); // wow
                        if (pList != null)
                        {
                            foreach (Piece p in pList)
                            {
                                ulong checkBits = 0;
                                if (pieceType == PieceType.Bishop)
                                    checkBits = BitboardHelper.GetSliderAttacks(pieceType, p.Square, b);
                                if (pieceType == PieceType.Knight)
                                    checkBits = BitboardHelper.GetKnightAttacks(p.Square);
                                if (pieceType == PieceType.Pawn)
                                    checkBits = BitboardHelper.GetPawnAttacks(p.Square, !isWhite);
                                //ulong goof = b.GetPieceBitboard(move.MovePieceType, isWhite);
                                //int tarGet = move.TargetSquare.Index;

                                //ulong wack = goof & checkBits;
                                if (BitboardHelper.SquareIsSet(checkBits, move.TargetSquare) && move.CapturePieceType < move.MovePieceType)
                                {
                                    Console.WriteLine("Oof, i woul dhave been killed there by: " + p.ToString());
                                    bad = true;
                                }

                            }
                        }
                    }
                    b.UndoMove(move);
                    if (!bad)
                        bestCapture = move;
                    
                }
            }
           
            
        }
        m = bestCapture;

        // no moves (?)
        if (m.IsNull)
        {
            Random rnd = new Random();
            m = moves[rnd.Next(0, moves.Length - 1)];
            /*b.MakeMove(m);
            int checkTimes = 0;
            while (b.IsInStalemate() || b.IsFiftyMoveDraw() || b.IsInsufficientMaterial() || b.IsRepeatedPosition())
            {
                b.UndoMove(m);
                m = moves[rnd.Next(0, moves.Length - 1)];
                b.MakeMove(m);
                Console.WriteLine("oops checkin");
                checkTimes++;
                if (checkTimes > moves.Length)
                {
                    Console.WriteLine("Ran out of moves. All of them were bad.\nThere goes the game...");
                    b.UndoMove(m);
                    break;
                }
            }
            if(checkTimes == 0) b.UndoMove(m);*/
        }
        else
        {
            searchCap += 1;
        }
        // 
       // Console.WriteLine("This is the board after im done with it fr fr: ");
       // Console.WriteLine(b.CreateDiagram());
        return m;
    }

    public Move Think(Board b, Timer t)
    {
        Move m = new Move();
        int prevTake = -1;
        // Steps:
        // Choose an initial move, and see how many we take from there
        Move[] moves = b.GetLegalMoves();
        foreach(Move move in moves)
        {
            
            List<Move> rollBack = new List<Move>();
            b.MakeMove(move);
            rollBack.Add(move);
            b.ForceSkipTurn();
            for(int i = 0; i < searchDepth; i++)
            {
                Console.WriteLine("======================");
                Console.WriteLine("This is search " + i);
                Console.WriteLine("======================");
                Move tmp = new Move(); 
               
                tmp = initMove(b);
               // Console.WriteLine(b.CreateDiagram());
                b.MakeMove(tmp);
                b.ForceSkipTurn();
                rollBack.Add(tmp);
                //Console.ReadKey();
            }
            Console.WriteLine("======================");
            Console.WriteLine("======================");
            Console.WriteLine("That move is done.");
            Console.WriteLine("Moves to rollback: " + rollBack.Count);
            Console.WriteLine("======================");
            Console.WriteLine("======================");
            if (searchCap > prevTake)
            {
                prevTake = searchCap;
                Console.WriteLine("Peicers i took: "+ searchCap);
                //Console.ReadKey();
                m = move;
                searchCap = 0;
            }
            if (move.CapturePieceType != PieceType.None && move.CapturePieceType >= move.MovePieceType)
            {
                m = move;
            }
            for (int j = rollBack.Count -1 ; j >= 0; j--)
            {
                b.UndoSkipTurn();
                b.UndoMove(rollBack[j]);
                
            }
        }
        return m;
    }
}