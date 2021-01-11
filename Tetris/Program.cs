using System;
using System.Collections.Generic;

namespace Tetris
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define tiles
            // 4*4 matrix, true where there is a tile part
            List<Tile> tiles = new List<Tile>()
            {
            new Tile(new bool[,]
                {
                    {   false,  true,   true,   false },
                    {   false,  false,  true,   false},
                    {   false,  false,  true,   false },
                    {   false,  false,  false,  false }
                }),

                new Tile(new bool[,]{
                    {   false,  true,   false,   false},
                    {   false,  true,  true,   false},
                    {   false,  false,  true,   false },
                    {   false,  false,  false,  false }
                }),

                new Tile(new bool[,]{
                    {   false,  false,   true,   false},
                    {   false,  true,  true,   false},
                    {   false,  true,  false,   false },
                    {   false,  false,  false,  false }
                }),

                new Tile(new bool[,]{
                    {   false,  false,   false,   false},
                    {   true,  true,  true,   true},
                    {   false,  false,  false,   false },
                    {   false,  false,  false,  false }
                }),

                new Tile(new bool[,]{
                    {   false,  true,   false,   false},
                    {   false,  true,  false,   false},
                    {   false,  true,  false,   false },
                    {   false,  true,  false,  false }
                }),

                new Tile(new bool[,]{
                    {   false,  false,   false,   false},
                    {   false,  true,  true,   false},
                    {   false,  true,  true,   false },
                    {   false,  false,  false,  false }
                }),

                new Tile(new bool[,]{
                    {   false,  false,   false,   false},
                    {   true,  true,  true,   false},
                    {   false,  true,  false,   false },
                    {   false,  false,  false,  false }
                }),

                new Tile(new bool[,]{
                    {   false,  true,   false,   false},
                    {   false,  true,  true,   false},
                    {   false,  true,  false,   false },
                    {   false,  false,  false,  false }
                })


            };

            Game game = new Game(26, 11, tiles);
            game.StartGame();
        }
    }


}
