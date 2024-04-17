using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonogameTicTacToe;

enum GameState { Running, Won, Lost, Draw }

enum Cell { X, O }

public class TicTacToeGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _horizontalLine;
    private Texture2D _verticalLine;
    private Texture2D _charX;
    private Texture2D _charO;
    private SpriteFont _font;
    private MouseState _currentMouseState;
    private MouseState _previousMouseState;
    private Random _rand = new Random();

    private const int WIDTH = 600;
    private const int HEIGHT = 600;
    private readonly List<Cell?> _cells;
    private readonly List<Vector2> _cellStart = new()
    {
        new Vector2(0, 0), new Vector2(200, 0), new Vector2(400, 0),
        new Vector2(0, 200), new Vector2(200, 200), new Vector2(400, 200),
        new Vector2(0, 400), new Vector2(200, 400), new Vector2(400, 400)
    };
    private GameState _state = GameState.Running;

    public TicTacToeGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = WIDTH;
        _graphics.PreferredBackBufferHeight = HEIGHT;

        _cells = Enumerable.Repeat<Cell?>(null, 9).ToList();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here

        _horizontalLine = new Texture2D(GraphicsDevice, WIDTH, 1, false, SurfaceFormat.Color);
        _horizontalLine.SetData(Enumerable.Repeat<Color>(Color.White, WIDTH).ToArray());

        _verticalLine = new Texture2D(GraphicsDevice, 1, HEIGHT, false, SurfaceFormat.Color);
        _verticalLine.SetData(Enumerable.Repeat<Color>(Color.White, WIDTH).ToArray());

        _charX = Content.Load<Texture2D>("X");
        _charO = Content.Load<Texture2D>("O");

        _font = Content.Load<SpriteFont>("Lib");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();    

        if (_state == GameState.Running && _currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            var cell = GetClickedCell(_currentMouseState.X, _currentMouseState.Y);
            if(cell != null && _cells[cell.Value] == null)
            {
                _cells[cell.Value] = Cell.X;
            }

            CheckIfGameOver();
            MakeAiMove();
            CheckIfGameOver();

            Console.WriteLine($"Left mouse button was just pressed X:{_currentMouseState.X} Y:{_currentMouseState.Y} {cell}");
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        DrawGame();
        if(_state != GameState.Running) DrawFinished();

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawFinished()
    {
        var statusStr = _state switch 
        {
            GameState.Won => "You won!",
            GameState.Lost => "You lost!",
            GameState.Draw => "Draw!",
            _ => throw new ArgumentException()
        };

        var size = _font.MeasureString(statusStr);
        var x = (WIDTH - size.X) / 2;
        var y = (HEIGHT - size.Y) / 2;

        _spriteBatch.DrawString(_font, statusStr, new Vector2(x, y), Color.Black);
    }

    private void DrawGame()
    {
        // Grid
        _spriteBatch.Draw(_horizontalLine, new Vector2(0, 200), null, Color.White,0,Vector2.Zero,Vector2.One,SpriteEffects.None,0);
        _spriteBatch.Draw(_horizontalLine, new Vector2(0, 400), null, Color.White,0,Vector2.Zero,Vector2.One,SpriteEffects.None,0);
        _spriteBatch.Draw(_verticalLine, new Vector2(200, 0), null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        _spriteBatch.Draw(_verticalLine, new Vector2(400, 0), null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);

        //Players
        for (int i = 0; i < _cells.Count; i++)
        {
            if(_cells[i] == null) continue;

            if(_cells[i] == Cell.X)
            {
                _spriteBatch.Draw(_charX, _cellStart[i], Color.White);
            }
            else if(_cells[i] == Cell.O)
            {
                _spriteBatch.Draw(_charO, _cellStart[i], Color.White);
            }
        }
    }

    private int? GetClickedCell(int x, int y)
    {
        if(x < 0 || y < 0 || x > WIDTH || y > HEIGHT) return null;

        int? cell = (X: x, Y: y) switch 
        {
            { X: <= 200 } and { Y: <= 200 } => 0,
            { X: > 200 } and { X: <= 400 } and { Y: <= 200 } => 1,
            { X: > 400 } and { Y: <= 200 } => 2,
            { X: <= 200 } and { Y: > 200 } and { Y: <= 400 } => 3,
            { X: > 200 } and { X: <= 400 } and { Y: > 200 } and { Y: <= 400 } => 4,
            { X: > 400 } and { Y: > 200 } and { Y: <= 400 } => 5,
            { X: <= 200 } and { Y: > 400 } => 6,
            { X: > 200 } and { X: <= 400 } and { Y: > 400 } => 7,
            { X: > 400 } and { Y: > 400 } => 8,
        };

        return cell;
    }

    private void MakeAiMove()
    {
        var freeCellsIdx = new List<int>();
        for(int i = 0; i < _cells.Count; i++)
        {
            if (_cells[i] == null) freeCellsIdx.Add(i);
        }

        if(!freeCellsIdx.Any()) return;

        var opponendIdx = freeCellsIdx[_rand.Next(0,freeCellsIdx.Count-1)];
        _cells[opponendIdx] = Cell.O;
    }

    private void CheckIfGameOver()
    {
        List<List<int>> winningCombinations = new() { 
            new() { 0, 1, 2 }, new() { 3, 4, 5 }, new() { 6, 7, 8 },
            new() { 0, 3, 6 }, new() { 1, 4, 7 }, new() { 2, 5, 8 } };

        foreach (var w in winningCombinations) 
        {
            var hasPlayerWon = w.All(c => _cells[c] == Cell.X);

            if(hasPlayerWon)
            {
                _state = GameState.Won;
                return;
            }

            var hasAiWon = w.All(c => _cells[c] == Cell.O);

            if(hasAiWon)
            {
                _state = GameState.Lost;
                return;
            }
        }

        //Draw
        if(_cells.All(c => c != null)) _state = GameState.Draw;
    }
}

