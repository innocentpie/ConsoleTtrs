using System;
using System.Collections.Generic;
using System.Text;

namespace Tetris
{
    public class Tile
    {
        public enum Rotation
        {
            Clockwise = 0,
            CounterClockwise = 1,
        }

        public ConsoleColor color;

        /// <summary>
        /// The matrix given at creation
        /// </summary>
        public bool[,] Original { get; private set; }


        /// <summary>
        /// The rotated matrix (used for storing the tile's state
        /// </summary>
        public bool[,] Rotated { get; private set; }


        /// <param name="tileMatrix4row4col">
        ///     (4, 4) 2d array
        ///     <br></br>
        ///     0 means blank (no block)
        ///     <br></br>
        ///     1 means block is there
        /// </param>
        public Tile(bool[,] tileMatrix4row4col)
        {
            Original = new bool[4, 4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Original[i, j] = tileMatrix4row4col[i, j];
                }
            }

            Rotated = (bool[,])Original.Clone();
            color = (ConsoleColor)(new Random().Next(1, 15));
        }


        /// <summary>
        /// Rotates the matrix in the given direction
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="storeRotated">True, if the rotated matrix should be stored for later use (save it's state)</param>
        /// <returns></returns>
        public bool[,] RotateMatrix(Rotation rotation, bool storeRotated)
        {
            if (storeRotated)
            {
                if (rotation == Rotation.CounterClockwise)
                    RotateMatrixCounterClockwise90(4, Rotated);
                else
                    RotateMatrixClockwise90(4, Rotated);

                return Rotated;
            }
            else
            {
                bool[,] copy = (bool[,])Rotated.Clone();
                if (rotation == Rotation.CounterClockwise)
                    RotateMatrixCounterClockwise90(4, copy);
                else
                    RotateMatrixClockwise90(4, copy);
                return copy;
            }
        }



        private static void RotateMatrixCounterClockwise90(int n, bool[,] matrix)
        {
            for (int i = 0; i < n / 2; i++)
            {
                for (int j = i; j < n - i - 1; j++)
                {
                    bool temp = matrix[i, j];
                    matrix[i, j] = matrix[j, n - 1 - i];
                    matrix[j, n - 1 - i] = matrix[n - 1 - i, n - 1 - j];
                    matrix[n - 1 - i, n - 1 - j] = matrix[n - 1 - j, i];
                    matrix[n - 1 - j, i] = temp;
                }
            }
        }

        private static void RotateMatrixClockwise90(int n, bool[,] matrix)
        {
            for (int i = 0; i < n / 2; i++)
            {
                for (int j = i; j < n - i - 1; j++)
                {
                    bool temp = matrix[i, j];
                    matrix[i, j] = matrix[n - 1 - j, i];
                    matrix[n - 1 - j, i] = matrix[n - 1 - i, n - 1 - j];
                    matrix[n - 1 - i, n - 1 - j] = matrix[j, n - 1 - i];
                    matrix[j, n - 1 - i] = temp;
                }
            }
        }
    }
}
