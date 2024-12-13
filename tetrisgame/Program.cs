using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using System.Media;


namespace tetris
{
    class Program
    {
        //Settings 
        static int TetrisRows = 21; //Số hàng trong bảng Tetris
        static int TetrisCols = 10; //Số cột trong bảng Tetris
        static int InfoCols = 20; //Chiều rộng khu vực thông tin
        static int ConsoleRows = 1 + TetrisRows + 1; //Tổng số hàng console
        static int ConsoleCols = 1 + TetrisCols + 1 + InfoCols + 1; //Tổng số cột console
        static List<bool[,]> TetrisFigures = new List<bool[,]>()
        {
            new bool [,] // I ----
            {
                {true, true, true, true }
            },
            new bool [,] // O
            {
                {true, true  },
                {true, true  }
            },
            new bool [,] // T
            {
                {false, true, false},
                {true, true ,true }
            },
            new bool [,] // S
            {
                {false, true, true},
                {true, true, false}
            },
            new bool[,] // Z
            {
                {true, true, false},
                {false, true, true}
            },
            new bool[,] // J
            {
                {false, false, true},
                {true, true, true}
            },
            new bool[,] // L
            {
                {true, false, false},
                {true, true, true}
            }
        };
        static string ScoresFileName = "scores.txt";
        static int[] ScorePerLines = { 0, 40, 100, 300, 1200 };
        //State
        static int HighScore; //Điểm cao nhất

        static int Score = 0; //Điểm hiện tại
        static int Frame = 0; //Số khung hình đã qua
        static int Level = 1; //Cấp độ hiện tại
        static string FigureSymbol = "*";
        static int FrameToMoveFigure = 16;
        static bool[,] CurrentFigure = null;
        static int CurrentFigureRow = 0;
        static int CurrentFigureCol = 0;
        static bool[,] NextFigure = null;
        static int NextFigureRow = 14;
        static int NextFigureCol = TetrisCols + 3;
        static bool[,] TetrisField = new bool[TetrisRows, TetrisCols];
        static Random Random = new Random();
        static bool PauseMode = false;
        static bool PlayGame = true;

        static void Main(string[] args)
        {
            HighScore = GetHighScoreFromFile();
            Console.WriteLine("Welcome to Tetris Console Game by Group 6.\n");
            Console.Write("Do you wanna play music (Y/N): ");
            string music = Console.ReadLine();
            if (music == "y" || music == "Y")
                PlayGame = true;
            else PlayGame = false;
            PlayConsoleMusic();
            
            if (File.Exists(ScoresFileName))
            {
                try
                {
                    var allScore = File.ReadAllLines(ScoresFileName);
                    foreach (var score in allScore)
                    {
                        var match = Regex.Match(score, @" =>(?<score>[0-9]+)");
                        HighScore = Math.Max(HighScore, int.Parse(match.Groups["score"].Value));
                    }
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine("Error reading the score file: " + ioEx.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unknown error: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Score file does not exist");
            }


            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Title = "Tetris Game by group 6";
            Console.CursorVisible = false;
            Console.WindowHeight = ConsoleRows;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows;
            Console.BufferWidth = ConsoleCols;
            CurrentFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];


            while (true)
            {
                Frame++;
                UpdateLevel();


                // Read user input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Spacebar && PauseMode == false)
                    {
                        PlayGame = false;
                        //PlayConsoleMusic();
                        PauseMode = true;
                        Write("╔═══════════════╗", 5, 5);
                        Write("║               ║", 6, 5);
                        Write("║     Pause     ║", 7, 5);
                        Write("║               ║", 8, 5);
                        Write("╚═══════════════╝", 9, 5);
                        //stop playing music after pause
                        Console.ReadKey();
                    }


                    if (key.Key == ConsoleKey.Spacebar && PauseMode == true)
                    {
                        PauseMode = false;
                        PlayGame = true;
                        PlayConsoleMusic();
                    }


                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }


                    if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)
                    {


                        if (CurrentFigureCol >= 1)
                        {
                            CurrentFigureCol--;
                        }


                    }


                    if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
                    {
                        if ((CurrentFigureCol < TetrisCols - CurrentFigure.GetLength(1)))
                        {
                            CurrentFigureCol++;
                        }
                    }


                    if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)
                    {
                        RotateCurrentFigure();
                    }


                    if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
                    {
                        Frame = 1;
                        Score += Level;
                        CurrentFigureRow++;
                    }
                    if (key.Key == ConsoleKey.K)
                    {
                        DropToBottom();
                    }
                }

