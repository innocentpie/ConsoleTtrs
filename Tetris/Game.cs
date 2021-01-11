using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tetris
{
    class Game
    {
        public enum MoveDirection
        {
            Left = 0,
            Right = 1,
            Down = 2
        }

        public enum RotateDirection
        {
            CounterClockwise = 0,
            Clockwise = 1
        }

        /// <summary>
        /// DoubleSpace
        /// </summary>
        const string dsp = "  ";
        
        /// <summary>
        /// SingleSpace
        /// </summary>
        const char ssp = ' ';
        
        /// <summary>
        /// Border
        /// </summary>
        const string bor = "\u25A1 ";
        
        /// <summary>
        /// Square
        /// </summary>
        const string sq = "\u25A0 ";

        const float checkInputFPS = 60f;

        const float additionalTileFallFPSPerSecond = 0.01f;
        const float originalTileFallingFPS = 1.3f;
        private float tileFallingFPS = 1.3f;


        // Controls >->

        const char rotateClockwiseChar = 'e';
        const char rotateCounterClockwiseChar = 'q';

        const char moveRight = 'd';
        const char moveLeft = 'a';
        const char moveDown = 's';

        const char restartGameChar = 'r';


        public int BodyWidth
        {
            get => bodyTileMatrix.GetLength(1);
        }

        public int BodyHeight
        {
            get => bodyTileMatrix.GetLength(0);
        }


        public int score = 0000000;

        public List<Tile> tileSet;

        private Random rnd;

        private Tile nextTile;
        private Tile currentTile;

        // Tiles are stored in this matrix
        private bool[,] bodyTileMatrix;

        // This will store the color for each tile
        private ConsoleColor[,] bodyColorMatrix;

        private (int, int) currTileTopLeft = (-1, -1);

        private bool spawnNextCycle = false;
        private bool lost = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="height">Minimum is 4</param>
        /// <param name="width">Minimum is 10</param>
        /// <param name="_tileSet"></param>
        public Game(int height, int width, List<Tile> _tileSet)
        {
            if (height < 4)
                height = 4;

            if (width < 10)
                width = 10;

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            rnd = new Random();
            tileSet = _tileSet;

            bodyTileMatrix = new bool[height, width];
            bodyColorMatrix = new ConsoleColor[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    bodyColorMatrix[i, j] = ConsoleColor.Black;
                }
            }
        }




        public void StartGame()
        {
            Console.CursorVisible = false;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            Stopwatch speedIncreaseSw = new Stopwatch();
            speedIncreaseSw.Start();

            Stopwatch checkInputSw = new Stopwatch();
            checkInputSw.Start();

            Stopwatch tileFallSw = new Stopwatch();
            tileFallSw.Start();

            nextTile = new Tile(tileSet[rnd.Next(0, tileSet.Count)].Original);
            SpawnTile();
            Update();

            while (true)
            {
                if (!lost && speedIncreaseSw.ElapsedMilliseconds / 1000f >= 1f)
                {
                    // Check the console cursor visibility (it will become visible if the player resized the console window)
                    if (Console.CursorVisible)
                        Console.CursorVisible = false;

                    tileFallingFPS += additionalTileFallFPSPerSecond;
                    speedIncreaseSw.Restart();
                }

                if (!lost && checkInputSw.ElapsedMilliseconds / 1000f >= 1f / checkInputFPS)
                {
                    if (Console.KeyAvailable)
                    {
                        char readKey = Console.ReadKey(true).KeyChar;

                        switch (readKey)
                        {
                            case rotateClockwiseChar:
                                RotateTile(RotateDirection.Clockwise);
                                Update();
                                break;

                            case rotateCounterClockwiseChar:
                                RotateTile(RotateDirection.CounterClockwise);
                                Update();
                                break;

                            case moveLeft:
                                MoveTile(MoveDirection.Left);
                                Update();
                                break;

                            case moveRight:
                                MoveTile(MoveDirection.Right);
                                Update();
                                break;

                            case moveDown:
                                tileFallSw.Restart();
                                MoveTile(MoveDirection.Down);
                                if (spawnNextCycle)
                                {
                                    if (!SpawnTile())
                                        lost = true;
                                    spawnNextCycle = false;
                                }

                                Update();
                                break;

                            default:
                                break;
                        }
                    }
                    checkInputSw.Restart();
                }

                if (!lost && tileFallSw.ElapsedMilliseconds / 1000f >= 1f / tileFallingFPS)
                {
                    MoveTile(MoveDirection.Down);
                    if (spawnNextCycle)
                    {
                        // If couldn't spawn tile, that means the player lost
                        // Display the lose screen and wait for further input (restart button)
                        if (!SpawnTile())
                            lost = true;

                        spawnNextCycle = false;
                    }

                    Update();
                    tileFallSw.Restart();
                }

                if (lost)
                {
                    if (Console.KeyAvailable)
                    {
                        if (Console.ReadKey(true).KeyChar == restartGameChar)
                        {
                            Restart();
                            tileFallingFPS = originalTileFallingFPS;
                            currentTile = null;
                            nextTile = new Tile(tileSet[rnd.Next(0, tileSet.Count)].Original);
                            spawnNextCycle = true;
                            lost = false;
                        }
                    }
                }
            }
        }

        private void Restart()
        {
            score = 0;

            for (int i = 0; i < BodyHeight; i++)
            {
                for (int j = 0; j < BodyWidth; j++)
                {
                    bodyTileMatrix[i, j] = false;
                    bodyColorMatrix[i, j] = ConsoleColor.White;
                }
            }
        }

        private void Update()
        {
            //Clear(0, 0, BodyWidth * 2 + 4, BodyHeight + 2 + 1 + 6);
            Console.SetCursorPosition(0, 0);
            DrawHead();
            DrawBody();

            void DrawHead()
            {
                // Just the text

                Console.Write($"{bor}{bor}{bor}{bor}{bor}{bor}{bor}{bor}{new string(ssp, 16)}{bor}{bor}{bor}{bor}{bor}{bor}{bor}{bor}{bor}\n");

                Console.Write(bor);
                Console.ForegroundColor = nextTile.color;
                Console.Write($"{dsp}{NTMETC(0, 0)}{NTMETC(0, 1)}{NTMETC(0, 2)}{NTMETC(0, 3)}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{dsp}{bor}{dsp}CONSOLE{new string(ssp, 7)}{bor}{new string(ssp, 14)}{bor}{new string(ssp, 10)}CONTROLS\n");

                Console.Write(bor);
                Console.ForegroundColor = nextTile.color;
                Console.Write($"{dsp}{NTMETC(1, 0)}{NTMETC(1, 1)}{NTMETC(1, 2)}{NTMETC(1, 3)}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{dsp}{bor}{dsp}{dsp}TETRIS{new string(ssp, 6)}{bor}{dsp}SCORE:{new string(ssp, 6)}{bor}{new string(ssp, 6)}ROTATE: 'q' - 'e'\n");

                Console.Write(bor);
                Console.ForegroundColor = nextTile.color;
                Console.Write($"{dsp}{NTMETC(2, 0)}{NTMETC(2, 1)}{NTMETC(2, 2)}{NTMETC(2, 3)}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{dsp}{bor}{dsp}{dsp}{dsp}>.>{new string(ssp, 7)}{bor}{new string(ssp, 3)}{score:0000000}{dsp}{dsp}{bor}{new string(ssp, 6)}MOVE SIDEWAYS: 'a' - 'd'\n");

                Console.Write(bor);
                Console.ForegroundColor = nextTile.color;
                Console.Write($"{dsp}{NTMETC(3, 0)}{NTMETC(3, 1)}{NTMETC(3, 2)}{NTMETC(3, 3)}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{dsp}{bor}{new string(ssp, 16)}{bor}{new string(ssp, 14)}{bor}{new string(ssp, 6)}MOVE DOWN: 's'\n");

                Console.Write($"{bor}{bor}{bor}{bor}{bor}{bor}{bor}{bor}{new string(ssp, 16)}{bor}{bor}{bor}{bor}{bor}{bor}{bor}{bor}{bor}\n");




            }

            void DrawBody()
            {
                // More text

                // The head's width is 24, so push the body in if it's smaller in width
                // If not, the head is pushed in
                string push = "";
                if (BodyWidth < 25)
                    push = string.Concat(Enumerable.Repeat(dsp, (23 - BodyWidth) / 2));

                Console.WriteLine();
                Console.WriteLine(push + string.Concat(Enumerable.Repeat(bor, BodyWidth + 2)));


                // Now comes printing the matrix
                for (int i = 0; i < BodyHeight; i++)
                {

                    if (i == BodyHeight / 2 - 2 && lost)
                    {
                        string gameEndPush = "";
                        if (BodyWidth * 2 > 20)
                        {
                            gameEndPush = new string(ssp, ((BodyWidth * 2) - 20) / 2);
                        }

                        // 20 long the end
                        Console.WriteLine(push + bor + gameEndPush + "################### " + gameEndPush + bor + dsp);
                        Console.WriteLine(push + bor + gameEndPush + "#    GAME ENDED   # " + gameEndPush + bor + dsp);
                        Console.WriteLine(push + bor + gameEndPush + $"#   RESTART: \"{restartGameChar}\"  # " + gameEndPush + bor + dsp);
                        Console.WriteLine(push + bor + gameEndPush + "################### " + gameEndPush + bor + dsp);
                        i += 3;
                        continue;
                    }
                    Console.Write(push + bor);


                    for (int j = 0; j < BodyWidth; j++)
                    {
                        // Draw current Tile
                        if (currentTile != null
                            && i < currTileTopLeft.Item1 + 4
                            && i >= currTileTopLeft.Item1
                            && j < currTileTopLeft.Item2 + 4
                            && j >= currTileTopLeft.Item2)
                        {
                            // If a block is there (part of the tile)
                            if (currentTile.Rotated[i - currTileTopLeft.Item1, j - currTileTopLeft.Item2])
                            {
                                Console.ForegroundColor = currentTile.color;
                                Console.Write(BTC(bodyTileMatrix[i, j] | currentTile.Rotated[i - currTileTopLeft.Item1, j - currTileTopLeft.Item2]));
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                Console.ForegroundColor = bodyColorMatrix[i, j];
                                Console.Write(BMETC(i, j));
                                Console.ForegroundColor = ConsoleColor.White;
                            }


                        }
                        else
                        {
                            Console.ForegroundColor = bodyColorMatrix[i, j];
                            Console.Write(BMETC(i, j));
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                    }

                    Console.WriteLine(bor);
                }

                Console.WriteLine(push + string.Concat(Enumerable.Repeat(bor, BodyWidth + 2)));
            }
        }


        /// <summary>
        /// Spawn a new tile
        /// Choosing the next tile is fully random
        /// </summary>
        /// <returns>False, if player lost</returns>
        public bool SpawnTile()
        {
            if (currentTile != null)
            {
                // Write current tile to matrix (it's fixed now)
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (currTileTopLeft.Item1 + i < BodyHeight
                            && currTileTopLeft.Item2 + j < BodyWidth
                            && currTileTopLeft.Item1 + i >= 0
                            && currTileTopLeft.Item2 + j >= 0)
                        {
                            bodyTileMatrix[i + currTileTopLeft.Item1, j + currTileTopLeft.Item2] = currentTile.Rotated[i, j] | bodyTileMatrix[i + currTileTopLeft.Item1, j + currTileTopLeft.Item2];

                            if (currentTile.Rotated[i, j] == true)
                            {
                                bodyColorMatrix[i + currTileTopLeft.Item1, j + currTileTopLeft.Item2] = currentTile.color;
                            }

                        }
                    }
                }

                CheckForFullRows();

                // Clear console readkey buffer
                while (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }
            }

            currTileTopLeft = (0, BodyWidth / 2 - 2);
            currentTile = nextTile;
            nextTile = new Tile(tileSet[rnd.Next(0, tileSet.Count)].Original);

            // Check if the spawned would overlap with existing tiles
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (currTileTopLeft.Item1 + i < BodyHeight
                        && currTileTopLeft.Item2 + j < BodyWidth
                        && currTileTopLeft.Item1 + i >= 0
                        && currTileTopLeft.Item2 + j >= 0)
                    {
                        if (currentTile.Rotated[i, j] & bodyTileMatrix[currTileTopLeft.Item1 + i, currTileTopLeft.Item2 + j] == true)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void MoveTile(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Left:
                    if (CanMove((currTileTopLeft.Item1, currTileTopLeft.Item2 - 1)))
                        currTileTopLeft.Item2--;
                    break;

                case MoveDirection.Right:
                    if (CanMove((currTileTopLeft.Item1, currTileTopLeft.Item2 + 1)))
                        currTileTopLeft.Item2++;
                    break;

                case MoveDirection.Down:
                    if (CanMove((currTileTopLeft.Item1 + 1, currTileTopLeft.Item2)))
                        currTileTopLeft.Item1++;
                    break;
            }

            bool CanMove((int, int) newTopLeft)
            {
                if (currentTile == null)
                    return false;

                // Moving the top left down is only possible when the move direction is Down
                if (direction == MoveDirection.Down)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        // Row is out of bounds (would move into the floor)
                        // This is where I spawn an other tile (fixing is in spawn method)
                        if (newTopLeft.Item1 + i >= BodyHeight)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                if (currentTile.Rotated[i, j] == true)
                                {
                                    spawnNextCycle = true;
                                    return false;
                                }
                            }
                        }
                    }
                }

                // Check the moved if it would overlap with existing tiles
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (newTopLeft.Item1 + i < BodyHeight
                            && newTopLeft.Item2 + j < BodyWidth
                            && newTopLeft.Item1 + i >= 0
                            && newTopLeft.Item2 + j >= 0)
                        {
                            if (currentTile.Rotated[i, j] & bodyTileMatrix[newTopLeft.Item1 + i, newTopLeft.Item2 + j] == true)
                            {
                                // Only spawn if moving down
                                if (direction == MoveDirection.Down)
                                    spawnNextCycle = true;

                                return false;
                            }
                        }
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    // Column is out of bounds
                    if (newTopLeft.Item2 + i < 0 || newTopLeft.Item2 + i >= BodyWidth)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if (currentTile.Rotated[j, i] == true)
                                return false;
                        }
                    }
                }

                return true;
            }
        }

        public void RotateTile(RotateDirection direction)
        {
            switch (direction)
            {
                case RotateDirection.CounterClockwise:
                    if (CanRotate())
                        currentTile.RotateMatrix(Tile.Rotation.CounterClockwise, true);
                    break;

                case RotateDirection.Clockwise:
                    if (CanRotate())
                        currentTile.RotateMatrix(Tile.Rotation.Clockwise, true);
                    break;
            }

            bool CanRotate()
            {
                if (currentTile == null)
                    return false;

                bool[,] rotated;

                // Get the rotated tile
                if (direction == RotateDirection.CounterClockwise)
                    rotated = currentTile.RotateMatrix(Tile.Rotation.CounterClockwise, false);
                else
                    rotated = currentTile.RotateMatrix(Tile.Rotation.Clockwise, false);

                // Check the rotated if it would overlap with existing tiles
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (currTileTopLeft.Item1 + i < BodyHeight
                            && currTileTopLeft.Item2 + j < BodyWidth
                            && currTileTopLeft.Item1 + i >= 0
                            && currTileTopLeft.Item2 + j >= 0)
                        {
                            if (rotated[i, j] & bodyTileMatrix[currTileTopLeft.Item1 + i, currTileTopLeft.Item2 + j] == true)
                                return false;
                        }
                    }
                }

                // Check if tile would get out of bounds after rotation
                for (int i = 0; i < 4; i++)
                {
                    // Row is out of bounds
                    if (currTileTopLeft.Item1 + i >= BodyHeight)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if (rotated[i, j] == true)
                                return false;
                        }
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    // Column is out of bounds
                    if (currTileTopLeft.Item2 + i < 0 || currTileTopLeft.Item2 + i >= BodyWidth)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if (rotated[j, i] == true)
                                return false;
                        }
                    }
                }

                return true;
            }
        }

        public void CheckForFullRows()
        {
            List<int> rowsToDelete = new List<int>();
            bool delRow;

            // Iterate throug every row and check if every element in that row is true
            for (int i = 0; i < BodyHeight; i++)
            {
                delRow = true;

                for (int j = 0; j < BodyWidth; j++)
                {
                    if (bodyTileMatrix[i, j] == false)
                    {
                        delRow = false;
                        break;
                    }
                }

                if (delRow)
                    rowsToDelete.Add(i);
            }

            if (rowsToDelete.Count > 0)
            {
                // Add the score    https://tetris.fandom.com/wiki/Scoring - Level 2 scoring
                score += rowsToDelete.Count switch
                {
                    1 => 120,
                    2 => 300,
                    3 => 900,
                    int alias when alias >= 4 => 3600,
                };

                currentTile = null;
                DeleteRows();
            }


            void DeleteRows()
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();

                // If width is odd, delete the middle block from every row first
                if (BodyWidth % 2 == 1)
                {
                    foreach (var item in rowsToDelete)
                    {
                        bodyTileMatrix[item, BodyWidth / 2] = false;
                        bodyColorMatrix[item, BodyWidth / 2] = ConsoleColor.White;
                        Update();
                    }
                }

                // Delete the rows
                for (int i = BodyWidth / 2; i > 0; i--)
                {
                    while (sw.ElapsedMilliseconds / 1000f < 1f / 24f) ;

                    foreach (var item in rowsToDelete)
                    {
                        bodyTileMatrix[item, i - 1] = false;
                        bodyTileMatrix[item, BodyWidth - i] = false;

                        bodyColorMatrix[item, i - 1] = ConsoleColor.White;
                        bodyColorMatrix[item, BodyWidth - i] = ConsoleColor.White;

                        Update();
                    }

                    sw.Restart();

                }

                // Move the rows down
                // Move upper rows down to the deleted one (move the rows, not the individual blocks in the row)

                // Start with upper rows and move the whole matrix above it down
                for (int i = 0; i < rowsToDelete.Count; i++)
                {
                    // Start moving from bottom row
                    for (int j = rowsToDelete[i]; j >= 0; j--)
                    {
                        for (int k = 0; k < BodyWidth; k++)
                        {
                            // Clear upper row
                            if (j == 0)
                            {
                                bodyTileMatrix[j, k] = false;
                                bodyColorMatrix[j, k] = ConsoleColor.White;
                            }
                            // Move down existing rows
                            else
                            {
                                bodyTileMatrix[j, k] = bodyTileMatrix[j - 1, k];
                                bodyColorMatrix[j, k] = bodyColorMatrix[j - 1, k];
                            }
                        }
                    }
                }

            }
        }


        /// <summary>
        /// BodyMatrixElementToChar
        /// </summary>
        private string BMETC(int row, int col)
        {
            if (bodyTileMatrix[row, col])
                return sq;

            return dsp;
        }

        /// <summary>
        /// NextTileMatrixElementToChar
        /// </summary>
        private string NTMETC(int x, int y)
        {
            if (nextTile.Rotated[x, y])
                return sq;

            return dsp;
        }


        /// <summary>
        /// BoolToChar
        /// </summary>
        private string BTC(bool b)
        {
            if (b)
                return sq;

            return dsp;
        }

    }
}
