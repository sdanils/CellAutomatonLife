using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.IO;
using System.Dynamic;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        public Board(int width, int height, int cellSize)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }
        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
        public (int, int) CountFigures()
        {
            bool[,] visited = new bool[Columns, Rows];
            int countFigures = 0, countAlive = 0;

            int[] dirX = { -1, 1, 0, 0, -1, 1, -1, 1 };
            int[] dirY = { 0, 0, -1, 1, -1, -1, 1, 1 };

            for (int i = 0; i < Columns; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    if (Cells[i, j].IsAlive && !visited[i, j])
                    {
                        countFigures++;
                        Queue<(int, int)> queue = new Queue<(int, int)>();
                        queue.Enqueue((i, j));
                        visited[i, j] = true;
                        countAlive++;

                        while (queue.Count > 0)
                        {
                            var (x, y) = queue.Dequeue();

                            for (int k = 0; k < 8; k++)
                            {
                                int nx = x + dirX[k], ny = y + dirY[k];
                                (nx, ny) = Index.ChechIndex(nx, ny, Columns, Rows);

                                if (Cells[nx, ny].IsAlive && !visited[nx, ny])
                                {
                                    visited[nx, ny] = true;
                                    countAlive++;
                                    queue.Enqueue((nx, ny));
                                }
                            }
                        }
                    }
                }
            }

            return (countFigures, countAlive);
        }
    }
    class Index
    {
        static public (int, int) ChechIndex(int x, int y, int xMax, int yMax)
        {
            if (x >= xMax)
                x = x % xMax;
            else if (x < 0)
                x = xMax + x % xMax;

            if (y >= yMax)
                y = y % yMax;
            else if (y < 0)
                y = yMax + y % yMax;

            return (x, y);
        }
    }
    class LoaderMap
    {
        static private string CreateStr(Board board)
        {
            int columns = board.Width;
            int rows = board.Height;
            int cellSize = board.CellSize;

            var strBuilder = new StringBuilder();
            strBuilder.Append($"{columns}\n{rows}\n{cellSize}\n");

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (board.Cells[col, row].IsAlive)
                    {
                        strBuilder.Append('*');
                    }
                    else
                    {
                        strBuilder.Append(' ');
                    }
                }
                strBuilder.Append("\n");
            }
            return strBuilder.ToString();

        }
        static public void SaveMap(Board board, string nameFile)
        {
            string data = CreateStr(board);
            File.WriteAllText($"maps/{nameFile}", data);
        }
        static public Board LoadMap(string fileName)
        {
            string[] parts = File.ReadAllText($"maps/{fileName}").Split('\n');
            int width = int.Parse(parts[0]);
            int height = int.Parse(parts[1]);
            int cellSize = int.Parse(parts[2]);

            Board board = new Board(width, height, cellSize);

            int columns = width / cellSize;
            int rows = height / cellSize;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    board.Cells[col, row].IsAlive = parts[row + 3][col] == '*';
                }
            }

            return board;
        }
    }

    class ConfigData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int CellSize { get; set; }
        public double LiveDensity { get; set; }
        public int TimeSleep { get; set; }
    }
    class TemplateCounter
    {
        static private SortedDictionary<int, string> namesTemplates = new SortedDictionary<int, string>
        {
            {440895, "Template one"},
            {5187, "Template two"},
            {1001, "Template three"},
            {88179, "Template four"},
            {25935, "Template five"}
        };
        static private int sizeTemplate = 5;
        static private int[,] tamplateOne = { { 0, 1, 1 }, { 1, 0, 1 }, { 1, 1, 0 } };
        static private int[,] tamplateTwo = { { 0, 1, 0 }, { 1, 0, 1 }, { 0, 1, 0 } };
        static private int[,] tamplateThree = { { 0, 0, 0 }, { 1, 1, 1 }, { 0, 0, 0 } };
        static private int[,] tamplateFour = { { 0, 1, 0 }, { 1, 0, 1 }, { 1, 1, 0 } };
        static private int[,] tamplateFive = { { 0, 1, 1 }, { 1, 0, 1 }, { 0, 1, 0 } };
        static private int[,] hashMatrix = { { 27, 27, 27, 27, 27 }, { 27, 2, 3, 5, 27 }, { 27, 7, 11, 13, 27 }, { 27, 17, 19, 23, 27 }, { 27, 27, 27, 27, 27 } };

        static private int CountHashSubBoard(Board board, int startCol, int startRow)
        {
            int hash = 1;
            for (int i = 0; i < sizeTemplate; i++)
            {
                for (int j = 0; j < sizeTemplate; j++)
                {
                    int col = startCol + j, row = startRow + i;
                    int maxCol = board.Columns, maxRow = board.Rows;

                    (col, row) = Index.ChechIndex(col, row, maxCol, maxRow);

                    if (board.Cells[col, row].IsAlive)
                    {
                        int m = hashMatrix[j, i];
                        if (m == 27)
                        {
                            return -1;
                        }

                        hash *= m;
                    }
                }
            }
            return hash;
        }

        static public Dictionary<string, int> CountTemplates(Board board)
        {
            Dictionary<string, int> numberTemplates = new Dictionary<string, int>();
            foreach (string val in namesTemplates.Values)
                numberTemplates.Add(val, 0);

            for (int j = 0; j < board.Rows; j++)
            {
                for (int i = 0; i < board.Columns; i++)
                {
                    int hashSubMatrix = CountHashSubBoard(board, i, j);
                    if (namesTemplates.ContainsKey(hashSubMatrix))
                    {
                        numberTemplates[namesTemplates[hashSubMatrix]]++;
                    }
                }
            }
            return numberTemplates;
        }
    }

    class Program
    {
        static Board board;
        static string configFile = "ConfigCLI.json";
        static private ConfigData GetConfig()
        {
            string json = File.ReadAllText(configFile);
            return JsonSerializer.Deserialize<ConfigData>(json);
        }
        static private void Reset(ConfigData dataConfig)
        {
            board = new Board(
                width: dataConfig.Width,
                height: dataConfig.Height,
                cellSize: dataConfig.CellSize,
                liveDensity: dataConfig.LiveDensity);
        }
        static private void Set()
        {
            Console.WriteLine("Enter file name: ");
            string fileName = Console.ReadLine();
            board = LoaderMap.LoadMap(fileName);
        }
        static private void Save()
        {
            Console.WriteLine("Enter file name: ");
            string fileName = Console.ReadLine();
            LoaderMap.SaveMap(board, fileName);
        }

        static void RenderStats()
        {
            var strBuilder = new StringBuilder();

            (int countFigure, int countAlive) = board.CountFigures();
            strBuilder.Append($"Count figures: {countFigure} ");
            strBuilder.Append($"Count alive: {countAlive}\n");
            strBuilder.AppendLine();

            Dictionary<string, int> countTemplate = TemplateCounter.CountTemplates(board);
            foreach (var (name, count) in countTemplate)
            {
                strBuilder.Append($"Count {name}: {count} ");
            }
            strBuilder.AppendLine();
            Console.Write(strBuilder.ToString());
        }
        static void RenderMap()
        {
            var strBuilder = new StringBuilder();

            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        strBuilder.Append('*');
                    }
                    else
                    {
                        strBuilder.Append(' ');
                    }
                }
                strBuilder.AppendLine();
            }
            Console.Clear();
            Console.Write(strBuilder.ToString());
        }
        static void Main(string[] args)
        {
            var dataConfig = GetConfig();
            Reset(dataConfig);

            bool pauseStatus = true;
            while (true)
            {
                Thread.Sleep(50);
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Q)
                    {
                        pauseStatus = pauseStatus ? false : true;
                    }
                    if (pauseStatus && key == ConsoleKey.S)
                    {
                        Save();
                    }
                    if (pauseStatus && key == ConsoleKey.D)
                    {
                        Set();
                        RenderMap();
                        RenderStats();
                    }
                }
                if (!pauseStatus)
                {
                    board.Advance();
                    RenderMap();
                    RenderStats();
                    Thread.Sleep(dataConfig.TimeSleep);
                }
            }
        }
    }
}

