using System;
using System.Collections;
using System.Collections.Generic;

namespace Maze
{
    public class ExtendingWall
    {
        static private int FIELD_SIZE_X = 51;
        static private int FIELD_SIZE_Z = 51;

        static private int[,] CellData;

        // 乱数生成用
        static private System.Random Random;
        // 壁の拡張を行う開始セルの情報
        static private List<Cell> StartCells;
        // 現在拡張中の壁の情報を保持
        static private List<Cell> CurrentWallCells;
        // 拡張可能方向の情報を保持
        static private List<List<Cell>> CandidateCells;

        static private int[,] DebugCell;
        static private List<Cell> CellLog;

        static private List<Cell> Toji;

        private static void Main()
        {
            CellData = new int[FIELD_SIZE_Z, FIELD_SIZE_X];
            Generate(CellData);
            TOJIDebug();

            DebugOutput(CellData);

            int count = 0;

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
                        count = 0;
                    }
                }
                else
                {
                    int input_num = int.Parse(input);
                    if (0 <= input_num && input_num <= CellLog.Count)
                    {
                        count = input_num;
                        DebugSimulate(input_num);
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
                            count = 0;
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
            CandidateCells = new List<List<Cell>>();

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
                    CandidateCells.Clear();
                    CurrentWallCells.Add(new Cell(x, z));
                    ExtendWall(x, z);
                }
            }
            return CellData;
        }

        // 指定座標から壁を生成拡張する
        static private void ExtendWall(int x, int z)
        {
            // 探索位置に初めて到達(次のセルの方向が記録されていない)
            if (CurrentWallCells.Count != CandidateCells.Count)
            {
                List<Cell> candidate_cells = new List<Cell>();
                // 伸ばすことができる方向のセル(1マス先が通路で2マス先まで範囲内)
                // 2マス先が壁で自分自身の場合は、伸ばせない
                if (CellData[z - 1, x] == DEFINITION.TYPE_PATH && !IsCurrentWall(x, z - 2))
                    candidate_cells.Add(new Cell(x, z - 2));
                if (CellData[z, x + 1] == DEFINITION.TYPE_PATH && !IsCurrentWall(x + 2, z))
                    candidate_cells.Add(new Cell(x + 2, z));
                if (CellData[z + 1, x] == DEFINITION.TYPE_PATH && !IsCurrentWall(x, z + 2))
                    candidate_cells.Add(new Cell(x, z + 2));
                if (CellData[z, x - 1] == DEFINITION.TYPE_PATH && !IsCurrentWall(x - 2, z))
                    candidate_cells.Add(new Cell(x - 2, z));

                CandidateCells.Add(candidate_cells);
            }

            //現在のセルでの候補方向を選択
            var current_candidate = CandidateCells[CandidateCells.Count - 1];

            //ランダムに伸ばす(2マス)
            if (current_candidate.Count > 0)
            {
                // 伸ばす先が通路の場合は拡張を続ける
                bool isPath = false;
                int dirIndex = Random.Next(current_candidate.Count);

                int cell_x = current_candidate[dirIndex].X;
                int cell_z = current_candidate[dirIndex].Z;

                isPath = (CellData[cell_z, cell_x] == DEFINITION.TYPE_PATH);
                CandidateCells[CandidateCells.Count - 1].RemoveAt(dirIndex);
                CurrentWallCells.Add(new Cell(cell_x, cell_z));

                if (isPath)
                {
                    // 既存の壁に接続できていない場合は拡張続行
                    ExtendWall(cell_x, cell_z);
                }
                else
                {
                    //壁を作成する
                    SetWall();
                    //DebugOutput(CellData);
                }
            }
            else
            {
                // すべて現在拡張中の壁にぶつかる場合、バックして再開
                Cell beforeCell = CurrentWallCells[CurrentWallCells.Count - 1];
                CandidateCells.RemoveAt(CandidateCells.Count - 1);
                CurrentWallCells.RemoveAt(CurrentWallCells.Count - 1);
                ExtendWall(beforeCell.X, beforeCell.Z);
            }
        }

        //壁を拡張する(再帰)
        static private void SetWall()
        {
            //現在の座標を取得
            int current_x = CurrentWallCells[0].X;
            int current_z = CurrentWallCells[0].Z;

            //現在の座標に壁を作成
            CellData[current_z, current_x] = DEFINITION.TYPE_WALL;
            CellLog.Add(new Cell(current_x, current_z));

            //拡張した方向を取得
            int diff_x = (CurrentWallCells[1].X - current_x) / 2;
            int diff_z = (CurrentWallCells[1].Z - current_z) / 2;

            //拡張した方向の奇数セルに壁を作成
            CellData[current_z + diff_z, current_x + diff_x] = DEFINITION.TYPE_WALL;
            CellLog.Add(new Cell(current_x + diff_x, current_z + diff_z));

            //壁の作成が完了したので対象セルを削除
            CurrentWallCells.RemoveAt(0);

            //次のセルが取得できる場合関数を実行する
            if (CurrentWallCells.Count > 1) SetWall();
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
            if (count < CellLog.Count)
            {
                DebugCell[CellLog[count].Z, CellLog[count].X] = 3;
            }

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
    }
}
