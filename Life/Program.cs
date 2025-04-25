using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.IO;
using ScottPlot;


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
        public bool Advance()
        {
            IsAlive = IsAliveNext;
            return IsAlive;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;
        public uint Epoch;
        public uint cellAlive;
        private uint lastNumberAlive;
        public uint staticBoardSteps;
        public double liveDensity;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity, bool newBoard)
        {
            CellSize = cellSize;
            Epoch = 0;
            cellAlive = 0;
            staticBoardSteps = 0;
            this.liveDensity = liveDensity;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            if (newBoard)
                Randomize();
        }
        readonly Random rand = new Random();
        public void Randomize()
        {
            foreach (var cell in Cells)
            {
                cell.IsAlive = rand.NextDouble() < liveDensity;
                if (cell.IsAlive)
                    cellAlive++;
            }
        }

        public void Advance()
        {
            cellAlive = 0;
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
            {
                cell.Advance();
                if (cell.IsAlive)
                    cellAlive++;
            }

            if (cellAlive == lastNumberAlive)
            {
                staticBoardSteps++;
            }
            else
            {
                lastNumberAlive = cellAlive;
                staticBoardSteps = 0;
            }
            Epoch++;
        }
        public void SetMap(string[] partsMap)
        {
            for (int row = 0; row < Rows; row++)
                for (int col = 0; col < Columns; col++)
                    if (partsMap[row][col] == '*')
                    {
                        Cells[col, row].IsAlive = true;
                        cellAlive++;
                    }
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
        public uint CountFigures()
        {
            bool[,] visited = new bool[Columns, Rows];
            uint countFigures = 0;

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

                        while (queue.Count > 0)
                        {
                            var (x, y) = queue.Dequeue();

                            for (int k = 0; k < 8; k++)
                            {
                                int nx = x + dirX[k], ny = y + dirY[k];
                                (nx, ny) = Index.ChekIndex(nx, ny, Columns, Rows);

                                if (Cells[nx, ny].IsAlive && !visited[nx, ny])
                                {
                                    visited[nx, ny] = true;
                                    queue.Enqueue((nx, ny));
                                }
                            }
                        }
                    }
                }
            }
            return countFigures;
        }
        public bool StaticBoard()
        {
            if (staticBoardSteps > 10)
            {
                return true;
            }
            else return false;
        }
    }
    class Index
    {
        static public (int, int) ChekIndex(int x, int y, int xMax, int yMax)
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

    public class LoaderMap
    {
        static private string CreateStr(Board board)
        {
            int columns = board.Width;
            int rows = board.Height;

            var strBuilder = new StringBuilder();
            strBuilder.Append($"{columns}\n{rows}\n{board.CellSize}\n{board.liveDensity}\n");

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
            double liveDensity = double.Parse(parts[3]);

            Board board = new Board(width, height, cellSize, liveDensity, false);
            board.SetMap(parts[4..]);

            return board;
        }
        static public void SaveDataMap(Board board)
        {
            string newStr = $"{board.liveDensity} {board.Epoch}\n";
            File.AppendAllText("data.txt", newStr);
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

    public class TemplateCounter
    {
        static private SortedDictionary<int, string> namesTemplates = new SortedDictionary<int, string>
        {
            {440895, "Template one"},
            {5187, "Template two"},
            {1001, "Template three"},
            {88179, "Template four"},
            {25935, "Template five"},
            {462, "Template six"}
        };
        static private int sizeTemplate = 5;
        //  Шаблоны фигур
        /*  { 0, 1, 1 }     { 0, 1, 0 }     { 0, 0, 0 }     { 0, 1, 0 }     { 0, 1, 1 }     { 1, 1, 0 }
            { 1, 0, 1 }     { 1, 0, 1 }     { 1, 1, 1 }     { 1, 0, 1 }     { 1, 0, 1 }     { 1, 1, 0 }
            { 1, 1, 0 }     { 0, 1, 0 }     { 0, 0, 0 }     { 1, 1, 0 }     { 0, 1, 0 }     { 0, 0, 0 }
         */
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

                    (col, row) = Index.ChekIndex(col, row, maxCol, maxRow);

                    if (board.Cells[col, row].IsAlive)
                    {
                        int m = hashMatrix[i, j];
                        if (m == 27)
                            return -1;

                        hash *= m;
                    }
                }
            }
            return hash;
        }
        /**
        * Проверка заключается в вычислении хэша подматриц основного поля, использую матрицу простых чисел.
        * Вычисленный хэш используется для проверки на совпадения с сохранёнными хэшами шаблонов
        */
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
                        numberTemplates[namesTemplates[hashSubMatrix]]++;

                }
            }
            return numberTemplates;
        }
    }

    class PainterGraph
    {
        static public void CreateGraph()
        {
            var lines = File.ReadAllLines("data.txt")
                .Select(line => line.Split(' '))
                .Select(parts => new { X = double.Parse(parts[0]), Y = double.Parse(parts[1]) })
                .OrderBy(point => point.X)  // Сортировка по X
                .ToList();

            double[] xs = lines.Select(p => p.X).ToArray();
            double[] ys = lines.Select(p => p.Y).ToArray();

            var plt = new Plot(800, 600);
            plt.AddScatter(xs, ys);

            plt.Title("График перехода в стабильное состояние (числа поколений) от попыток случайного распределения с разной плотностью заполнения поля");
            plt.XLabel("Плотнсоть заполнения");
            plt.YLabel("Число покоений");
            plt.Legend();

            plt.SaveFig("plot.png");
        }
    }

    class Program
    {
        static Board board;
        static ConfigData config;
        static string configFile = "ConfigCLI.json";
        static public ConfigData GetConfig()
        {
            string json = File.ReadAllText(configFile);
            config = JsonSerializer.Deserialize<ConfigData>(json);
            return config;
        }
        static public Board Reset()
        {
            board = new Board(
                width: config.Width,
                height: config.Height,
                cellSize: config.CellSize,
                liveDensity: config.LiveDensity,
                newBoard: true);

            return board;
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
        static bool KeyControl(bool pauseStatus)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
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
                if (pauseStatus && key == ConsoleKey.A)
                {
                    LoaderMap.SaveDataMap(board);
                }
                if (pauseStatus && key == ConsoleKey.Escape)
                {
                    Reset();
                    RenderMap();
                    RenderStats();
                }
                if (pauseStatus && key == ConsoleKey.P)
                {
                    PainterGraph.CreateGraph();
                }
                if (key == ConsoleKey.Q)
                {
                    return pauseStatus ? false : true;
                }
            }
            return pauseStatus;
        }
        static bool StepEpoch(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                Console.Clear();
                Console.WriteLine("Q - pause; A - save board stats; S - save board; D - load board; P - create picture graphic\n");

                board.Advance();
                RenderMap();
                RenderStats();

                if (board.StaticBoard())
                {
                    pauseStatus = true;
                    Console.WriteLine("Static board.\n");
                }

                Thread.Sleep(config.TimeSleep);
            }
            return pauseStatus;
        }
        static void RenderStats()
        {
            var strBuilder = new StringBuilder();

            strBuilder.Append($"Count figures: {board.CountFigures()} ");
            strBuilder.Append($"Count alive: {board.cellAlive}\n");

            Dictionary<string, int> countTemplate = TemplateCounter.CountTemplates(board);
            foreach (var (name, count) in countTemplate)
            {
                strBuilder.Append($"Count {name}: {count} ");
            }
            strBuilder.AppendLine();
            strBuilder.Append($"Epoch: {board.Epoch}\n");

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
            Console.Write(strBuilder.ToString());
        }
        static void Main(string[] args)
        {
            GetConfig();
            Reset();

            Console.WriteLine("Press Q for unpause.");

            bool pauseStatus = true;
            while (true)
            {
                Thread.Sleep(30);
                pauseStatus = KeyControl(pauseStatus);
                pauseStatus = StepEpoch(pauseStatus);
            }
        }
    }
}

