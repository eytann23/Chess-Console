using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace ChessGame
{

    class ChessGameLauncher
    {
        static void Main(string[] args)
        {
            new ChessGame().play();
        }
    }

    class ChessGame
    {
        public void play()
        {
            Board board = new Board();

            bool isWhitesTurn = true;
            bool isBlackInCheck = false, isWhiteInCheck = false;
            bool gameOver = false;
            Location userFrom, userTo;
            
            Move nextMove = new Move();

            board.refreshLocations();
            board.refreshHistories();
            board.updatePiecesAlive();
            board.refreshPossibleMoves();
            board.refreshPossibleMovesTeam();

            board.printBoard();

            while (!gameOver)
            {
                char playingNowColor = isWhitesTurn ? 'W' : 'B';
                Console.WriteLine();

                if (Status.checkIfGameIsOver(isWhitesTurn, board, isWhiteInCheck, isBlackInCheck))
                    break;

                nextMove = Player.GetNextMoveFromUser(isWhitesTurn, board);
                userFrom = nextMove.GetUserFrom();
                userTo = nextMove.GetUserTo();

                board.refreshPossibleMoves();
                board.refreshPossibleMovesTeam();

                //save the "to" piece in case of executing "undo"
                Piece lastPieceBeated=null;
                if (board.getPiece(userFrom) != null)
                    lastPieceBeated = board.getPiece(userTo);


                //Make move if it's legal
                if (!board.makeMove(userFrom, userTo))
                {
                    Console.WriteLine("Invalid move!");
                    continue;
                }

                else
                {
                    board.refreshPossibleMoves();
                    board.refreshPossibleMovesTeam();

                    if (!board.IsCheck(isWhitesTurn))
                    {
                        isWhitesTurn = !isWhitesTurn;
                        board.RoundsCountUpdate();
                        board.printBoard();

                        if (board.checkPromotion(isWhitesTurn))
                        {
                            board.refreshLocations();
                            board.refreshHistories();
                            board.updatePiecesAlive();
                            board.refreshPossibleMoves();
                            board.refreshPossibleMovesTeam();
                            board.printBoard();
                        }

                        //if there's CHECK - declare and change check status
                        isBlackInCheck=Status.checkIfBlackIsInCheck(board);
                        isWhiteInCheck=Status.checkIfWhiteIsInCheck(board);

                        board.refreshLocations();
                        board.refreshHistories();
                        board.updatePiecesAlive();

                        gameOver = board.IsItPossibleToExecuteMate();
                        gameOver = board.DidFiftyRoundsOccurWithoutBeating();
                        gameOver = board.DoesSamePlacingRepeatedThreeTimes();
                    }

                    else
                    {
                        board.Undo(userFrom, userTo, lastPieceBeated);

                        board.refreshPossibleMoves();
                        board.refreshPossibleMovesTeam();

                        Console.WriteLine("Invalid move! King can't be in a check position.");
                    }
                }
            }
        }
    }

 
    class History
    {
        Location location;
        int movesCounter;

        public History(int movesCounter)
        {
            this.movesCounter = movesCounter;
            this.location = new Location();
        }

        public void setLocation(int row,int column)
        {
            this.location.setLocation(row, column);
        }

        public Location getLocation()
        {
            return location;
        }

        public int getMovesCounter()
        {
            return movesCounter;
        }

        public override string ToString()
        {
            return "Location: " + location + " | Moves Counter: " + movesCounter;
        }
    }

    class Location
    {
        int row, column;

        public int getRow()
        {
            return row;
        }
        public int getColumn()
        {
            return column;
        }
        public void setLocation(int row, int column)
        {
            this.row = row;
            this.column = column;
        }
        public void setLocation(Location location)
        {
            this.row= location.getRow();
            this.column= location.getColumn();
        }
        public static Location convertStringToLocation(string location)
        {
            Location l = new Location();
            int row, column = 10;
            row = int.Parse("" + location[1]) - 1;
            switch (location[0])
            {
                case 'A':
                case 'a':
                    column = 0;
                    break;
                case 'B':
                case 'b':
                    column = 1;
                    break;
                case 'C':
                case 'c':
                    column = 2;
                    break;
                case 'D':
                case 'd':
                    column = 3;
                    break;
                case 'E':
                case 'e':
                    column = 4;
                    break;
                case 'F':
                case 'f':
                    column = 5;
                    break;
                case 'G':
                case 'g':
                    column = 6;
                    break;
                case 'H':
                case 'h':
                    column = 7;
                    break;
            }
            l.setLocation(row, column);
            return l;
        }

        public int[] ConvertLocationToArray()
        {
            int[] arrayLocation = new int[2];
            arrayLocation[0] = getRow();
            arrayLocation[1] = getColumn();
            return arrayLocation;
        }

        public override string ToString()
        {
            return "Row: " + row + " | Column: " + column;
        }
        public override bool Equals(object obj)
        {
            Location other = (Location)obj;
            return ((this.row == other.row) && (this.column == other.column));
        }
    }

    class Player
    {
        //check user input
        public static string checkInput(string message)
        {
            bool valid = false;
            string input = "";
            while (!valid)
            {
                valid = false;
                Console.WriteLine(message);
                input = Console.ReadLine();

                input = input.Trim();
                if (input.Length != 2)
                {
                    Console.WriteLine("Invalid place, choose again");
                    continue;
                }

                switch (input[0])
                {
                    case 'A':
                    case 'a':
                    case 'B':
                    case 'b':
                    case 'C':
                    case 'c':
                    case 'D':
                    case 'd':
                    case 'E':
                    case 'e':
                    case 'F':
                    case 'f':
                    case 'G':
                    case 'g':
                    case 'H':
                    case 'h':
                        valid = true;
                        break;

                    default:
                        Console.WriteLine("Invalid place, choose agian");
                        continue;
                }

                switch (input[1])
                {
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                        valid = true;
                        break;
                    default:
                        Console.WriteLine("Invalid place, choose agian");
                        continue;
                }
            }
            return input;

        }

        //convert location --> get location from user as string (A1) and return as int array {0,0}
        public static int[] convertLocation(string location)
        {
            int row, column = 10;
            row = int.Parse("" + location[1]) - 1;
            switch (location[0])
            {
                case 'A':
                case 'a':
                    column = 0;
                    break;
                case 'B':
                case 'b':
                    column = 1;
                    break;
                case 'C':
                case 'c':
                    column = 2;
                    break;
                case 'D':
                case 'd':
                    column = 3;
                    break;
                case 'E':
                case 'e':
                    column = 4;
                    break;
                case 'F':
                case 'f':
                    column = 5;
                    break;
                case 'G':
                case 'g':
                    column = 6;
                    break;
                case 'H':
                case 'h':
                    column = 7;
                    break;

            }

            return new int[2] { row, column };
        }

        public static Move GetNextMoveFromUser(bool isWhitesTurn, Board board)//if return null, in play() continue
        {
            Move nextMove = new Move();
            Location userFrom, userTo;
            char playingColor = (isWhitesTurn) ? 'W' : 'B';
            string userInput;

            Console.WriteLine(isWhitesTurn ? "WHITE PLAYER ENTER YOUR MOVE:" : "BLACK PLAYER ENTER YOUR MOVE:");

            //from
            userInput = Player.checkInput("From: ");
            userFrom = Location.convertStringToLocation(userInput);
            if (board.getPiece(userFrom) == null)
            {
                Console.WriteLine("This place is empty!");
                return null;
            }

            if (board.getPiece(userFrom).getColor() != playingColor)
            {
                Console.WriteLine("This one isn't your color!");
                return null;
            }

            //to
            userInput = Player.checkInput("To: ");
            userTo = Location.convertStringToLocation(userInput);

            nextMove.SetMove(userFrom, userTo);
            return nextMove;
        }
    }

    class Board
    {
        Piece[,] chessBoard;
        int roundsCounter;
        Location[] possibleMovesBlack;
        Location[] possibleMovesWhite;
        int blackPiecesAlive;
        int whitePiecesAlive;
        int[] blackPiecesAliveHistory;
        int[] whitePiecesAliveHistory;
        
        public Board()
        {
            roundsCounter = 0;
            possibleMovesBlack = new Location[64];
            possibleMovesWhite = new Location[64];
            blackPiecesAlive=16;
            whitePiecesAlive=16;
            blackPiecesAliveHistory = new int[200];
            whitePiecesAliveHistory = new int[200];

            chessBoard = new Piece[8, 8];
            
            //White Team
            chessBoard[0, 0] = new Rook('W');
            chessBoard[0, 7] = new Rook('W');
            chessBoard[0, 1] = new Knight('W');
            chessBoard[0, 6] = new Knight('W');
            chessBoard[0, 2] = new Bishop('W');
            chessBoard[0, 5] = new Bishop('W');
            chessBoard[0, 3] = new Queen('W');
            chessBoard[0, 4] = new King('W');
            for (int i = 0; i < 8; i++)
                chessBoard[1, i] = new Pawn('W');

            //Black Team
            chessBoard[7, 0] = new Rook('B');
            chessBoard[7, 7] = new Rook('B');
            chessBoard[7, 1] = new Knight('B');
            chessBoard[7, 6] = new Knight('B');
            chessBoard[7, 2] = new Bishop('B');
            chessBoard[7, 5] = new Bishop('B');
            chessBoard[7, 3] = new Queen('B');
            chessBoard[7, 4] = new King('B');
            for (int i = 0; i < 8; i++)
                chessBoard[6, i] = new Pawn('B');
        }


        //check if there is at least 1 legal movement
        bool doesPieceHasValidMoveOption(Piece piece, char playingColor)
        {
            bool isWhitesTurn = (playingColor == 'W') ? true : false;

            //save the "to" piece
            Location currentLocation=new Location();
            
            currentLocation.setLocation(piece.getLocation().getRow(), piece.getLocation().getColumn());
            Piece pieceUndo;

            if (piece.getPossibleMovesAsLocations() == null)
                return false;

            foreach (Location l in piece.getPossibleMovesAsLocations())
            {
                if (l == null)
                    continue;
                pieceUndo = null;
                if (getPiece(l) != null)
                    pieceUndo = getPiece(l);

                setPiece(piece, l);
                setPiece(null, currentLocation);
                
                //Refresh possible moves
                refreshPossibleMoves();
                //Refresh possible moves - both of the teams
                refreshPossibleMovesTeam();

                if (!IsCheck(isWhitesTurn))
                {
                    Undo(currentLocation, l, pieceUndo);
                    return true;
                }

                Undo(currentLocation, l, pieceUndo);
            }
            return false;
        }

        //doesPieceHasValidMoveOption - All pieces
        public bool DoesTeamHaveMoveOptions(char playingColor)
        {
            Location location = new Location();
            //King
            for (int i=0;i<8;i++)
                for (int j=0;j<8;j++)
                {
                    location.setLocation(i, j);
                    //find the King
                    if ((getPiece(location) is King) && (getPiece(location).getColor() == playingColor))
                            if (doesPieceHasValidMoveOption(getPiece(location), playingColor))
                                return true; //if find at least ONE option
                }

            //Rest of the team - if didn't return true until now
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    location.setLocation(i, j);
                    //find next piece of the team
                    if (getPiece(location)!=null)
                        if  (getPiece(location).getColor() == playingColor)
                            if (doesPieceHasValidMoveOption(getPiece(location), playingColor))
                                return true; //if find at least ONE option
                }

            return false; //means = Checkmate or Stalemate
        }

        //check if only king vs king OR king+bishop/knight
        public bool IsItPossibleToExecuteMate()
        {
            if (getWhitePiecesAlive() + getBlackPiecesAlive() == 2)
            {
                Console.WriteLine("Both sides don't have the means to win");
                Console.WriteLine("IT'S A TIE!");
                return true;
            }
                
            if (getWhitePiecesAlive()+getBlackPiecesAlive()==3)
            {
                Location location = new Location();
                for (int i=0;i<8;i++)
                    for (int j = 0; j < 8; j++)
                    {
                        location.setLocation(i, j);
                        if (getPiece(location) != null)
                            if ((getPiece(location) is Knight) || (getPiece(location) is Bishop))
                            {
                                Console.WriteLine("Both sides don't have the means to win");
                                Console.WriteLine("IT'S A TIE!");
                                return true;
                            }
                    }
            }
            return false;
        }

        public bool DoesSamePlacingRepeatedThreeTimes()
        {
            Location location = new Location();
            int countSamePlacing = 0;
            
            //searching for the first piece
            location = findTheFirstPiece();
            
            //r --> round
            for (int r=getMovesCounter()-2;r>=0;r-=2)
            {
                //if location in history is Equal to current location
                if (getPiece(location).getHistory(r).getLocation().Equals(location))
                {
                    //check if the pieces amount now and then didn't change
                    if ((getBlackPiecesAliveHistory(r)!=getBlackPiecesAlive())
                        ||(getWhitePiecesAliveHistory(r) != getWhitePiecesAlive()))
                        return false;

                    //check all the board in this r
                    if (IsCurrentPlacingSimilarToPreviousPlacing(r))
                        countSamePlacing++;
                    
                    if (countSamePlacing == 2)
                        break;
                }   
            }

            if (countSamePlacing == 2)
            {
                Console.WriteLine("Three times same placing!");
                Console.WriteLine("IT'S A TIE");
                return true;
            }
            else
                return false;
        }

        //searching for the first piece in the board
        Location findTheFirstPiece()
        {
            Location location = new Location();

            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    location.setLocation(i, j);
                    //searching for the first piece in the board
                    if (getPiece(location) != null)
                        return location;
                }
            return location;
        }

        bool IsCurrentPlacingSimilarToPreviousPlacing (int round)
        {
            Location location = new Location();
            
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    location.setLocation(i, j);
                    
                    if (getPiece(location) != null)
                        if(!getPiece(location).IsCurrentLocationSimilarToPreviousLocation(round))
                            return false;
                }

            return true;
        }

        //check 50 rounds without beating
        public bool DidFiftyRoundsOccurWithoutBeating()
        {
            if ((roundsCounter - 50) < 0)
                return false;
            int blackNow = getBlackPiecesAlive();
            int whiteNow = getWhitePiecesAlive();
            int blackFifty = getBlackPiecesAliveHistory(roundsCounter - 50);
            int whiteFifty = getWhitePiecesAliveHistory(roundsCounter - 50);

            if (blackNow + whiteNow == blackFifty + whiteFifty)
            {
                Console.WriteLine("50 rounds without beating!");
                Console.WriteLine("IT'S A TIE");
                return true;
            }

            return false;
        }

        //Check
        public bool IsCheck (bool isWhitesTurn)
        {
            Location location=new Location();
            char playingColor = (isWhitesTurn) ? 'W' : 'B';
            char opponentColor = (isWhitesTurn) ? 'B' : 'W';
            
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    location.setLocation(i, j);
                    if (getPiece(location) != null)
                    {
                        //where is the king
                        if ((getPiece(location) is King) && (getPiece(location).getColor() == playingColor))
                        {
                            foreach (Location l in getPossibleMovesTeam(opponentColor))
                                if (l != null)
                                    if (location.Equals(l)) //check if King location is in the "PossibleMovesTeam" of the opponent
                                        return true;
                        }
                    }
                }
            return false;
        }

        public void Undo(Location userFrom, Location userTo, Piece lastPieceBeated)
        {
            //put To in From
            setPiece(getPiece(userTo), userFrom);
            //put null or pieceUndo in To
            setPiece(lastPieceBeated, userTo);

            if (getPiece(userFrom) is Pawn)
                getPiece(userFrom).setLastMoveCounter(0);

            refreshPossibleMoves();
            refreshPossibleMovesTeam();
        }
        public int getBlackPiecesAlive()
        {
            return blackPiecesAlive;
        }
        public int getWhitePiecesAlive()
        {
            return whitePiecesAlive;
        }
        public void updatePiecesAlive()
        {
            Location location = new Location();
            int blackPieces = 0, whitePieces = 0;
            for (int i=0;i<8;i++)
                for (int j=0;j<8;j++)
                {
                    location.setLocation(i, j);
                    if (getPiece(location)!=null)
                    {
                        if (getPiece(location).getColor() == 'W')
                            whitePieces++;
                        else
                            blackPieces++;
                    }
                }

            blackPiecesAlive = blackPieces;
            whitePiecesAlive = whitePieces;
            //add to history
            blackPiecesAliveHistory[roundsCounter] = blackPieces;
            whitePiecesAliveHistory[roundsCounter] = whitePieces;
        }
        public int getBlackPiecesAliveHistory(int round)
        {
            return blackPiecesAliveHistory[round];
        }

        public int getWhitePiecesAliveHistory(int round)
        {
            return whitePiecesAliveHistory[round];
        }

        public void RoundsCountUpdate()
        {
            roundsCounter++;
        }

        public int getMovesCounter()
        {
            return roundsCounter;
        }

        public Piece getPiece(int[] location)
        {
            return chessBoard[location[0], location[1]];
        }

        public Piece getPiece(Location location)
        {
            if (location == null)
                return null;
            return chessBoard[location.getRow(), location.getColumn()];
        }

        void setPiece(Piece p, int[] location)
        {
            chessBoard[location[0], location[1]] = p;
        }

        void setPiece(Piece p, Location location)
        {
            
            chessBoard[location.getRow(), location.getColumn()] = p;
        }
        //get location and check if this place is empty
        public bool isEmpty(int[] location)
        {
            if (location[0] < 0 || location[0] > 7 || location[1] < 0 || location[1] > 7)
                return false;
            if (chessBoard[location[0], location[1]] == null)
                return true;
            else
                return false;
        }
        //get location and check if this place is containing an opponent piece
        public bool isOpponent(int[] location, char playingSide)
        {
            if (location[0] < 0 || location[0] > 7 || location[1] < 0 || location[1] > 7)
                return false;
            if (chessBoard[location[0], location[1]] != null)
                if (chessBoard[location[0], location[1]].getColor() != playingSide)
                    return true;

            return false;
        }

        //check if "to" location is in the possible moves of "from"
        public bool makeMove(Location userFrom,Location userTo)
        {
            int[] from = userFrom.ConvertLocationToArray();
            int[] to = userTo.ConvertLocationToArray();
            //location in case of inPassing
            int[] location = new int[2];
            string[] moves = chessBoard[from[0], from[1]].getPossibleMoves();

            //string of "to" just for comparing to possible moves string array
            string stringOfTo = "" + to[0] + to[1];
            if (moves == null)
                return false;
            foreach (string s in moves)
            {
                if (s == "")//just for now
                    continue;

                //if to is in possible moves
                if (s[0] == stringOfTo[0] && s[1] == stringOfTo[1])
                {
                    getPiece(from).updateMoves();

                    //inPassing
                    //if moving with Pawn && moving to empty place && not in the same column
                    if ((chessBoard[from[0], from[1]] is Pawn) && isEmpty(to) && from[1] != to[1])
                    {
                        location[0] = (chessBoard[from[0], from[1]].getColor() == 'W') ? (to[0] - 1) : (to[0] + 1);
                        location[1] = to[1];
                        setPiece(null, location);
                    }

                    //Castling
                    //if the King is steping 2 steps, move the rook to his new place
                    if (getPiece(from) is King)
                        if ((from[1] - to[1] == 2) || (from[1] - to[1] == -2))
                        {
                            int[] rookPlace;
                            int[] rookNewPlace;
                            //left
                            if ((from[1] - to[1]) == 2)
                            {
                                rookPlace = new int[2] { from[0], 0 };
                                rookNewPlace = new int[2] { from[0], 3 };
                            }
                            //right
                            else
                            {
                                rookPlace = new int[2] { from[0], 7 };
                                rookNewPlace = new int[2] { from[0], 5 };
                            }

                            setPiece(getPiece(rookPlace), rookNewPlace);
                            setPiece(null, rookPlace);
                        }

                    setPiece(getPiece(from), to);   
                    setPiece(null, from);
                    getPiece(to).setLastMoveCounter(roundsCounter);

                    return true;
                }
            }
            return false;
        }

        //check if one of the pawns arrived to the last row
        public bool checkPromotion(bool isWhitesTurn)
        {
            string input;
            int[] nextCheck = new int[2];
            bool valid = false;
            char playingColor = (isWhitesTurn) ? 'W' : 'B';
            Location location = new Location();
            for (int i = 0; i < 8; i++)
            {
                if ((getPiece(new int[] { 0, i }) is Pawn) || (getPiece(new int[] { 7, i }) is Pawn))
                {
                    nextCheck[0] = (playingColor == 'W') ? (7) : (0);
                    nextCheck[1] = i;
                    while (!valid)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Which piece would you like to have instead of the pawn?");
                        Console.WriteLine("Q - QUEEN \nR - ROOK \nB - BISHOP \nN - KNIGHT");
                        Console.WriteLine("Please ENTER your choice: ");
                        input = Console.ReadLine();
                        input = input.Trim();
                        if (input.Length == 1)
                        {
                            switch (input)
                            {
                                case "Q":
                                case "q":
                                    setPiece(new Queen(playingColor), nextCheck);
                                    valid = true;
                                    break;
                                case "R":
                                case "r":
                                    setPiece(new Rook(playingColor), nextCheck);
                                    getPiece(nextCheck).updateMoves();
                                    valid = true;
                                    break;
                                case "B":
                                case "b":
                                    setPiece(new Bishop(playingColor), nextCheck);
                                    valid = true;
                                    break;
                                case "N":
                                case "n":
                                    setPiece(new Knight(playingColor), nextCheck);
                                    valid = true;
                                    break;
                                default:
                                    Console.WriteLine("Invalid choice!");
                                    break;
                            }
                        }
                        else
                            Console.WriteLine("Invalid choice!");
                    }
                }
            }
            if (valid)
                Console.WriteLine("Congratulations!");
            return valid;
        }

        //get board and execute possibleMoves() on every piece
        public void refreshPossibleMoves()
        {
            int[] location = new int[2];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    location[0] = i;
                    location[1] = j;
                    if (chessBoard[i, j] != null)
                    {
                        getPiece(location).possibleMoves(location, this);
                    }
                }
            }
        }
        public void refreshPossibleMovesTeam()
        {
            //reset the locations
            resetPossibleMovesTeam();

            Location location=new Location();
            char color;
            
            for (int i=0; i<8;i++)
                for (int j=0;j<8;j++)
                {
                    location.setLocation(i, j);//current location
                    if ((getPiece(location)!=null))
                        if (getPiece(location).getPossibleMovesAsLocations() != null)
                        {
                            color = getPiece(location).getColor();
                            //for every location of possibleMoves of a piece, check if it's in the possibleMovesTeam
                            foreach (Location l in getPiece(location).getPossibleMovesAsLocations())
                            {
                                if (l!=null)
                                    if (!isLocationInPossibleMovesTeamCollection(l, color))
                                        setLocationInPossibleMovesTeam(l, color);
                            }
                        }
                }
        }

        void resetPossibleMovesTeam()
        {
            possibleMovesBlack = new Location[64];
            possibleMovesWhite = new Location[64];
        }

        //get location and team color - set the location in possibleMovesTeam
        void setLocationInPossibleMovesTeam(Location location, char color)
        {
            int counter = 0;
            foreach (Location l in ((color=='W')? possibleMovesWhite: possibleMovesBlack))
            {
                if (l == null)
                    break;
                counter++;
                    
            }
            if (color=='W')
            {
                possibleMovesWhite[counter] = new Location();
                possibleMovesWhite[counter].setLocation(location);
            }   
            else
            {
                possibleMovesBlack[counter] = new Location();
                possibleMovesBlack[counter].setLocation(location);
            }
        }

        //check if this location is already in the possibleMovesTeam
        bool isLocationInPossibleMovesTeamCollection(Location location,char color)
        {
            Location[] possibleMovesTeam = getPossibleMovesTeam(color);
            //Equals between location to all of possibleMovesTeam
            foreach (Location l in possibleMovesTeam)
            {
                if (l!=null)
                    if (location.Equals(l))
                        return true;
            }
            return false;
        }

        //return the possibleMovesTeam locations array
        public Location[] getPossibleMovesTeam(char color)
        {
            if (color == 'W')
                return possibleMovesWhite;
            else
                return possibleMovesBlack;
        }

        //executing updateLocation() on every piece
        public void refreshLocations()
        {
            int[] location = new int[2];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    location[0] = i;
                    location[1] = j;
                    if (chessBoard[i, j] != null)
                        getPiece(location).updateLocation(location[0], location[1]);
                }
        }

        //executing updateHistory() on every piece
        public void refreshHistories()
        {
            int[] location = new int[2];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    location[0] = i;
                    location[1] = j;
                    if (chessBoard[i, j] != null)
                        getPiece(location).updateHistory(roundsCounter);
                }
        }

        //Print board
        public void printBoard()
        {
            Console.WriteLine("    A    B    C    D    E    F    G    H");
            for (int i = 0; i < 8; i++)
            {
                Console.WriteLine();
                Console.Write(i + 1);
                for (int j = 0; j < 8; j++)
                {
                    Console.Write("   ");
                    Console.Write(chessBoard[i, j]);
                    if (chessBoard[i, j] == null)
                        Console.Write("EE");
                }
                Console.WriteLine();
            }
        }
    }

    interface IMove
    {
        void possibleMoves(int[] from, Board board);
    }
    class Piece : IMove
    {
        string[] possibleMovesArray;
        int movesCounter;
        char color;
        protected string sign;
        protected int lastMoveCounter;
        Location location;

        History[] gameHistory;

        public Piece(char color)
        {
            this.color = color;
            movesCounter = 0;
            location = new Location();
            gameHistory = new History[200];

        }
        //in Main --> every round execute updateHistory
        public void updateHistory (int roundsCounter)
        {
            int row, column;
            gameHistory[roundsCounter] = new History(movesCounter);
            row = location.getRow();
            column = location.getColumn();
            gameHistory[roundsCounter].setLocation(row,column);
        }

        //get number of round and check if the location in this round similar to the current location
        public bool IsCurrentLocationSimilarToPreviousLocation(int round)
        {
            if (getHistory(round) == null)
                return false;
            if (!getHistory(round).getLocation().Equals(location))
                return false;
            else
            {
                //check King and Rook STATUS (Castling)
                if (this is King || this is Rook)
                    if (getHistory(round).getMovesCounter() == 0)
                        if (getMovesCount() != 0)
                            return false;
            }
            return true;
        }

        //get round and return history of specific piece
        public History getHistory(int round)
        {
            return gameHistory[round];
        }

        public virtual int getLastMoveCounter()
        {
            return lastMoveCounter;
        }

        public virtual void setLastMoveCounter(int moveCounter){ }

        public string[] getPossibleMoves()
        {
            return possibleMovesArray;
        }

        public Location[] getPossibleMovesAsLocations()
        {
            int len = possibleMovesArray.Length;
            if (len==1)
                return null;
            Location[] locations = new Location[len-1];//last one ""
            
            int count = 0,row, column;
            foreach (string s in possibleMovesArray)
            {
                if (s.Length>1)
                {
                    row = s[0] - '0';
                    column = s[1] - '0';

                    locations[count] = new Location();
                    locations[count].setLocation(row, column);
                    count++;
                }
            }
            return locations;
        }

        protected void setPossibleMoves(string[] allOptions)
        {
            possibleMovesArray = allOptions;
        }

        public void updateLocation(int row,int column)
        {
            location.setLocation(row, column);
        }

        public Location getLocation()
        {
            return location;
        }

        public void updateMoves()
        {
            movesCounter++;
        }

        public void undoMoves()
        {
            movesCounter--;
        }

        public int getMovesCount()
        {
            return movesCounter;
        }

        public char getColor()
        {
            return color;
        }

        public override string ToString()
        {
            return sign;
        }

        public virtual void possibleMoves(int[] from, Board board) { }
    }
    class King : Piece, IMove
    {
        public King(char color) : base(color)
        {
            sign = "K" + color;
        }

        public override void possibleMoves(int[] from, Board board)
        {
            //allOptions="21 54 23"
            //nextCheck={1,2}
            string allOptions = "";
            char playingColor = board.getPiece(from).getColor();
            int[] nextCheck = new int[2];
            
            //top
            nextCheck[0] = from[0] - 1;
            nextCheck[1] = from[1];
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //bottom
            nextCheck[0] = from[0] + 1;
            nextCheck[1] = from[1];
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //left
            nextCheck[0] = from[0];
            nextCheck[1] = from[1] - 1;
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //right
            nextCheck[0] = from[0];
            nextCheck[1] = from[1] + 1;
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //top-right
            nextCheck[0] = from[0] - 1;
            nextCheck[1] = from[1] + 1;
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //top-left
            nextCheck[0] = from[0] - 1;
            nextCheck[1] = from[1] - 1;
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //bottom-right
            nextCheck[0] = from[0] + 1;
            nextCheck[1] = from[1] + 1;
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //bottom-left
            nextCheck[0] = from[0] + 1;
            nextCheck[1] = from[1] - 1;
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //Castling
            bool rowIsEmpty = true;
            //left side of the board (NEED TO BE GERERIC)
            //if King didn't move yet
            if (board.getPiece(from).getMovesCount() == 0)
            {
                nextCheck[0] = from[0];
                nextCheck[1] = 0;

                Rook rook = board.getPiece(nextCheck) as Rook;

                if ((rook != null) && (rook.getMovesCount() == 0) && (rook.getColor() == playingColor))
                {
                    //if the space between the King and the Rook is empty
                    for (int i = 1; i < from[1]; i++)
                    {
                        nextCheck[1] = i;
                        if (!board.isEmpty(nextCheck))
                            rowIsEmpty = false;
                    }
                    if (rowIsEmpty)
                        allOptions += "" + nextCheck[0] + 2 + ' ';
                }
            }

            //right side of the board (NEED TO BE GERERIC)
            rowIsEmpty = true;
            //if King didn't move yet
            if (board.getPiece(from).getMovesCount() == 0)
            {
                nextCheck[0] = from[0];
                nextCheck[1] = 7;

                Rook rook = board.getPiece(nextCheck) as Rook;

                if ((rook != null) && (rook.getMovesCount() == 0) && (rook.getColor() == playingColor))
                {
                    //if the row between the King and Rook is empty
                    for (int i = from[1] + 1; i < 7; i++)
                    {
                        nextCheck[1] = i;
                        if (!board.isEmpty(nextCheck))
                            rowIsEmpty = false;
                    }
                    if (rowIsEmpty)
                        allOptions += "" + nextCheck[0] + 6 + ' ';
                }
            }
            //setPossibleMoves({21,54,32} | first number=row | second number=column
            setPossibleMoves(allOptions.Split(' '));
        }
    }
    class Queen : Piece, IMove
    {
        public Queen(char color) : base(color)
        {
            sign = "Q" + color;
        }

        public override void possibleMoves(int[] from, Board board)
        {
            //allOptions="21 54 23"
            //nextCheck={1,2}
            string allOptions = "";
            char playingColor = board.getPiece(from).getColor();

            bool empty, opponent;

            int[] nextCheck = new int[2];
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            
            //top
            while (true)
            {
                nextCheck[0]--;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;

            }

            //bottom
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[0]++;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //left
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[1]--;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //right
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[1]++;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //top-right
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[0]--;
                nextCheck[1]++;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //top-left
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[0]--;
                nextCheck[1]--;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //bottom-left
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[0]++;
                nextCheck[1]--;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //bottom-right
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[0]++;
                nextCheck[1]++;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //setPossibleMoves({21,54,32} | first number=row | second number=column
            setPossibleMoves(allOptions.Split(' '));
        }
    }
    class Bishop : Piece, IMove
    {
        public Bishop(char color) : base(color)
        {
            sign = "B" + color;
        }
        public override void possibleMoves(int[] from, Board board)
        {
            //allOptions="21 54 23"
            //nextCheck={1,2} {row,column}
            string allOptions = "";
            char playingColor = board.getPiece(from).getColor();

            bool empty, opponent;

            int[] nextCheck = new int[2];
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            //top-right
            while (true)
            {
                nextCheck[0]--;
                nextCheck[1]++;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //top-left
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[0]--;
                nextCheck[1]--;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //bottom-left
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[0]++;
                nextCheck[1]--;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //bottom-right
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[0]++;
                nextCheck[1]++;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //setPossibleMoves({21,54,32} | first number=row | second number=column
            setPossibleMoves(allOptions.Split(' '));
        }
    }
    class Knight : Piece, IMove
    {
        public Knight(char color) : base(color)
        {
            sign = "N" + color;
        }

        public override void possibleMoves(int[] from, Board board)
        {
            //allOptions="21 54 23"
            //nextCheck={1,2}
            string allOptions = "";
            char playingColor = board.getPiece(from).getColor();
            int[] nextCheck = new int[2];

            //top
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //top-right
            nextCheck[0] = from[0] - 2;//row
            nextCheck[1] = from[1] + 1;//column
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
            //top-left
            nextCheck[0] = from[0] - 2;//row
            nextCheck[1] = from[1] - 1;//column
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //right-top
            nextCheck[0] = from[0] - 1;//row
            nextCheck[1] = from[1] + 2;//column
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
            //right-bottom
            nextCheck[0] = from[0] + 1;//row
            nextCheck[1] = from[1] + 2;//column
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
            //left-top
            nextCheck[0] = from[0] - 1;//row
            nextCheck[1] = from[1] - 2;//column
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
            //left-bottom
            nextCheck[0] = from[0] + 1;//row
            nextCheck[1] = from[1] - 2;//column
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //bottom-right
            nextCheck[0] = from[0] + 2;//row
            nextCheck[1] = from[1] + 1;//column
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
            //bottom-left
            nextCheck[0] = from[0] + 2;//row
            nextCheck[1] = from[1] - 1;//column
            if (board.isEmpty(nextCheck) || board.isOpponent(nextCheck, playingColor))
                allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

            //setPossibleMoves({21,54,32} | first number=row | second number=column
            setPossibleMoves(allOptions.Split(' '));
        }
    }
    class Rook : Piece, IMove
    {
        public Rook(char color) : base(color)
        {
            sign = "R" + color;
        }
        public override void possibleMoves(int[] from, Board board)
        {
            //allOptions="21 54 23"
            //nextCheck={1,2}
            string allOptions = "";
            char playingColor = board.getPiece(from).getColor();

            bool empty, opponent;

            int[] nextCheck = new int[2];
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            //top
            while (true)
            {
                nextCheck[0]--;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //bottom
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[0]++;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //left
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[1]--;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //right
            nextCheck[0] = from[0];
            nextCheck[1] = from[1];
            while (true)
            {
                nextCheck[1]++;
                empty = board.isEmpty(nextCheck);
                opponent = board.isOpponent(nextCheck, playingColor);

                if (empty || opponent)
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                else
                    break;
                if (opponent)
                    break;
            }

            //setPossibleMoves({21,54,32} | first number=row | second number=column
            setPossibleMoves(allOptions.Split(' '));
        }
    }
    class Pawn : Piece, IMove
    {
        public Pawn(char color) : base(color)
        {
            sign = "P" + color;
        }

        public override int getLastMoveCounter()
        {
            return lastMoveCounter;
        }

        public override void setLastMoveCounter(int moveCounter)
        {
            lastMoveCounter = moveCounter;
        }

        public override void possibleMoves(int[] from, Board board)
        {
            //allOptions="21 54 23"
            //nextCheck={1,2}
            //playing = 'W' or 'B'
            string allOptions = "";
            char playingColor = board.getPiece(from).getColor();
            int[] nextCheck = new int[2];

            //white player
            if (board.getPiece(from).getColor() == 'W')
            {
                nextCheck[0] = from[0] + 1;
                nextCheck[1] = from[1];
                if (board.isEmpty(nextCheck))
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                //if it's the first move
                if (board.getPiece(from).getMovesCount() == 0)
                {
                    if (board.isEmpty(nextCheck))
                    {
                        nextCheck[0] += 1;
                        if (board.isEmpty(nextCheck))
                            allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                    }
                    
                }
                //beating right
                nextCheck[0] = from[0] + 1;
                nextCheck[1] = from[1] + 1;
                if (board.isOpponent(nextCheck, playingColor))
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                //beating left
                nextCheck[0] = from[0] + 1;
                nextCheck[1] = from[1] - 1;
                if (board.isOpponent(nextCheck, playingColor))
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
            }

            //black player
            if (board.getPiece(from).getColor() == 'B')
            {
                nextCheck[0] = from[0] - 1;
                nextCheck[1] = from[1];
                if (board.isEmpty(nextCheck))
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';

                //if it's the first move
                if (board.getPiece(from).getMovesCount() == 0)
                {
                    if (board.isEmpty(nextCheck))
                    {
                        nextCheck[0] -= 1;
                        if (board.isEmpty(nextCheck))
                            allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                    }
                    
                }
                //beating right
                nextCheck[0] = from[0] - 1;
                nextCheck[1] = from[1] + 1;
                if (board.isOpponent(nextCheck, playingColor))
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
                //beating left
                nextCheck[0] = from[0] - 1;
                nextCheck[1] = from[1] - 1;
                if (board.isOpponent(nextCheck, playingColor))
                    allOptions += "" + nextCheck[0] + nextCheck[1] + ' ';
            }

            //inPassing
            //nextCheck (NEED TO BE GENERIC)
            nextCheck[0] = from[0];
            //left
            nextCheck[1] = from[1] - 1;

            for (int i = 0; i < 2; i++)
            {
                if (board.isOpponent(nextCheck, playingColor) && board.getPiece(nextCheck).getMovesCount() == 1)
                {
                    if (board.getPiece(nextCheck).getLastMoveCounter() == board.getMovesCounter() - 1)
                    {
                        if (nextCheck[0] == 3 || nextCheck[0] == 4)
                        {
                            nextCheck[0] = (playingColor == 'W') ? (nextCheck[0] + 1) : (nextCheck[0] - 1);
                            allOptions += "" + (nextCheck[0]) + (nextCheck[1]) + ' ';
                        }
                    }
                }
                //right
                nextCheck[1] = from[1] + 1;
            }
            //setPossibleMoves({21,54,32} | first number=row | second number=column
            setPossibleMoves(allOptions.Split(' '));
        }
    }
}