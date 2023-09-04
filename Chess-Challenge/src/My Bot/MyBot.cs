﻿using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public class MyBot : IChessBot
{
    public Move Think(Board b, Timer t)
    {
        bool isWhite = b.IsWhiteToMove;
        Move m = new Move();
        Move[] moves = b.GetLegalMoves();
        Move bestCapture = new Move();
        foreach (Move move in moves)
        {
            if (move.CapturePieceType != PieceType.None)
            {
                if (move.CapturePieceType > bestCapture.CapturePieceType)
                {
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
                                ulong goof = b.GetPieceBitboard(move.MovePieceType, isWhite);
                                int tarGet = move.TargetSquare.Index;

                                //ulong wack = goof & checkBits;
                                if (BitboardHelper.SquareIsSet(checkBits, move.TargetSquare) && move.CapturePieceType <= move.MovePieceType)
                                {
                                    Console.WriteLine("Oof, i woul dhave been killed there by: " + p.ToString());
                                    bad = true;
                                }

                            }
                        }
                    }

                    if (!bad)
                        bestCapture = move;
                    b.UndoMove(move);
                }
            }
            // Test checkmate
            b.MakeMove(move);
            if (b.IsInCheckmate())
            {
                return move;
            }
            b.UndoMove(move);
        }
        m = bestCapture;

        if (m.IsNull)
        {
            Random rnd = new Random();
            m = moves[rnd.Next(0, moves.Length - 1)];
            b.MakeMove(m);
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
                    break;
                }
            }
            if(b.FiftyMoveCounter > 40)
            {
                Console.WriteLine("oof, fifty.");
                // Check to see if we have a pawn 
                if(b.GetPieceList(PieceType.Pawn, isWhite).Count > 0)
                {
                    foreach(Move mo in moves)
                    {
                        if(mo.MovePieceType == PieceType.Pawn)
                        {
                            Console.WriteLine("SAVED!");
                            m = mo;
                        }
                    }
                }
            }
        }

        return m;
    }
}