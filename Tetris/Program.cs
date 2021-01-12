using System;
using System.Collections.Generic;
using System.IO;

namespace Tetris
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Tile> tiles = new List<Tile>();
            bool[,] tmpTile = new bool[4, 4];

            // Read tiles from file
            using (FileStream fs = new FileStream("Tiles.txt", FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {

                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                // Character code of 0 is 48, 1 is 49
                                tmpTile[i, j] = sr.Read() == 48 ? false : true;
                            }
                            sr.ReadLine();
                        }

                        tiles.Add(new Tile(tmpTile));
                        sr.ReadLine();
                    }
                }
            }

            Game game = new Game(26, 11, tiles);
            game.StartGame();
        }
    }


}
