using System;
using System.Collections;
using System.Collections.Generic;

namespace Maze
{
    public class ExtendingWall
    {
        static private int FIELD_SIZE_X;
        static private int FIELD_SIZE_Z;

        static private int[,] CellData;

        // 乱数生成用
        static private System.Random Random;
        // 壁の拡張を行う開始セルの情報
        static private List<Cell> StartCells;
        // 現在拡張中の壁の情報を保持
        static private List<Cell> CurrentWallCells;

        static private int[,] DebugCell;
        static private List<Cell> CellLog;

        static private List<Cell> Toji;

        private static void Main()
        {
            CellData = new int[51, 51];
            Generate(CellData);
            TOJIDebug();

            int count = 1;

            while (true)
            {
                Console.WriteLine("[{0}/{1}]>> ", count, CellLog.Count);

                string input = Console.ReadLine();
                if (input == "")
                {
                    DebugSimulate(count);
                    if (count < CellLog.Count)
                    {
                        count++;
                    }
                    else
                    {
                        count = 1;
                    }
                }
                else
                {
                    int input_num = int.Parse(input);
                    if (0 < input_num && input_num <= CellLog.Count)
                    {
                        DebugSimulate(input_num);
                        count = input_num;
                    }
                    else
                    {
                        DebugSimulate(count);
                        if (count < CellLog.Count)
                        {
                            count++;
                        }
                        else
                        {
                            count = 1;
                        }
                    }
                }
                DebugOutput(DebugCell);
            }
        }

        static private void Initialization(int[,] cell_data)
        {
            //引数から各種情報を取得
            CellData = cell_data;
            FIELD_SIZE_X = CellData.GetLength(1);
            FIELD_SIZE_Z = CellData.GetLength(0);

            StartCells = new List<Maze.Cell>();
            CurrentWallCells = new List<Maze.Cell>();
            Random = new System.Random();

            DebugCell = new int[FIELD_SIZE_Z, FIELD_SIZE_X];
            CellLog = new List<Cell>();
            Toji = new List<Cell>();

            // 各マスの初期設定を行う
            for (int z = 0; z < FIELD_SIZE_Z; z++)
            {
                for (int x = 0; x < FIELD_SIZE_X; x++)
                {
                    //外周を壁に設定し、開始候補として保持
                    if (x == 0 || z == 0 || x == FIELD_SIZE_X - 1 || z == FIELD_SIZE_Z - 1)
                    {
                        CellData[z, x] = DEFINITION.TYPE_WALL;
                    }
                    else
                    {
                        CellData[z, x] = DEFINITION.TYPE_PATH;
                        //外周ではない偶数座標を壁伸ばし開始点として保持
                        if (x % 2 == 0 && z % 2 == 0)
                        {
                            // 開始候補座標
                            StartCells.Add(new Cell(x, z));
                        }
                    }
                }
            }
        }

        static public int[,] Generate(int[,] cell_data)
        {
            //初期設定メソッド
            Initialization(cell_data);

            while (StartCells.Count > 0)
            {
                // ランダムに開始セルを取得し、開始候補から削除
                int index = Random.Next(StartCells.Count);
                Cell cell = StartCells[index];
                StartCells.RemoveAt(index);

                int x = cell.X;
                int z = cell.Z;

                // すでに壁の場合は何もしない
                if (CellData[z, x] == DEFINITION.TYPE_PATH)
                {
                    //拡張中の壁の情報を初期化
                    CurrentWallCells.Clear();
                    ExtendWall(x, z);
                }
            }
            return CellData;
        }