                //Update the game state
                if (Frame % (FrameToMoveFigure - Level) == 0)
                {
                    CurrentFigureRow++;
                    Frame = 0;
                    Score++;
                }
                // user input
                // change state
                if (Collision(CurrentFigure))
                {
                    AddCurrentFigureToTetrisField();
                    int lines = CheckForFullLines();
                    //add points to score
                    Score += ScorePerLines[lines] * Level;
                    //CurrentFigure = NextFigure;
                    CurrentFigureCol = 0;
                    CurrentFigureRow = 0;


                    //game over!
                    if (Collision(CurrentFigure))
                    {
                        var scoreAsString = Score.ToString();
                        scoreAsString += new string(' ', 7 - scoreAsString.Length);
                        try
                        {
                            if (Score > HighScore)
                            {
                                HighScore = Score; // Cập nhật HighScore nếu Score lớn hơn.
                                SaveHighScoreToFile(HighScore); // Ghi HighScore mới vào file.
                                Write("╔══════════════╗", 5, 5);
                                Write("║  New        ║", 6, 5);
                                Write("║  High Score! ║", 7, 5);
                                Write($"║      {scoreAsString} ║", 8, 5);
                                Write("╚══════════════╝", 9, 5);
                                PlayGame = false;
                                PlayMusic("tetris_gameover.wav");
                                Thread.Sleep(4000);
                            }


                            File.AppendAllLines(ScoresFileName, new List<string>
        {
            $"[{DateTime.Now.ToString()}] {Environment.UserName} => {Score}"
        });
                        }
                        catch (IOException ioEx)
                        {
                            Console.WriteLine("Error writing the score to the file: " + ioEx.Message);
                        }
                        catch (UnauthorizedAccessException uaeEx)
                        {
                            Console.WriteLine("No permission to write to the file: " + uaeEx.Message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Unknown error when writing to the file: " + ex.Message);
                        }

                        Write("╔══════════════╗", 5, 5);
                        Write("║  Game        ║", 6, 5);
                        Write("║     over!    ║", 7, 5);
                        Write($"║      {scoreAsString} ║", 8, 5);
                        Write("╚══════════════╝", 9, 5);

                        Thread.Sleep(1000000);
                        return;
                    }
                }

                //Redraw Ui  
                DrawBorder();
                DrawInfo();
                DrawTetrisField();
                DrawShadow();
                DrawCurrentFigure();
                //wait 40 miliseconds
                Thread.Sleep(40);
            }
        }
        // Đọc điểm cao nhất từ tệp khi chương trình bắt đầu
        private static int GetHighScoreFromFile()
        {
            int highScore = 0;
            if (File.Exists(ScoresFileName))
            {
                try
                {
                    var allScores = File.ReadAllLines(ScoresFileName);
                    // Kiểm tra từng dòng để lấy điểm cao nhất
                    foreach (var score in allScores)
                    {
                        var match = Regex.Match(score, @" => (?<score>[0-9]+)");
                        if (match.Success)
                        {
                            highScore = Math.Max(highScore, int.Parse(match.Groups["score"].Value));
                        }
                    }
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine("Error reading the score file: " + ioEx.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unknown error when reading the score file: " + ex.Message);
                }
            }

            return highScore;
        }

        // Lưu điểm cao nhất vào tệp
        private static void SaveHighScoreToFile(int highScore)
        {
            try
            {
                // Cập nhật lại điểm cao nhất vào tệp
                File.WriteAllText(ScoresFileName, $" => {highScore}");
            }
            catch (IOException ioEx)
            {
                Console.WriteLine("Error writing to the score file: " + ioEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown error when writing to the score file: " + ex.Message);
            }
        }
        private static bool[,] GetNextFigure()
        {
            NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)]; //Chọn ngẫu nhiên 1 khối trong số khối TetrisFigure để làm khối tiếp theo rơi xuống, gán khối đó cho mảng NextFigure
            return NextFigure; //Trả về NextFigure.
        }


        private static void UpdateLevel()
        {
            if (Score <= 0)
            {
                Level = 1;
            }


            if (Score >= 1000)
            {
                Level = 2;
            }
            else if (Score == 5000)
            {
                Level = 3;
            }
            else if (Score == 10000)
            {
                Level = 4;
            }
            else if (Score == 20000)
            {
                Level = 5;
            }
            else if (Score == 50000)
            {
                Level = 6;
            }
            else if (Score == 100000)
            {
                Level = 7;
            }
            else if (Score == 250000)
            {
                Level = 8;
            }
            else if (Score == 500000)
            {
                Level = 9;
            }
            else if (Score == 1000000)
            {
                Level = 10;
            }
        }


        private static void RotateCurrentFigure()
        {
            var newFigure = new bool[CurrentFigure.GetLength(1), CurrentFigure.GetLength(0)]; //mảng mới newFigure xoay CurrentFigure 90 độ theo chiều kim đồng hồ, số dòng của newFigure là số cột của CurrentFigure và số cột của newFigure là số dòng của CurrentFigure.
            for (int row = 0; row < CurrentFigure.GetLength(0); row++) //Duyệt qua từng hàng của mảng CurrentFigure.
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++) //Duyệt qua từng cột trong hàng thứ row của mảng CurrentFigure.
                {
                    newFigure[col, CurrentFigure.GetLength(0) - row - 1] = CurrentFigure[row, col]; //Mỗi ô của CurrentFigure được đặt lại theo quy tắc xoay 90 độ trong newFigure: hàng ban đầu trở thành cột mới, cột ban đầu trở thành hàng mới theo công thức: số hàng ban đầu - row - 1.
                }
            }


            if (!Collision(newFigure)) //Kiểm tra xem newFigure có va chạm với đối tượng khác trên lưới không, nếu không có va chạm, cập nhật CurrentFigure thành newFigure để áp dụng khối đã xoay.
            {
                CurrentFigure = newFigure;
            }
        }

        private static int CheckForFullLines() //0,1,2,3,4
        {
            int lines = 0; //Khai báo và khởi tạo biến lines, dùng để đếm số hàng đã được xóa


            for (int row = 0; row < TetrisField.GetLength(0); row++) //Duyệt qua từng hàng của mảng TetrisField.
            {
                bool rowIsFull = true; //Khai báo và khởi tạo biến rowIsFull, dùng để xác định xem hàng có đầy đủ các ô (tất cả các ô đều true) hay không.
                for (int col = 0; col < TetrisField.GetLength(1); col++) //Duyệt qua từng cột trong hàng thứ row của mảng TetrisField.
                {
                    if (!TetrisField[row, col]) //Nếu một ô TetrisField[row, col] có giá trị false (trống) thì:
                    {
                        rowIsFull = false; //Hàng không đầy đủ, gán rowIsFull = false.
                        break; // Thoát khỏi vòng lặp.
                    }
                }


                if (rowIsFull) // Nếu rowIsFull là true, tức là hàng đầy đủ, thì tiến hành:
                {
                    for (int rowToMove = row; rowToMove >= 1; rowToMove--) //Duyệt từ hàng hiện tại 
                    {
                        for (int col = 0; col < TetrisField.GetLength(1); col++)
                        {
                            TetrisField[rowToMove, col] = TetrisField[rowToMove - 1, col];
                        }
                    }


                    lines++;
                }
            }
            if (lines > 0)
            {
                new Thread(() =>
                {
                    PlayMusic("tetris_clear.wav");
                }).Start();
            } 
                
            return lines;
        }


        private static void AddCurrentFigureToTetrisField()
        {
            for (int row = 0; row < CurrentFigure.GetLength(0); row++)
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    if (CurrentFigure[row, col])
                    {
                        TetrisField[CurrentFigureRow + row, CurrentFigureCol + col] = true;
                    }
                }
            }


            CurrentFigure = NextFigure;
            GetNextFigure();
        }


        static bool Collision(bool[,] figure)
        {
            //CHEK for right outsite border 
            if (CurrentFigureCol > TetrisCols - figure.GetLength(1))
            {
                return true;
            }
            //CHEK for down outsite border 
            if (CurrentFigureRow + figure.GetLength(0) == TetrisRows)
            {
                return true;
            }
            //CHEK FOR COLLISUM DOWN
            for (int row = 0; row < figure.GetLength(0); row++)
            {
                for (int col = 0; col < figure.GetLength(1); col++)
                {
                    if (figure[row, col] && TetrisField[CurrentFigureRow + row + 1, CurrentFigureCol + col])
                    {
                        return true;
                    }
                }
            }


            return false;
        }
        static bool CollisionShadow(bool[,] figure, int testRow, int testCol)
        {
            if (testCol > TetrisCols - figure.GetLength(1) || testCol < 0)
            {
                return true;
            }
            if (testRow + figure.GetLength(0) > TetrisRows)
            {
                return true;
            }
            for (int row = 0; row < figure.GetLength(0); row++)
            {
                for (int col = 0; col < figure.GetLength(1); col++)
                {
                    if (figure[row, col] && TetrisField[testRow + row, testCol + col])
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        static int GetDropPosition()
        {
            int shadowRow = CurrentFigureRow;
            while (!CollisionShadow(CurrentFigure, shadowRow + 1, CurrentFigureCol))
            {
                shadowRow++;
            }
            return shadowRow;
        }
        static void DrawShadow()
        {
            int shadowRow = GetDropPosition();
            for (int row = 0; row < CurrentFigure.GetLength(0); row++)
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    if (CurrentFigure[row, col])
                    {
                        Write(".", row + shadowRow + 1, col + CurrentFigureCol + 1);
                    }
                }
            }
        }


        static void DropToBottom()
        {
            // Try to move the figure to the bottom row, checking for collision at each step
            while (!Collision(CurrentFigure))
            {
                CurrentFigureRow++; // Move the figure down
            }


            // Add the figure to the Tetris field at its final position
            AddCurrentFigureToTetrisField();

            // After dropping the figure, check for full lines
            int lines = CheckForFullLines();

            // Add points to score based on the number of full lines
            Score += ScorePerLines[lines] * Level;

            // Generate a new figure for the next round
            CurrentFigure = NextFigure;
            NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            CurrentFigureRow = 0;
        }


        static void DrawInfo()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
            }


            Write("Level:", 1, TetrisCols + 3);
            Write(Level.ToString(), 3, TetrisCols + 3);
            Write("Score:", 5, TetrisCols + 3);
            Write(Score.ToString(), 7, TetrisCols + 3);
            Write("High Score:", 9, TetrisCols + 3);
            Write(HighScore.ToString(), 11, TetrisCols + 3);
            Write("Next figure:", 13, TetrisCols + 3);


            DrawNextFigure();


            //Write("Frame:", 13, TetrisCols + 3);
            //Write(Frame.ToString(), 14, TetrisCols + 3);
            //Write("Position:", 15, TetrisCols + 3);
            //Write($"{CurrentFigureCol}, {CurrentFigureCol}", 16, TetrisCols + 3);
            Write("Keys:", 18, TetrisCols + 3);
            Write("  ^  ", 19, TetrisCols + 3);
            Write("<   >", 20, TetrisCols + 3);
            Write("  v ", 21, TetrisCols + 3);
            Write("Pause:", 18, TetrisCols + 13);
            Write("space", 20, TetrisCols + 13);
        }


        static void DrawTetrisField()
        {


            for (int row = 0; row < TetrisField.GetLength(0); row++)
            {
                string line = "";
                for (int col = 0; col < TetrisField.GetLength(1); col++)
                {
                    if (TetrisField[row, col])
                    {
                        line += $"{FigureSymbol}";
                    }
                    else
                    {
                        line += " ";
                    }
                }
                //+1 for the border
                Write(line, row + 1, 1);
            }
        }


        static void DrawCurrentFigure()
        {


            for (int row = 0; row < CurrentFigure.GetLength(0); row++)
            {


                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    if (CurrentFigure[row, col])
                    {
                        Write($"{FigureSymbol}", row + 1 + CurrentFigureRow, col + 1 + CurrentFigureCol);
                    }
                }
            }
        }


        static void DrawNextFigure()
        {


            for (int row = 0; row < NextFigure.GetLength(0); row++)
            {


                for (int col = 0; col < NextFigure.GetLength(1); col++)
                {
                    if (NextFigure[row, col])
                    {
                        Write($"{FigureSymbol}", row + 1 + NextFigureRow, col + 1 + NextFigureCol);
                    }
                }
            }
        }


        static void DrawBorder()
        {
            //always start drawing border from point (0,0);
            Console.SetCursorPosition(0, 0);


            //drawing border
            string firstLine = "╔";
            firstLine += new string('═', TetrisCols);
            firstLine += "╦";
            firstLine += new string('═', InfoCols);
            firstLine += "╗";


            string middleLine = "";
            for (int i = 0; i < TetrisRows; i++)
            {
                middleLine += "║";
                middleLine += new string(' ', TetrisCols) + "║" + new string(' ', InfoCols) + "║" + "\n";
            }


            string endLine = "╚";
            endLine += new string('═', TetrisCols);
            endLine += "╩";
            endLine += new string('═', InfoCols);
            endLine += "╝";


            string borderFrame = firstLine + "\n" + middleLine + endLine;
            Console.Write(borderFrame);
        }


        static void Write(string text, int row, int col)
        {
            Console.SetCursorPosition(col, row);
            Console.Write(text);
        }

        private static void PlayConsoleMusic()
        {
            if (PauseMode==false)
            {
                new Thread(() =>
                {
                    if (PlayGame)
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            var gameMusic = new SoundPlayer();
                            if (PlayGame)
                            {
                                gameMusic.SoundLocation = "tetris_m.wav";
                                gameMusic.PlaySync();
                            }
                            else gameMusic.Stop();
                        }
                        
                    }
                }).Start();
            }
        }
        static void PlayMusic (string songurl)
        {
            if (OperatingSystem.IsWindows())
            {
                var myPlayer = new SoundPlayer();
                myPlayer.SoundLocation = songurl;
                myPlayer.Play();
            }
        }
    }
}