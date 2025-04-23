using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.IO;

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

        static void Render()
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
                Render();
                Thread.Sleep(20);

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
                        Render();
                    }
                }
                if (!pauseStatus)
                {
                    Thread.Sleep(dataConfig.TimeSleep);
                    board.Advance();
                }
            }
        }
    }
}