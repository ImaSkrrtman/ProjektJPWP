using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using SeriousGame.Properties;
using System.Drawing.Configuration;

namespace SeriousGame
{
    public partial class Form1 : Form
    {
        // Zmienne gry
        private System.Windows.Forms.Timer gameTimer;
        private int playerX = 600; // Pozycja statku gracza
        private int playerY = 900;
        private Rectangle enemy;
        private int enemyX = 300; // Pozycja pocz¹tkowa przeciwnika
        private int enemyHP;
        private int enemyMaxHP;
        private int enemySpeed = 5;
        private Random random = new Random();
        private string mathQuestion; // Rzeczywista zagadka do pokazania w okienku
        private string displayedQuestion; // Ukryta forma zagadki do interfejsu
        private int mathAnswer;
        private int score = 0;
        private int enemiesLeft;
        private string difficulty = "easy"; // Domyœlnie ³atwy poziom
        private int lives;
        private int attackDamage = 10; // Obra¿enia gracza
        private int currentEnemyIndex = 1; // Aktualny przeciwnik
        private bool gameRunning = false;
        private bool canShoot = true; // Blokada strzelania wielokrotnego

        // Strzelanie
        private List<Rectangle> bullets = new List<Rectangle>();

        // Gwiazdy
        private List<Point> stars = new List<Point>();
        private int starSpeed = 2;

        // Planety
        private List<Planet> planets = new List<Planet>();
        private int planetSpeed = 1;

        // Licznik czasu gry
        private int elapsedTime = 0;

        public Form1(string selectedDifficulty)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ClientSize = new Size(1280, 1024);
            this.Text = "Space Math Invaders";
            difficulty = selectedDifficulty;

            // Ustawienia trudnoœci
            SetDifficulty();

            // Timer gry
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 20;
            gameTimer.Tick += GameLoop;

            // Dodanie obs³ugi klawiatury
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            // Generowanie gwiazd i planet
            GenerateStars();
            GeneratePlanets();

            StartGame();
        }

        private void SetDifficulty()
        {
            switch (difficulty.ToLower())
            {
                case "easy":
                    enemiesLeft = 3;
                    enemyMaxHP = 20;
                    lives = 3;
                    break;
                case "medium":
                    enemiesLeft = 5;
                    enemyMaxHP = 30;
                    lives = 2;
                    break;
                case "hard":
                    enemiesLeft = 7;
                    enemyMaxHP = 50;
                    lives = 1;
                    break;
            }
        }

        private void StartGame()
        {
            enemyHP = enemyMaxHP;
            enemy = new Rectangle(enemyX, 100, 150, 50); // Prostok¹t przeciwnika
            GenerateMathQuestion();
            gameRunning = true;
            gameTimer.Start();
        }

        private void GenerateMathQuestion()
        {
            int a = random.Next(1, 20);
            int b = random.Next(1, 20);

            switch (difficulty.ToLower())
            {
                case "easy":
                    displayedQuestion = "X + Y";
                    mathQuestion = $"{a} + {b}";
                    mathAnswer = a + b;
                    break;
                case "medium":
                    if (random.Next(0, 2) == 0) // Losowo dodawanie lub odejmowanie
                    {
                        displayedQuestion = "X - Y";
                        mathQuestion = $"{a} - {b}";
                        mathAnswer = a - b;
                    }
                    else
                    {
                        displayedQuestion = "X * Y";
                        mathQuestion = $"{a} * {b}";
                        mathAnswer = a * b;
                    }
                    break;
                case "hard":
                    while (a % b != 0) b = random.Next(1, 20); // Zapewnienie dzielenia z wynikiem ca³kowitym
                    displayedQuestion = "X / Y";
                    mathQuestion = $"{a} / {b}";
                    mathAnswer = a / b;
                    break;
            }
        }

        private void GenerateStars()
        {
            for (int i = 0; i < 100; i++)
            {
                stars.Add(new Point(random.Next(0, this.ClientSize.Width), random.Next(0, this.ClientSize.Height)));
            }
        }