        // 指定座標から壁を生成拡張する
        static private void ExtendWall(int x, int z)
        {
            // 伸ばすことができる方向(1マス先が通路で2マス先まで範囲内)
            // 2マス先が壁で自分自身の場合は、伸ばせない
            var directions = new List<DEFINITION.DIRECTION>();
            if (CellData[z - 1, x] == DEFINITION.TYPE_PATH && !IsCurrentWall(x, z - 2))
                directions.Add(DEFINITION.DIRECTION.UP);
            if (CellData[z, x + 1] == DEFINITION.TYPE_PATH && !IsCurrentWall(x + 2, z))
                directions.Add(DEFINITION.DIRECTION.RIGHT);
            if (CellData[z + 1, x] == DEFINITION.TYPE_PATH && !IsCurrentWall(x, z + 2))
                directions.Add(DEFINITION.DIRECTION.DOWN);
            if (CellData[z, x - 1] == DEFINITION.TYPE_PATH && !IsCurrentWall(x - 2, z))
                directions.Add(DEFINITION.DIRECTION.LEFT);

            //ランダムに伸ばす(2マス)
            if (directions.Count > 0)
            {
                // 壁の作成(この地点から壁を伸ばす)
                SetWall(x, z);

                // 伸ばす先が通路の場合は拡張を続ける
                bool isPath = false;
                int dirIndex = Random.Next(directions.Count);
                switch (directions[dirIndex])
                {
                    case DEFINITION.DIRECTION.UP:
                        isPath = (CellData[z - 2, x] == DEFINITION.TYPE_PATH);
                        SetWall(x, --z);
                        SetWall(x, --z);
                        break;
                    case DEFINITION.DIRECTION.RIGHT:
                        isPath = (CellData[z, x + 2] == DEFINITION.TYPE_PATH);
                        SetWall(++x, z);
                        SetWall(++x, z);
                        break;
                    case DEFINITION.DIRECTION.DOWN:
                        isPath = (CellData[z + 2, x] == DEFINITION.TYPE_PATH);
                        SetWall(x, ++z);
                        SetWall(x, ++z);
                        break;
                    case DEFINITION.DIRECTION.LEFT:
                        isPath = (CellData[z, x - 2] == DEFINITION.TYPE_PATH);
                        SetWall(--x, z);
                        SetWall(--x, z);
                        break;
                }
                if (isPath)
                {
                    // 既存の壁に接続できていない場合は拡張続行
                    ExtendWall(x, z);
                }
            }
            else
            {
                // すべて現在拡張中の壁にぶつかる場合、バックして再開
                Cell beforeCell = CurrentWallCells[CurrentWallCells.Count - 1];
                CurrentWallCells.RemoveAt(CurrentWallCells.Count - 1);
                ExtendWall(beforeCell.X, beforeCell.Z);
            }
        }

        //壁を拡張する
        static private void SetWall(int x, int z)
        {
            CellLog.Add(new Cell(x, z));
            CellData[z, x] = DEFINITION.TYPE_WALL;
            if (x % 2 == 0 && z % 2 == 0)
            {
                CurrentWallCells.Add(new Cell(x, z));
            }
        }

        static private bool IsCurrentWall(int x, int z)
        {
            foreach (var cell in CurrentWallCells)
            {
                if (cell.X == x && cell.Z == z)
                {
                    return true;
                }
            }
            return false;
        }

        //Debug
        static private void TOJIDebug()
        {
            for (int z = 0; z < FIELD_SIZE_Z; z++)
            {
                for (int x = 0; x < FIELD_SIZE_X; x++)
                {
                    if (x % 2 == 1 && z % 2 == 1)
                    {
                        if (CellData[z - 1, x] == DEFINITION.TYPE_WALL)
                        {
                            if (CellData[z, x + 1] == DEFINITION.TYPE_WALL)
                            {
                                if (CellData[z + 1, x] == DEFINITION.TYPE_WALL)
                                {
                                    if (CellData[z, x - 1] == DEFINITION.TYPE_WALL)
                                    {
                                        Console.WriteLine("閉じた領域検出: " + "[" + z + "," + x + "]");
                                        Toji.Add(new Cell(x, z));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        static private void DebugSimulate(int count)
        {
            for (int z = 0; z < FIELD_SIZE_Z; z++)
            {
                for (int x = 0; x < FIELD_SIZE_X; x++)
                {
                    //外周を壁に設定し、開始候補として保持
                    if (x == 0 || z == 0 || x == FIELD_SIZE_X - 1 || z == FIELD_SIZE_Z - 1)
                    {
                        DebugCell[z, x] = DEFINITION.TYPE_WALL;
                    }
                    else
                    {
                        DebugCell[z, x] = DEFINITION.TYPE_PATH;
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                DebugCell[CellLog[i].Z, CellLog[i].X] = DEFINITION.TYPE_WALL;
            }
            DebugCell[CellLog[count].Z, CellLog[count].X] = 3;



            foreach (var e in Toji)
            {
                DebugCell[e.Z, e.X] = 2;
            }
        }

        static private void DebugOutput(int[,] cell_data)
        {
            for (int z = 0; z < FIELD_SIZE_Z; z++)
            {
                for (int x = 0; x < FIELD_SIZE_X; x++)
                {
                    switch (cell_data[z, x])
                    {
                        case 0:
                            Console.Write("□");
                            break;
                        case 1:
                            Console.Write("■");
                            break;
                        case 2:
                            Console.Write("◇");
                            break;
                        case 3:
                            Console.Write("◆");
                            break;
                    }
                }
                Console.WriteLine();
            }
        }

    }

    public class Cell
    {
        public int X { get; set; }
        public int Z { get; set; }

        public Cell(int x, int z)
        {
            this.X = x;
            this.Z = z;
        }
    }

    //定義
    public static class DEFINITION
    {
        public const int TYPE_PATH = 0;
        public const int TYPE_WALL = 1;

        public enum DIRECTION
        {
            UP = 0,
            RIGHT = 1,
            DOWN = 2,
            LEFT = 3
        }
    }
}
