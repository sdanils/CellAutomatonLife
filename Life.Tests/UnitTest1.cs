using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.IO;
using System.Dynamic;
using ScottPlot;

using cli_life;
namespace Life.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestCountFigureOne()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/all_template.txt");
        uint numFigures = board.CountFigures();
        uint expected = 10;
        Assert.AreEqual(expected, numFigures);
    }
    [TestMethod]
    public void TestCountFigureTwo()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/bant_ship.txt");
        uint numFigures = board.CountFigures();
        uint expected = 9;
        Assert.AreEqual(expected, numFigures);
    }
    [TestMethod]
    public void TestCountFigureThree()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/period_gun2.txt");
        uint numFigures = board.CountFigures();
        uint expected = 10;
        Assert.AreEqual(expected, numFigures);
    }
    [TestMethod]
    public void TestCountAliveOne()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/period_gun2.txt");
        uint expected = 38;
        Assert.AreEqual(expected, board.cellAlive);
    }
    [TestMethod]
    public void TestCountAliveTwo()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/bant_ship.txt");
        uint expected = 40;
        Assert.AreEqual(expected, board.cellAlive);
    }
    [TestMethod]
    public void TestCountAliveThree()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/all_template.txt");
        uint expected = 52;
        Assert.AreEqual(expected, board.cellAlive);
    }
    [TestMethod]
    public void TestCountTemplateOne()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/all_template.txt");
        Dictionary<string, int> numTemplates = TemplateCounter.CountTemplates(board);
        foreach (var (name, count) in numTemplates)
        {
            int expected = 1;
            Assert.AreEqual(expected, count);
        }

    }
    [TestMethod]
    public void TestCountTemplateTwo()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/without_template.txt");
        Dictionary<string, int> numTemplates = TemplateCounter.CountTemplates(board);
        foreach (var (name, count) in numTemplates)
        {
            int expected = 0;
            Assert.AreEqual(expected, count);
        }
    }
    [TestMethod]
    public void TestStaticBoardOne()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/static_board1.txt");
        uint cellAlive = board.cellAlive;
        for (int i = 0; i < 10; i++)
        {
            board.Advance();
            Assert.AreEqual(cellAlive, board.cellAlive);
        }

    }
    [TestMethod]
    public void TestStaticBoardTwo()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/static_board2.txt");
        uint cellAlive = board.cellAlive;
        for (int i = 0; i < 10; i++)
        {
            board.Advance();
            Assert.AreEqual(cellAlive, board.cellAlive);
        }
    }
    [TestMethod]
    public void TestEvalutionOne()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/evolution1.txt");
        uint expected = board.cellAlive - 1;
        board.Advance();
        Assert.AreEqual(expected, board.cellAlive);
    }
    [TestMethod]
    public void TestEvalutionTwo()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/evolution2.txt");
        uint expected = board.cellAlive - 2;
        board.Advance();
        Assert.AreEqual(expected, board.cellAlive);
    }
    [TestMethod]
    public void TestEvalutionThree()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/evolution3.txt");
        uint expected = board.cellAlive + 4;
        board.Advance();
        Assert.AreEqual(expected, board.cellAlive);
    }
    [TestMethod]
    public void TestEvalutionFour()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/evolution4.txt");
        uint expected = board.cellAlive + 5;
        board.Advance();
        Assert.AreEqual(expected, board.cellAlive);
    }
    [TestMethod]
    public void TestEvalutionFive()
    {
        Board board = LoaderMap.LoadMap("../../../../test_map/evolution4.txt");
        uint expected = board.cellAlive + 5;
        board.Advance();
        Assert.AreEqual(expected, board.cellAlive);
        board.Advance();
        expected -= 4;
        Assert.AreEqual(expected, board.cellAlive);
    }
}