        private void GeneratePlanets()
        {
            while (planets.Count < 5)
            {
                int planetX = random.Next(0, this.ClientSize.Width);
                int planetY = random.Next(0, this.ClientSize.Height);

                // Sprawdzenie, czy planeta nie jest w linii statku kosmicznego
                if (planetX >= playerX - 40 && planetX <= playerX + 120) // Zakres X statku
                {
                    continue; // Pomiñ tê pozycjê i wygeneruj now¹ planetê
                }

                var newPlanet = new Planet
                {
                    Position = new Point(planetX, planetY),
                    Size = random.Next(50, 100),
                    Color = Color.FromArgb(random.Next(100, 256), random.Next(100, 256), random.Next(100, 256))
                };

                // Sprawdzanie, czy planety nie nachodz¹ na siebie
                bool overlaps = false;
                foreach (var planet in planets)
                {
                    if (Math.Sqrt(Math.Pow(newPlanet.Position.X - planet.Position.X, 2) +
                                  Math.Pow(newPlanet.Position.Y - planet.Position.Y, 2)) < (newPlanet.Size + planet.Size) / 2)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    planets.Add(newPlanet);
                }
            }
        }


        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            // Poruszanie statkiem
            if (e.KeyCode == Keys.Left && playerX > 10) playerX -= 20;
            if (e.KeyCode == Keys.Right && playerX < this.ClientSize.Width - 100) playerX += 20;

            // Strzelanie
            if (e.KeyCode == Keys.Space && gameRunning && canShoot)
            {
                bullets.Add(new Rectangle(playerX + 40, playerY - 20, 10, 20)); // Pocisk
                canShoot = false; // Blokada strza³u
            }
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            // Odblokowanie strzelania po puszczeniu spacji
            if (e.KeyCode == Keys.Space) canShoot = true;
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            elapsedTime += 20; // Licznik czasu gry w milisekundach

            // Ruch gwiazd
            for (int i = 0; i < stars.Count; i++)
            {
                stars[i] = new Point(stars[i].X, stars[i].Y + starSpeed);
                if (stars[i].Y > this.ClientSize.Height)
                {
                    stars[i] = new Point(random.Next(0, this.ClientSize.Width), 0);
                }
            }

            // Ruch planet
            for (int i = 0; i < planets.Count; i++)
            {
                planets[i].Position = new Point(planets[i].Position.X, planets[i].Position.Y + planetSpeed);
                if (planets[i].Position.Y > this.ClientSize.Height)
                {
                    planets[i].Position = new Point(random.Next(0, this.ClientSize.Width), 0);
                }
            }

            // Ruch przeciwnika
            enemyX += enemySpeed;
            if (enemyX > this.ClientSize.Width - 150 || enemyX < 0) enemySpeed *= -1;

            enemy = new Rectangle(enemyX, 100, 150, 50);

