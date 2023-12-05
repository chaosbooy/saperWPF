using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace saper_2._0
{
    public partial class MainWindow : Window
    {
        private bool gameEnded = true;
        private bool bombsCreated = false;
        private int NoBombNr = 0;
        private int flagNr = 0;
        public string fileName = "czas.txt";
        
        private Grid gameGrid = new Grid();
        private DockPanel menu = new DockPanel();
        private Grid exit = new Grid();
        private DispatcherTimer CountTime = new DispatcherTimer();
        private TimeSpan currTime = new TimeSpan();

        private List<Button> bombs = new List<Button>();
        private List<List<Button>> allGameObjects = new List<List<Button>>();

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(fileName))
                using (StreamReader reader = new StreamReader(fileName))
                    scoresVisual.Text = "Scores: " + Environment.NewLine + reader.ReadToEnd();
        }

        private void Button_Click(object sender, RoutedEventArgs ed)
        {
            Button btn = (Button)sender;
            string diff = btn.Content.ToString();

            lobby.Visibility = Visibility.Collapsed;
            scorePanel.Visibility = Visibility.Collapsed;

            switch (diff)
            {
                case "Easy":
                    Create_Game(8, 8);
                    break;
                case "Medium":
                    Create_Game(16, 16);
                    break;
                case "Hard":
                    Create_Game(32, 16);
                    break;
            }

            CreateMenu();
        }

        private void Create_Game(int x, int y)
        {
            if (x >= 32) this.Width = 1500;

            gameEnded = false;
            bombsCreated = false;
            gameGrid = new Grid();
            allGameObjects = new List<List<Button>>();

            gameGrid.Name = "gameGrid";
            gameGrid.Height = this.Height / 2;
            gameGrid.Width = this.Width / 2;

            for (int i = 0; i < x; i++)
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int i = 0; i < y; i++)
            {
                gameGrid.RowDefinitions.Add(new RowDefinition());
                allGameObjects.Add(new List<Button>());

                for (int j = 0; j < x; j++)
                {
                    Button next = new Button();
                    next.Name = "_" + i.ToString() + "_" + j.ToString();
                    next.Click += (object s, RoutedEventArgs e) => { Dig((Button)s); };
                    next.MouseRightButtonUp += Flag;
                    next.Content = string.Empty;
                    next.Margin = new Thickness(1);
                    gameGrid.Children.Add(next);

                    Grid.SetRow(next, i);
                    Grid.SetColumn(next, j);

                    allGameObjects[i].Add(next);
                }
            }
            this.SizeChanged += (object send, SizeChangedEventArgs e) =>
            {
                gameGrid.Height = this.Height / 2;
                gameGrid.Width = this.Width / 2;
            };

            main.Children.Add(gameGrid);

            Random rand = new Random();
            flagNr = 1; // rand.Next((x*y) / 5, (x * y) / 3);
            NoBombNr = (x * y) - flagNr;
        }

        private void CreateBombs(int[] position)
        {
            bombsCreated = true;

            Random rand = new Random();

            for(int i = 0; i < flagNr; i++)
            {
                int tmp = rand.Next(allGameObjects.Count * allGameObjects[0].Count);
                Button next = (Button)gameGrid.Children[tmp];

                string[] indexes = next.Name.Split('_');
                int[] nextPosition = new int[] { Convert.ToInt32(indexes[1]), Convert.ToInt32(indexes[2]) };

                if (bombs.Contains<Button>(next)) { i--; continue; }
                else if ((nextPosition[0] - 1 <= position[0] && nextPosition[0] + 1 >= position[0]) &&
                            nextPosition[1] - 1 <= position[1] && nextPosition[1] + 1 >= position[1]) { i--; continue; }
                bombs.Add(next);
            }
        }

        private void CreateMenu()
        {
            menu = new DockPanel();
            menu.VerticalAlignment = VerticalAlignment.Top;
            menu.HorizontalAlignment = HorizontalAlignment.Center;
            menu.Width = this.Width / 1.5;
            menu.Height = 100;
            menu.Background = new SolidColorBrush(Colors.Azure);
            menu.Margin = new Thickness(0, 20, 0, 0);

            FontFamilyConverter fontConverter = new FontFamilyConverter();
            Label flaggers = new Label();
            flaggers.Name = "flaggers";
            flaggers.Content = "🚩 = " + flagNr;
            flaggers.HorizontalAlignment = HorizontalAlignment.Left;
            flaggers.VerticalAlignment = VerticalAlignment.Center;
            flaggers.FontSize = 32;
            flaggers.Margin = new Thickness(50, 0, 50, 0);
            flaggers.FontFamily = (FontFamily) fontConverter.ConvertFromString("Berlin Sans FB");
            flaggers.Width = 150;

            Label timer = new Label();
            timer.Content = "your time: " + "00:00:00";
            timer.HorizontalAlignment = HorizontalAlignment.Center;
            timer.VerticalAlignment = VerticalAlignment.Center;
            timer.FontSize = 32;
            timer.FontFamily = (FontFamily)fontConverter.ConvertFromString("Berlin Sans FB");
            timer.Width = 300;

            CountTime = new DispatcherTimer();
            CountTime.Interval = new TimeSpan(0, 0, 1);
            CountTime.Start();
            currTime = new TimeSpan(0,0,0);
            CountTime.Tick += new EventHandler((object s, EventArgs e) => {
                currTime += new TimeSpan(0, 0, 1);
                timer.Content = "your time: " + currTime;
            });

            Button comeBack = new Button();
            comeBack.Content = "return";
            comeBack.HorizontalAlignment = HorizontalAlignment.Right;
            comeBack.Margin = new Thickness(0, 0, 30, 0);
            comeBack.FontSize = 20;
            comeBack.Background = new SolidColorBrush(Colors.Aquamarine);
            comeBack.VerticalAlignment = VerticalAlignment.Center;
            comeBack.Height = 50;
            comeBack.Width = 80;

            comeBack.Click += (object s, RoutedEventArgs e) =>
            {
                main.Children.Remove(menu);
                HideAll();
            };

            this.SizeChanged += (object send, SizeChangedEventArgs e) => 
            {
                menu.Width = this.Width / 1.5; 
            };

            menu.Children.Add(flaggers);
            menu.Children.Add(timer);
            menu.Children.Add(comeBack);
            main.Children.Add(menu);
        }

        private void Dig(Button s)
        {
            if (gameEnded) { return; }

            string[] indexes = s.Name.Split('_');
            int[] sPosition = new int[] { Convert.ToInt32(indexes[1]), Convert.ToInt32(indexes[2]) };

            if (s.Content.ToString() != string.Empty) return;
            else if (!bombsCreated) CreateBombs(sPosition);
            else if (bombs.Contains(s)) { EndGame(false); return; }

            int bombsAround = 0;
            for(int i = sPosition[0] - 1; i <= sPosition[0] + 1; i++)
                for(int j = sPosition[1] - 1; j <= sPosition[1] + 1; j++)
                {
                    if (i < 0 || j < 0 || i >= allGameObjects.Count || j >= allGameObjects[0].Count) 
                        continue;

                    if (bombs.Contains(allGameObjects[i][j]))
                        bombsAround++;
                }

            if (bombsAround == 0)
            {
                s.Background = new SolidColorBrush(Colors.Green);
                s.Content = "•";
                DigAround(sPosition);
            }
            else
            {
                s.Background = new SolidColorBrush(Colors.Yellow);
                s.Content = Convert.ToString(bombsAround);
            }

            if (--NoBombNr < 1) { EndGame(true); return; }
        }

        private void DigAround(int[] around)
        {
            for(int i = -1; i < 2; i++)
                for(int j = -1; j < 2; j++)
                {
                    int x = around[0] + i;
                    int y = around[1] + j;
                    //bez skosu dodaj: || i == j || i + j == 0
                    if (x < 0 || y < 0 || x >= allGameObjects.Count || y >= allGameObjects[0].Count)
                        continue;

                    Dig(allGameObjects[x][y]);
                }
        }

        private void Flag(object sender, EventArgs e)
        {
            Button s = sender as Button;
            Label flaggers = menu.Children.OfType<Label>().FirstOrDefault();

            if ((flagNr <= 0 && s.Content.ToString() == string.Empty) || !bombsCreated) return;
            else if (s.Content.ToString() == "🚩")
            {
                s.Content = string.Empty;
                flaggers.Content = "🚩 = " + ++flagNr;
            } else if (s.Content.ToString() == string.Empty)
            {
                s.Content = "🚩";
                flaggers.Content = "🚩 = " + --flagNr;
            }
            
        }

        private void EndGame(bool end)
        {
            gameEnded = true;

            exit = new Grid();
            exit.VerticalAlignment = VerticalAlignment.Bottom;
            exit.HorizontalAlignment = HorizontalAlignment.Center;

            Label finalText = new Label();
            finalText.FontSize = 60;
            finalText.FontWeight = FontWeights.Bold;
            finalText.HorizontalAlignment = HorizontalAlignment.Center;
            if (end)
            {
                finalText.Content = "Won!";
                finalText.Foreground = new SolidColorBrush(Colors.Green);

                foreach(Button b in bombs)
                {
                    b.FontSize = 20;
                    b.HorizontalContentAlignment = HorizontalAlignment.Center;
                    b.VerticalContentAlignment = VerticalAlignment.Center;
                    b.Content = "🌼";
                    b.Background = new SolidColorBrush(Colors.LightSteelBlue);
                }

                try
                {
                    string score = "Leon: " + CountTime.Dispatcher.ToString();
                    string allScores = string.Empty;
                    string thisScore = "Leon: " + currTime;

                    if(File.Exists(fileName))
                        using (StreamReader reader = new StreamReader(fileName))
                            allScores = reader.ReadToEnd() + Environment.NewLine;

                    using (StreamWriter writer = new StreamWriter(fileName))
                        if (allScores.Trim() != string.Empty)
                            writer.WriteLine(allScores + thisScore);
                        else
                            writer.WriteLine(thisScore);

                    scoresVisual.Text = allScores + thisScore;
                }
                catch
                {

                }
            }
            else
            {
                finalText.Content = "Lost!";
                finalText.Foreground = new SolidColorBrush(Colors.Red);

                foreach(Button b in bombs)
                {
                    b.FontSize = 20;
                    b.HorizontalContentAlignment = HorizontalAlignment.Center;
                    b.VerticalContentAlignment = VerticalAlignment.Center;
                    b.Content = "🤯";
                    b.Background = new SolidColorBrush(Colors.Red);
                }
            }

            Button restart = new Button();
            restart.FontSize = 30;
            restart.Content = "Restart";
            restart.Margin = new Thickness(0,0,300,10);
            restart.Height = 70;
            restart.Width = 100;
            restart.Click += (object s, RoutedEventArgs e) =>
            {
                HideAll();
                lobby.Visibility = Visibility.Collapsed;
                scorePanel.Visibility = Visibility.Collapsed;

                Create_Game(allGameObjects[0].Count, allGameObjects.Count);
                CreateMenu();
            };

            Button comeBack = new Button();
            comeBack.FontSize = 30;
            comeBack.Content = "Return";
            comeBack.Margin = new Thickness(300, 0, 0, 10);
            comeBack.Height = 70;
            comeBack.Width = 100;
            comeBack.Click += (object s, RoutedEventArgs e) => { HideAll(); };

            menu.Children.Remove(menu.Children[menu.Children.Count - 1]);
            CountTime.Stop();
            exit.Children.Add(restart);
            exit.Children.Add(finalText);
            exit.Children.Add(comeBack);

            main.Children.Add(exit);
        }

        private void HideAll()
        {
            main.Children.Remove(menu);
            main.Children.Remove(gameGrid);
            main.Children.Remove(exit);
            lobby.Visibility = Visibility.Visible;
            scorePanel.Visibility = Visibility.Visible;
            this.Width = this.MinWidth;
            this.Height = this.MinHeight;
        }
    }
}