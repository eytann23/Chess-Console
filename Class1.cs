using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ChessGame
{
    class Status
    {
        public static bool checkIfGameIsOver(bool isWhitesTurn,Board board, bool isWhiteInCheck, bool isBlackInCheck)
        {
            char playing = (isWhitesTurn) ? 'W' : 'B';
            if (!board.DoesTeamHaveMoveOptions(playing))
            {
                if ((playing == 'W') && (isWhiteInCheck == true))
                {
                    Console.WriteLine("Checkmate! Black is the WINNER!");
                    return true;   
                }
                else if ((playing == 'B') && (isBlackInCheck == true))
                {
                    Console.WriteLine("Checkmate! White is the WINNER!");
                    return true;  
                }
                else
                {
                    Console.WriteLine("Stalemate!");
                    Console.WriteLine("IT'S A TIE");
                    return true;
                }
            }
            return false;
        }

        public static bool checkIfBlackIsInCheck(Board board)
        {
            if (board.IsCheck(false))
            {
                Console.WriteLine("CHECK!");
                return true;
            }
            else
                return false;
        }
        public static bool checkIfWhiteIsInCheck(Board board)
        {
            if (board.IsCheck(true))
            {
                Console.WriteLine("CHECK!");
                return true;
            }
            else
                return false;
        }

    }


    class Move
    {
        Location from, to;

        public void SetMove(Location from, Location to)
        {
            this.from = from;
            this.to = to;
        }
        
        public Location GetUserFrom()
        {
            return from;
        }

        public Location GetUserTo()
        {
            return to;
        }

    }
      

}