            // Ruch pocisków
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i] = new Rectangle(bullets[i].X, bullets[i].Y - 10, bullets[i].Width, bullets[i].Height);
                if (bullets[i].Y < 0) bullets.RemoveAt(i);
            }

            // Kolizje
            for (int i = 0; i < bullets.Count; i++)
            {
                if (enemy.IntersectsWith(bullets[i]))
                {
                    bullets.RemoveAt(i);
                    enemyHP -= attackDamage;
                    if (enemyHP <= 0)
                    {
                        bullets.Clear(); // Pociski znikaj¹ po rozwi¹zaniu zagadki
                        gameTimer.Stop();
                        ShowMathPuzzle();
                    }
                    break;
                }
            }

            this.Invalidate(); // Odœwie¿anie rysowania
        }

        private async void ShowMathPuzzle()
        {
            gameRunning = false;
            int timeLimit = difficulty.ToLower() == "easy" ? 10 : difficulty.ToLower() == "medium" ? 7 : 5;
            int timeRemaining = timeLimit;
            string answer = "";
            bool answeredCorrectly = false;

            // Zadanie do odliczania czasu
            Task timerTask = Task.Run(() =>
            {
                while (timeRemaining > 0 && !answeredCorrectly)
                {
                    timeRemaining--;
                    Task.Delay(1000).Wait();
                }
            });

            // Zadanie do wprowadzania odpowiedzi
            Task inputTask = Task.Run(() =>
            {
                while (timeRemaining > 0 && !answeredCorrectly)
                {
                    answer = Microsoft.VisualBasic.Interaction.InputBox(
                        $"Zagadka: {mathQuestion}\nPozosta³y czas: {timeRemaining}s",
                        "Rozwi¹¿ zagadkê",
                        ""
                    );
                    if (int.TryParse(answer, out int result) && result == mathAnswer)
                    {
                        answeredCorrectly = true;
                    }
                }
            });

            // Poczekaj na koniec odliczania lub odpowiedŸ
            await Task.WhenAny(timerTask, inputTask);

            if (!answeredCorrectly)
            {
                lives--;
                if (lives <= 0)
                {
                    MessageBox.Show("Nie odpowiedzia³eœ na czas! Koniec gry.");
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Nie odpowiedzia³eœ na czas! Pozosta³o ¿yæ: {lives}");
                }
            }
            else
            {
                score += 100;
                attackDamage += 5; // Zwiêkszenie obra¿eñ po poprawnej odpowiedzi
                currentEnemyIndex++;
                enemiesLeft--;
                enemyMaxHP += 10; // Ka¿dy kolejny przeciwnik ma wiêcej HP
            }

            if (enemiesLeft > 0)
            {
                enemyHP = enemyMaxHP;
                GenerateMathQuestion();
                gameRunning = true;
                gameTimer.Start();
            }
            else
            {
                MessageBox.Show($"Gratulacje! Wygra³eœ!\nTwój wynik: {score}");
                this.Close();
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // T³o
            g.Clear(Color.Black);

            // Gwiazdy
            foreach (var star in stars)
            {
                g.FillEllipse(Brushes.White, star.X, star.Y, 3, 3);
            }

            // Planety
            foreach (var planet in planets)
            {
                g.FillEllipse(new SolidBrush(planet.Color), planet.Position.X, planet.Position.Y, planet.Size, planet.Size);
            }

            Image invader = Image.FromFile(Path.GetFullPath("..\\..\\..\\Resources\\ship.png")); 

            // Rysowanie statku kosmicznego (obraz zamiast trójk¹ta)
            if (invader != null) // Upewnij siê, ¿e obraz istnieje (inaczej nie uda³o mi sie tego zrobic)
            {
                g.DrawImage(invader, playerX, playerY, 80, 40); // Dopasuj wymiary do obrazu
            }
            
            // Rysowanie przeciwnika
            g.FillRectangle(Brushes.Gray, enemy);
            g.DrawString(displayedQuestion, this.Font, Brushes.White, enemy.X + 10, enemy.Y + 10);
            g.DrawString($"HP: {enemyHP}", this.Font, Brushes.White, enemy.X + 10, enemy.Y + 30);

            // Rysowanie pocisków
            foreach (var bullet in bullets)
            {
                g.FillRectangle(Brushes.Yellow, bullet);
            }

            // Wyœwietlanie interfejsu
            g.DrawString($"Punkty: {score}", this.Font, Brushes.White, 10, 10);
            g.DrawString($"Przeciwników: {currentEnemyIndex}/{currentEnemyIndex + enemiesLeft - 1}", this.Font, Brushes.White, 10, 30);
            g.DrawString($"¯ycia: {lives}", this.Font, Brushes.White, 10, 50);
            g.DrawString($"Obra¿enia: {attackDamage}", this.Font, Brushes.White, 10, 70);
            g.DrawString($"Czas gry: {elapsedTime / 1000}s", this.Font, Brushes.White, 10, 90);
        }


    }

    // Klasa planety
    public class Planet
    {
        public Point Position { get; set; }
        public int Size { get; set; }
        public Color Color { get; set; }
    }
}
