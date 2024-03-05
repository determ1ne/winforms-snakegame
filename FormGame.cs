using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace SnakeGame
{
    public enum Direction { Left, Right, Up, Down }

    public partial class FormGame : Form
    {
        private int _xBlocks = 30;
        private int _yBlocks = 30;
        private List<CheckBox> _checkBoxes = new List<CheckBox>();
        private List<ValueTuple<int, int>> _snakeBody = new List<ValueTuple<int, int>>();
        private ValueTuple<int, int> _food = new ValueTuple<int, int>(-1, -1);
        private bool _gameStarted = false;
        private Direction _direction = Direction.Right;
        private Random _random = new Random();
        private int _score = 0;
        private RadioButton _foodButton;

        public FormGame()
        {
            InitializeComponent();
        }

        public void ReSize()
        {
            var dpiRatio = this.DeviceDpi / 96.0;
            this.Size = new Size((int)(dpiRatio * _xBlocks * 15) + 5, (int)(dpiRatio * _yBlocks * 15) + 40);
            var blocks = _xBlocks * _yBlocks;

            // remove all checkboxes
            for (var i = 0; i < _checkBoxes.Count; ++i)
            {
                this.Controls.Remove(_checkBoxes[i]);
            }

            for (int i = _checkBoxes.Count; i < blocks; ++i)
            {
                var checkBox = new CheckBox
                {
                    AutoCheck = false,
                    Text = "",
                    Size = new Size(22, 22),
                };
                _checkBoxes.Add(checkBox);
            }

            for (var ix = 0; ix < _xBlocks; ++ix)
            {
                for (var iy = 0; iy < _yBlocks; ++iy)
                {
                    var checkBox = _checkBoxes[ix * _yBlocks + iy];
                    checkBox.Location = new Point(ix * 22, iy * 22);
                    Controls.Add(checkBox);
                }
            }
        }

        private void FormGame_ResizeEnd(object sender, EventArgs e)
        {
            var xBlocksNew = (Size.Width - 5) / 15 * 96 / DeviceDpi;
            var yBlocksNew = (Size.Height - 40) / 15 * 96 / DeviceDpi;

            return;
            if (!_gameStarted && xBlocksNew != _xBlocks && yBlocksNew != _yBlocks)
            {
                ReSize();
            }
        }

        private void FormGame_Load(object sender, EventArgs e)
        {
            _foodButton = new RadioButton()
            {
                AutoCheck = false,
                Location = new Point(0, 0),
                Size = new Size(22, 22),
                Checked = true,
                Text = "",
                UseVisualStyleBackColor = true,
            };
            Controls.Add(_foodButton);
            ReSize();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (!_gameStarted) StartGame();
                else
                {
                    if (gameTimer.Enabled)
                    {
                        gameTimer.Stop();
                        Text = "Paused - Press [Enter] to resume";
                    }
                    else
                    {
                        gameTimer.Start();
                    }
                }
            }
            else if (keyData == Keys.Space)
            {
                NextFrame();
            }
            else if (keyData == Keys.Up && _direction != Direction.Down)
            {
                _direction = Direction.Up;
            }
            else if (keyData == Keys.Down && _direction != Direction.Up)
            {
                _direction = Direction.Down;
            }
            else if (keyData == Keys.Left && _direction != Direction.Right)
            {
                _direction = Direction.Left;
            }
            else if (keyData == Keys.Right && _direction != Direction.Left)
            {
                _direction = Direction.Right;
            }
            else if (keyData == Keys.Add)
            {
                gameTimer.Interval = Math.Max(20, gameTimer.Interval - 30);
            }
            else if (keyData == Keys.Subtract)
            {
                gameTimer.Interval = Math.Min(1000, gameTimer.Interval + 30);
            }

            return true;
        }

        private CheckBox CheckBoxByXy(int x, int y) => this._checkBoxes[x * _xBlocks + y];
        private CheckBox CheckBoxByXy(ValueTuple<int, int> xy) => CheckBoxByXy(xy.Item1, xy.Item2);

        private void GenerateFood()
        {
            int x;
            int y;
            while (true)
            {
                x = _random.Next(_xBlocks);
                y = _random.Next(_yBlocks);
                if (CheckBoxByXy(x, y).Checked == false)
                {
                    // found empty position
                    break;
                }
            }

            _food.Item1 = x;
            _food.Item2 = y;
            _foodButton.Location = new Point(x * 22, y * 22);
        }

        private void StartGame()
        {
            // Reset Game
            _foodButton.BringToFront();
            _snakeBody.Clear();
            _snakeBody.Add((_xBlocks / 2, _yBlocks / 2));
            _snakeBody.Add((_xBlocks / 2 + 1, _yBlocks / 2));
            _direction = Direction.Right;
            _score = 0;

            foreach (var control in Controls)
            {
                if (control is CheckBox ch)
                {
                    ch.Checked = false;
                }
            }
            foreach (var item in _snakeBody)
            {
                CheckBoxByXy(item).Checked = true;
            }

            GenerateFood();
            CheckBoxByXy(_food).CheckState = CheckState.Indeterminate;

            _gameStarted = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            gameTimer.Start();
        }

        private void StopGame()
        {
            _gameStarted = false;
            Text = $"Game End - Score: {_score} - Press [Enter] to start";
            MessageBox.Show($"Score: {_score}", "Game End", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void NextFrame()
        {
            if (!_gameStarted)
            {
                return;
            }

            var snakeTail = _snakeBody[0];
            var snakeHead = _snakeBody.Last();
            CheckBoxByXy(snakeHead).CheckState = CheckState.Checked;
            switch (_direction)
            {
                case Direction.Up:
                    _snakeBody.Add((snakeHead.Item1, snakeHead.Item2 - 1));
                    break;
                case Direction.Down:
                    _snakeBody.Add((snakeHead.Item1, snakeHead.Item2 + 1));
                    break;
                case Direction.Left:
                    _snakeBody.Add((snakeHead.Item1 - 1, snakeHead.Item2));
                    break;
                case Direction.Right:
                    _snakeBody.Add((snakeHead.Item1 + 1, snakeHead.Item2));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            snakeHead = _snakeBody.Last();
            if (snakeHead.Item1 == -1 || snakeHead.Item2 == -1 ||
                snakeHead.Item1 == _xBlocks || snakeHead.Item2 == _yBlocks)
            {
                StopGame();
                return;
            }
            for (var i = 0; i < _snakeBody.Count - 1; i++)
            {
                if (snakeHead.Item1 == _snakeBody[i].Item1 &&
                    snakeHead.Item2 == _snakeBody[i].Item2)
                {
                    StopGame();
                    return;
                }
            }

            // check head
            CheckBoxByXy(snakeHead).CheckState = CheckState.Indeterminate;

            // food detection
            if (snakeHead == _food)
            {
                _score++;
                GenerateFood();
                // CheckBoxByXy(_food).CheckState = CheckState.Indeterminate;
                CheckBoxByXy(snakeHead).CheckState = CheckState.Checked;
            }
            else
            {
                // uncheck tail
                _snakeBody.RemoveAt(0);
                CheckBoxByXy(snakeTail).Checked = false;
            }

            Text = $"Score: {_score} - Press [Enter] to pause";
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (_gameStarted == false)
            {
                ((Timer)sender).Stop();
                return;
            }
            NextFrame();
        }
    }
}
