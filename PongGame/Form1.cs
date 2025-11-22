using System;
using System.Drawing;
using System.Windows.Forms;
using NAudio.Wave;      // Thư viện phát MP3 cực mạnh và ổn định trên .NET 8
using System.IO;        // Để kiểm tra file tồn tại, lấy đường dẫn
using System.Threading.Tasks; // Cho Task.Delay trong TriggerReferee
using System.Drawing.Drawing2D; // Cho hiệu ứng vẽ đẹp

namespace PongGame
{
    public partial class Form1 : Form
    {
        // Thông số game
        private int ballSpeedX = 3;           // Tốc độ ngang của bóng (dương = sang phải)
        private int ballSpeedY = 3;           // Tốc độ dọc của bóng (dương = xuống dưới)
        private const int PADDLE_SPEED = 20;  // Tốc độ di chuyển vợt mỗi khung hìnH
        private int bounceCount = 0;           // Đếm số lần bóng nảy vợt → dùng để tăng tốc
        private int scorePlayer1 = 0;                    // Bàn thắng Player 1 (trái)
        private int scorePlayer2 = 0;                    // Bàn thắng Player 2 (phải)

        // Hiệu ứng xoay bóng
        private float rotationAngle = 0f;      // Góc xoay hiện tại của bóng (để hiệu ứng quay)
        private Bitmap originalBallImage;      // Hình bóng gốc (lưu để xoay không bị mờ)

        // Thời gian di chuyển/đứng im của player (dùng để trọng tài phạt)
        private DateTime lastMoveP1 = DateTime.MinValue;      // Lần cuối P1 di chuyển vợt
        private DateTime lastMoveP2 = DateTime.MinValue;      // Lần cuối P2 di chuyển vợt
        private DateTime idleStartP1 = DateTime.MinValue; // Thời điểm P1 bắt đầu đứng im
        private DateTime idleStartP2 = DateTime.MinValue; // Thời điểm P2 bắt đầu đứng im
        private DateTime gameStartTime = DateTime.MinValue; // Thời điểm bấm Start


        // Âm thanh sử dụng NAudio
        private AudioFileReader bgMusicReader;           // Đọc file nhạc nền MP3
        private WaveOutEvent bgMusicOutput;              // Loa phát nhạc nền (có loop)
        private AudioFileReader cheerReader;             // Đọc tiếng cổ vũ
        private WaveOutEvent cheerOutput;                // Loa phát tiếng cổ vũ
        private AudioFileReader kickReader;              // Đọc tiếng đá bóng
        private WaveOutEvent kickOutput;                 // Loa phát tiếng đá
        private AudioFileReader whistleReader;           // Đọc tiếng còi trọng tài
        private WaveOutEvent whistleOutput;              // Loa phát còi
        private AudioFileReader endGameReader;   // Tiếng kết thúc game
        private WaveOutEvent endGameOutput;      // Loa phát EndGame
        private AudioFileReader chaoMungReader;   // Tiếng kết thúc game
        private WaveOutEvent chaoMungOutput;      // Loa phát EndGame

        //Giảm tốc độ bóng theo thời gian
        private DateTime lastBounceTime = DateTime.Now; // Thời điểm cuối cùng bóng chạm vợt
        private const int SLOWDOWN_INTERVAL = 1000;     // 1000ms = 1 giây
        private const int MIN_BALL_SPEED = 5;           // Tốc độ tối thiểu của bóng SpeedX


        // Trạng thái phím (true = đang nhấn)
        private bool wPressed, sPressed, aPressed, dPressed;     // Player 1: WASD
        private bool upPressed, downPressed, leftPressed, rightPressed; // Player 2: Mũi tên

        //Trạng thái game
        private bool gameStarted = false;  // Biết đang ở màn chào hay đang chơi
        private bool refereeActive = false;  // Đang có trọng tài phạt không?
        private Image welcomeBackground; // background giới thiệu


        public Form1()
        {
            InitializeComponent(); // Khởi tạo các control trên Form

            this.KeyPreview = true; // Cho phép Form nhận sự kiện phím trước các control con       

            // Gắn sự kiện nhấn và nhả phím
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            // TỐI ƯU HIỂN THỊ
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
         
            // Load hình nền chào mừng
            welcomeBackground = Image.FromFile("welcome.jpg");
        }

        // ==================== KHI FORM MỞ LẦN ĐẦU ====================
        private void Form1_Load(object sender, EventArgs e)
        {
            // Gọi hàm chung để đặt vị trí lần đầu
            UpdateAllPositions(); 

            // Resize Form → Cập nhật vị trí các vật thể
            this.SizeChanged += (s, ev) => UpdateAllPositions(); // Gọi hàm chung để đặt vị trí lần đầu

            // Lưu hình bóng gốc để xoay liên tục mà không bị vỡ hình
            if (pbBall.Image != null) originalBallImage = new Bitmap(pbBall.Image);         

            // ÂM THANH - KHỞI TẠO NAudio
            string basePath = Application.StartupPath; // Thư mục chứa file .exe
            string bgPath = Path.Combine(basePath, "NhacNen.mp3");
            string cheerPath = Path.Combine(basePath, "CrowdCheeringVoice.mp3");
            string kickPath = Path.Combine(basePath, "No.mp3");
            string whistlePath = Path.Combine(basePath, "RefereeWhistleVoice.mp3");
            string endGamePath = Path.Combine(basePath, "EndGame.mp3");
            string chaoMungPath = Path.Combine(basePath, "ChaoMung.mp3");

            //  Nhạc nền (loop vô hạn) 
            if (File.Exists(bgPath))
            {
                bgMusicReader = new AudioFileReader(bgPath);           // Mở file MP3
                bgMusicOutput = new WaveOutEvent();                    // Tạo loa vật lý
                bgMusicOutput.Init(bgMusicReader);                     // Nối file vào loa
                bgMusicOutput.PlaybackStopped += (s, e) =>             // Khi hết bài hát
                {
                    if (gameStarted)                                   // Chỉ loop khi đang chơi
                    {
                        PlaySound(bgMusicReader, bgMusicOutput);
                    }
                };
            }

            //  Tiếng hiệu ứng: kick, cheer, whistle, endgame
            cheerOutput = new WaveOutEvent();
            kickOutput = new WaveOutEvent();
            whistleOutput = new WaveOutEvent();
            endGameOutput = new WaveOutEvent();
            chaoMungOutput = new WaveOutEvent();

            // Hàm nhỏ để Init an toàn
            void SafeInit(WaveOutEvent output, AudioFileReader reader)
            {
                if (reader != null && output != null)
                {
                    try { output.Init(reader); }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            // Khởi tạo từng cái (chỉ khi file tồn tại)
            if (File.Exists(cheerPath) && cheerReader == null)
            {
                cheerReader = new AudioFileReader(cheerPath);
                SafeInit(cheerOutput, cheerReader);
            }
            if (File.Exists(kickPath) && kickReader == null)
            {
                kickReader = new AudioFileReader(kickPath);
                SafeInit(kickOutput, kickReader);
            }
            if (File.Exists(whistlePath) && whistleReader == null)
            {
                whistleReader = new AudioFileReader(whistlePath);
                SafeInit(whistleOutput, whistleReader);
            }
            if (File.Exists(endGamePath) && endGameReader == null)
            {
                endGameReader = new AudioFileReader(endGamePath);           
                SafeInit(endGameOutput, endGameReader);
            }
            if (File.Exists(chaoMungPath) && chaoMungReader == null)
            {
                chaoMungReader = new AudioFileReader(chaoMungPath);
                SafeInit(chaoMungOutput, chaoMungReader);
            }


            // MÀN HÌNH CHÀO MỪNG
            gameStarted = false;

            // Ẩn tất cả vật thể game
            pbPlayer1.Visible = false;
            pbPlayer2.Visible = false;
            pbBall.Visible = false;
            pbGoal1.Visible = false;
            pbGoal2.Visible = false;
            pbReferee.Visible = false;

            // Hiện nút Start + căn giữa
            btnStart.Visible = true;
            btnStart.Left = (this.ClientSize.Width - btnStart.Width) / 2;                    
            btnStart.Top = this.ClientSize.Height - btnStart.Height - 150;

            // Vẽ nền chào mừng
            this.Paint += Form1_WelcomePaint;
            // Vẽ tốc độ
            this.Paint += Form1_Paint;

            // Timer chạy mỗi 8ms → ~120 FPS
            TimerPongGame.Interval = 8;
            TimerPongGame.Stop();

            // Phát tiếng chào mừng AI
            if (chaoMungReader != null)
            {
                PlaySound(chaoMungReader, chaoMungOutput);
            }
        }

        // Cập nhật vị trí tất cả vật thể theo kích thước Form
        private void UpdateAllPositions()
        {
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            // 1. Goal luôn sát biên + giữa dọc
            pbGoal1.Left = 0;
            pbGoal1.Top = (formHeight - pbGoal1.Height) / 2;

            pbGoal2.Left = formWidth - pbGoal2.Width;
            pbGoal2.Top = (formHeight - pbGoal2.Height) / 2;

            // 2. Player cách Goal 15px (dựa trên Goal đã cố định)
            pbPlayer1.Left = pbGoal1.Right + 15;
            pbPlayer1.Top = (formHeight - pbPlayer1.Height) / 2;

            pbPlayer2.Left = pbGoal2.Left - pbPlayer2.Width - 15;
            pbPlayer2.Top = (formHeight - pbPlayer2.Height) / 2;

            // 3. Bóng luôn ở giữa sân
            pbBall.Left = (formWidth - pbBall.Width) / 2;
            pbBall.Top = (formHeight - pbBall.Height) / 2;

            // 4. Nút Start (khi ở màn chào)
            if (!gameStarted)
            {
                btnStart.Left = (formWidth - btnStart.Width) / 2;
                btnStart.Top = formHeight - btnStart.Height - 150;
            }
        }

        // Sử lí nhấn và nhả phím
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: wPressed = true; break;
                case Keys.S: sPressed = true; break;
                case Keys.A: aPressed = true; break;
                case Keys.D: dPressed = true; break;
                case Keys.Up: upPressed = true; break;
                case Keys.Down: downPressed = true; break;
                case Keys.Left: leftPressed = true; break;
                case Keys.Right: rightPressed = true; break;
            }
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: wPressed = false; break;
                case Keys.S: sPressed = false; break;
                case Keys.A: aPressed = false; break;
                case Keys.D: dPressed = false; break;
                case Keys.Up: upPressed = false; break;
                case Keys.Down: downPressed = false; break;
                case Keys.Left: leftPressed = false; break;
                case Keys.Right: rightPressed = false; break;
            }
        }

        // TIMER CHẠY MỖI KHUNG HÌNH 120fps
        private void TimerPongGame_Tick(object sender, EventArgs e)
        {
            MovePaddles();      // Di chuyển vợt
            UpdateBall();       // Di chuyển bóng
            RotateBall();       // Xoay bóng (hiệu ứng đẹp)
            CheckCollisions();  // Kiểm tra va chạm tường, vợt, thua cuộc
            CheckRefereeIdle(); //  kiểm tra đứng im
            TrySlowDownBall();  // Giảm tốc độ bóng theo thời gian
            this.Invalidate();  // Vẽ lại toàn bộ form → mượt mà
        }

        // DI CHUYỂN VỢT
        private void MovePaddles()
        {
            // === PLAYER 1 (WASD) ===
            if (wPressed) pbPlayer1.Top = Math.Max(0, pbPlayer1.Top - PADDLE_SPEED);
            if (sPressed) pbPlayer1.Top = Math.Min(ClientSize.Height - pbPlayer1.Height, pbPlayer1.Top + PADDLE_SPEED);
            if (aPressed) pbPlayer1.Left = Math.Max(0, pbPlayer1.Left - PADDLE_SPEED);
            if (dPressed) pbPlayer1.Left = Math.Min(ClientSize.Width - pbPlayer1.Width, pbPlayer1.Left + PADDLE_SPEED);
            if (wPressed || sPressed || aPressed || dPressed) 
            {
                lastMoveP1 = DateTime.Now;     // Cập nhật: "P1 vừa mới động đậy"
                idleStartP1 = DateTime.MinValue; // Reset idle khi di chuyển
            }   

            // === PLAYER 2 (MŨI TÊN) ===
            if (upPressed) pbPlayer2.Top = Math.Max(0, pbPlayer2.Top - PADDLE_SPEED);
            if (downPressed) pbPlayer2.Top = Math.Min(ClientSize.Height - pbPlayer2.Height, pbPlayer2.Top + PADDLE_SPEED);
            if (leftPressed) pbPlayer2.Left = Math.Max(0, pbPlayer2.Left - PADDLE_SPEED);
            if (rightPressed) pbPlayer2.Left = Math.Min(ClientSize.Width - pbPlayer2.Width, pbPlayer2.Left + PADDLE_SPEED);
            if (upPressed || downPressed || leftPressed || rightPressed)
            {
                lastMoveP2 = DateTime.Now;     // Cập nhật: "P2 vừa mới động đậy"
                idleStartP2 = DateTime.MinValue; // Reset idle khi di chuyển
            }
            // KIỂM TRA VA CHẠM GIỮA 2 PLAYER → KHÔNG XUYÊN QUA NHAU
            if (pbPlayer1.Bounds.IntersectsWith(pbPlayer2.Bounds))
            {
                // Tính tâm (center) của 2 player để quyết định ai đẩy ai
                float centerX1 = pbPlayer1.Left + pbPlayer1.Width / 2f;
                float centerX2 = pbPlayer2.Left + pbPlayer2.Width / 2f;
                float centerY1 = pbPlayer1.Top + pbPlayer1.Height / 2f;
                float centerY2 = pbPlayer2.Top + pbPlayer2.Height / 2f;

                // Đẩy ngang (X): Ai bên trái thì đẩy thằng kia sang phải
                if (centerX1 < centerX2)
                {
                    pbPlayer2.Left = pbPlayer1.Right + 2;  // Đẩy P2 sang phải 2px
                }
                else
                {
                    pbPlayer1.Left = pbPlayer2.Right + 2;  // Đẩy P1 sang phải 2px
                }

                // Đẩy dọc (Y): Ai trên thì đẩy thằng kia xuống dưới
                if (centerY1 < centerY2)
                {
                    pbPlayer2.Top = pbPlayer1.Bottom + 2;  // Đẩy P2 xuống dưới
                }
                else
                {
                    pbPlayer1.Top = pbPlayer2.Bottom + 2;  // Đẩy P1 xuống dưới
                }

                

                // Cập nhật thời gian di chuyển (để tránh bị trọng tài phạt khi va chạm)
                lastMoveP1 = DateTime.Now;
                lastMoveP2 = DateTime.Now;
                idleStartP1 = DateTime.MinValue;
                idleStartP2 = DateTime.MinValue;
            }
        }

        // CẬP NHẬT VỊ TRÍ BÓNG
        private void UpdateBall()
        {
            pbBall.Left += ballSpeedX;  // Di chuyển ngang
            pbBall.Top += ballSpeedY;   // Di chuyển dọc
        }

        // XOAY BÓNG
        private void RotateBall()
        {
            rotationAngle += 6f;                    // Tăng góc xoay mỗi khung hình
            if (rotationAngle >= 360f) rotationAngle = 0f;

            if (originalBallImage != null)
            {
                Bitmap rotated = RotateImage(originalBallImage, rotationAngle);
                pbBall.Image?.Dispose();            // Giải phóng hình cũ tránh rò rỉ bộ nhớ
                pbBall.Image = rotated;             // Gán hình mới đã xoay
            }
        }
        // XOAY HÌNH ẢNH
        private Bitmap RotateImage(Bitmap bmp, float angle)
        {
            int w = bmp.Width, h = bmp.Height;
            Bitmap rotated = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.TranslateTransform(w / 2f, h / 2f);   // Đưa tâm về giữa
                g.RotateTransform(angle);              // Xoay theo góc
                g.TranslateTransform(-w / 2f, -h / 2f); // Đưa tâm về lại
                g.DrawImage(bmp, 0, 0, w, h);           // Vẽ hình gốc
            }
            return rotated;
        }

        // Reset bóng về giữa sân + random hướng
        private void ResetBall()
        {
            pbBall.Left = (ClientSize.Width - pbBall.Width) / 2;
            pbBall.Top = (ClientSize.Height - pbBall.Height) / 2;
            Random r = new Random();
            ballSpeedX = r.Next(0, 2) == 0 ? 4 : -4;   // Random trái/phải
            ballSpeedY = r.Next(0, 2) == 0 ? 4 : -4;   // Random lên/xuống
            bounceCount = 0;
        }

        // KIỂM TRA VA CHẠM (4 MẶT ĐỀU NẢY LẠI)
        private void CheckCollisions()
        {
            // Goal Player 2 thắng (bóng vào pbGoal1)
            if (pbBall.Bounds.IntersectsWith(pbGoal1.Bounds))
            {
                // Cộng điểm Player 2
                scorePlayer2++;
                // Kiểm tra thắng cuộc
                if (scorePlayer2 >= 3)
                {
                    ShowGameOver("PLAYER 2 THẮNG TRẬN!", $"TỈ SỐ: {scorePlayer1} - {scorePlayer2}");
                    return;
                }
                else
                {
                    PlaySound(cheerReader, cheerOutput);
                }
                // Reset bóng về giữa sân
                ResetBall();
                return;
            }

            // Goal Player 1 thắng (bóng vào pbGoal2)  
            if (pbBall.Bounds.IntersectsWith(pbGoal2.Bounds))
            {
                scorePlayer1++;
                if (scorePlayer1 >= 3)
                {
                    ShowGameOver("PLAYER 1 THẮNG TRẬN!", $"TỈ SỐ: {scorePlayer1} - {scorePlayer2}");
                    return;
                }
                else
                {
                    PlaySound(cheerReader, cheerOutput);
                }
                ResetBall();             
                return;
            }

            // NẢY 4 BIÊN 
            if (pbBall.Left <= 0)
            {
                pbBall.Left = 0;
                ballSpeedX = Math.Abs(ballSpeedX);
            }
            if (pbBall.Left + pbBall.Width >= ClientSize.Width)
            {
                pbBall.Left = ClientSize.Width - pbBall.Width;
                ballSpeedX = -Math.Abs(ballSpeedX);
            }
            if (pbBall.Top <= 0)
            {
                pbBall.Top = 0;
                ballSpeedY = Math.Abs(ballSpeedY);
            }
            if (pbBall.Top + pbBall.Height >= ClientSize.Height)
            {
                pbBall.Top = ClientSize.Height - pbBall.Height;
                ballSpeedY = -Math.Abs(ballSpeedY);
            }
            CheckPaddleBounce();
        }

        // NẢY VỢT + TĂNG TỐC ĐỘ 
        private void CheckPaddleBounce()
        {
            // Nảy vợt Player 1 (trái) → bóng bay sang phải
            if (pbBall.Bounds.IntersectsWith(pbPlayer1.Bounds))
            {
                bounceCount++; // TĂNG SỐ LẦN NẢY
                lastBounceTime = DateTime.Now; // CẬP NHẬT: bóng vừa được chạm!

                // Phát tiếng đá bóng AN TOÀN
                if (kickReader != null)
                {
                    PlaySound(kickReader, kickOutput);
                }

                // TỐC ĐỘ GỐC = |ballSpeedX hiện tại| + 2 × lần nảy, tối đa 
                int currentSpeed = Math.Abs(ballSpeedX);  // Lấy giá trị tuyệt đối
                int newSpeed = Math.Min(currentSpeed + (bounceCount * 7), 30);

                ballSpeedX = newSpeed;   // Cập nhật tốc độ mới (dương)
                pbBall.Left = pbPlayer1.Right + 1; // Đặt bóng sát vợt tránh dính vợt
            }   
            // Nảy vợt Player 2 (phải) → bóng bay sang trái
            else if (pbBall.Bounds.IntersectsWith(pbPlayer2.Bounds))
            {
                bounceCount++; // TĂNG SỐ LẦN NẢY
                lastBounceTime = DateTime.Now; // CẬP NHẬT: bóng vừa được chạm!

                // Phát tiếng đá bóng AN TOÀN
                if (kickReader != null)
                {
                    PlaySound(kickReader, kickOutput);
                }

                int currentSpeed = Math.Abs(ballSpeedX);
                int newSpeed = Math.Min(currentSpeed + (bounceCount * 7), 30);

                ballSpeedX = -newSpeed;     // Âm → bay về trái
                pbBall.Left = pbPlayer2.Left - pbBall.Width - 1;
            }
        }

        //  GIẢM TỐC ĐỘ BÓNG THEO THỜI GIAN 
        private void TrySlowDownBall()
        {
            if (!gameStarted) return;

            // Nếu đã quá 1 giây kể từ lần chạm vợt cuối cùng
            if ((DateTime.Now - lastBounceTime).TotalMilliseconds >= SLOWDOWN_INTERVAL)
            {
                // Lấy tốc độ hiện tại SpeedX
                int currentSpeed = Math.Abs(ballSpeedX);

                // Chỉ giảm nếu đang nhanh hơn mức tối thiểu
                if (currentSpeed > MIN_BALL_SPEED)
                {
                    int newSpeed = currentSpeed - 5;

                    // Giữ nguyên hướng
                    if (ballSpeedX > 0)
                        ballSpeedX = newSpeed;
                    else
                        ballSpeedX = -newSpeed;

                    // Cập nhật lại thời gian để tránh giảm liên tục 
                    lastBounceTime = DateTime.Now;
                }
            }
        }
  
        // Form1_Welcome
        private void Form1_WelcomePaint(object sender, PaintEventArgs e)
        {
            if (gameStarted) return; // Chỉ vẽ khi chưa bắt đầu
            Graphics g = e.Graphics;
            if (welcomeBackground != null)
            {
                g.DrawImage(welcomeBackground,
                            0, 0,
                            ClientSize.Width, ClientSize.Height);
            }
            string title = "BALL GAME";
            using (Font f = new Font("Arial", 60, FontStyle.Bold))
            using (Brush b = new SolidBrush(Color.Black))
            {
                SizeF size = g.MeasureString(title, f);
                g.DrawString(title, f, b, (ClientSize.Width - size.Width) / 2, 100);
            }
            // Chào mừng
            string welcome = "Chào mừng bạn đã đến với trò chơi!";
            using (Font f = new Font("Consolas", 24))
            using (Brush b = new SolidBrush(Color.Black))
            {
                SizeF size = g.MeasureString(welcome, f);
                g.DrawString(welcome, f, b, (ClientSize.Width - size.Width) / 2, 200);
            }
            // Hướng dẫn
            string[] lines =
            {
                "Player 1: Dùng phím W A S D",
                "Player 2: Dùng phím mũi tên",
                "",
                "Đưa bóng vào khung thành đối phương để ghi bàn +1 điểm!"
};
            using (Font f = new Font("Arial", 18))
            using (Brush b = new SolidBrush(Color.Black))
            {
                int y = 300;
                foreach (string line in lines)
                {
                    SizeF size = g.MeasureString(line, f);
                    g.DrawString(line, f, b, (ClientSize.Width - size.Width) / 2, y);
                    y += 40;
                }
            }
        }

        // HIỂN THỊ TỐC ĐỘ BÓNG
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (!gameStarted) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Tính tốc độ thực tế (căn bậc 2 của vx² + vy²)
            double speed = Math.Sqrt(ballSpeedX * ballSpeedX + ballSpeedY * ballSpeedY);
            int displaySpeed = (int)(speed * 5); // Nhân hệ số cho đẹp 
            // Hiển thị tốc độ bóng
            string speedText = $"SPEED: {displaySpeed} KM/H";

            // Vẽ chữ với hiệu ứng bóng đổ + viền trắng + chữ vàng neon
            using (Font font = new Font("Consolas", 20, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.FromArgb(255, 255, 100)))
            using (Brush shadow = new SolidBrush(Color.Black))
            {
                // Bóng đổ
                g.DrawString(speedText, font, shadow, 13, 13);
                g.DrawString(speedText, font, shadow, 12, 12);

                // Chữ chính (vàng neon)
                g.DrawString(speedText, font, brush, 12, 12);

                // Viền trắng mỏng cho nổi bật
                using (Pen pen = new Pen(Color.White, 2))
                {
                    SizeF size = g.MeasureString(speedText, font);
                    g.DrawRectangle(pen, 8, 8, size.Width + 12, size.Height + 8);
                }
            }

            // Bonus: Hiển thị số lần nảy (Bounce Count)
            string bounceText = $"BOUNCE: {bounceCount}";
            using (Font font = new Font("Consolas", 16))
            using (Brush brush = new SolidBrush(Color.Cyan))
            {
                g.DrawString(bounceText, font, Brushes.Black, 13, 53);
                g.DrawString(bounceText, font, brush, 12, 52);
            }

            // Vẽ tỉ số lớn giữa sân (chỉ khi đang chơi)
            if (gameStarted)
            {
                string scoreText = $"{scorePlayer1}  :  {scorePlayer2}";
                using (Font f = new Font("Arial", 72, FontStyle.Bold))
                using (Brush b = new SolidBrush(Color.FromArgb(255, 255, 200)))
                using (Brush shadow = new SolidBrush(Color.Black))
                {
                    SizeF sz = e.Graphics.MeasureString(scoreText, f);
                    float x = (ClientSize.Width - sz.Width) / 2;
                    float y = 10;

                    e.Graphics.DrawString(scoreText, f, shadow, x + 3, y + 3); // bóng đổ
                    e.Graphics.DrawString(scoreText, f, b, x, y);             // chữ chính
                }
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Vẽ tên kèm thời gian đứng im cho mỗi player
                DrawPlayerIdleInfo(g, pbPlayer1, "PLAYER 1", Brushes.Red, Brushes.Orange, idleStartP1);
                DrawPlayerIdleInfo(g, pbPlayer2, "PLAYER 2", Brushes.Cyan, Brushes.Lime, idleStartP2);
            }
        }

        // Vẽ thông tin đứng im của player với màu tùy chỉnh
        private void DrawPlayerIdleInfo(Graphics g, PictureBox playerPb, string playerName,
                                        Brush nameBrush, Brush safeIdleBrush, DateTime idleStart)
        {
            // Tính thời gian đứng im (giống code gốc)
            double idleTime = idleStart == DateTime.MinValue ? 0 : (DateTime.Now - idleStart).TotalSeconds;
            string idleText = idleTime > 0 ? $"IDLE: {idleTime:F1}s" : "IDLE: 0.0s";

            // 1. Vẽ tên player (màu tùy chỉnh)
            DrawFancyText(g, playerName, new Font("Arial", 16, FontStyle.Bold), nameBrush,
                new PointF(playerPb.Left + 10, playerPb.Top - 30), Brushes.Black);

            // 2. Vẽ IDLE text (màu đỏ nếu >3s, màu safe nếu <3s)
            Brush idleBrush = idleTime > 3 ? Brushes.Red : safeIdleBrush;
            DrawFancyText(g, idleText, new Font("Consolas", 14), idleBrush,
                new PointF(playerPb.Left + 10, playerPb.Top - 10), Brushes.Black);

            // 3. Troll warning: "DI CHUYỂN ĐI!!!" (3-5s)
            if (idleTime > 3.0 && idleTime <= 5.0)
            {
                DrawFancyText(g, "DI CHUYỂN ĐI!!!", new Font("Arial", 12, FontStyle.Bold),
                    Brushes.Yellow, new PointF(playerPb.Left + 5, playerPb.Top - 50), Brushes.Black);
            }
        }

        // Vẽ chữ đẹp
        private void DrawFancyText(Graphics g, string text, Font font, Brush brush, PointF position, Brush shadowBrush = null)
        {
            if (shadowBrush != null)
            {
                g.DrawString(text, font, shadowBrush, position.X + 2, position.Y + 2); // Bóng đổ
            }
            g.DrawString(text, font, brush, position); // Text chính
        }

        // Kiểm tra ai đứng im quá 5 giây
        private void CheckRefereeIdle()
        {
            if (!gameStarted || refereeActive) return;

            // Chỉ bắt đầu đếm idle khi người chơi đã di chuyển ít nhất 1 lần
            if (lastMoveP1 != DateTime.MinValue && idleStartP1 == DateTime.MinValue && !wPressed && !sPressed && !aPressed && !dPressed)
                idleStartP1 = DateTime.Now;

            if (lastMoveP2 != DateTime.MinValue && idleStartP2 == DateTime.MinValue && !upPressed && !downPressed && !leftPressed && !rightPressed)
                idleStartP2 = DateTime.Now;

            // Kiểm tra P1 đứng im > 5s
            if (idleStartP1 != DateTime.MinValue && (DateTime.Now - idleStartP1).TotalSeconds > 5)
            {
                TriggerReferee(1);
                idleStartP1 = DateTime.MinValue; // Reset sau khi bị phạt
                return;
            }

            // Kiểm tra P2 đứng im > 5s
            if (idleStartP2 != DateTime.MinValue && (DateTime.Now - idleStartP2).TotalSeconds > 5)
            {
                TriggerReferee(2);
                idleStartP2 = DateTime.MinValue; // Reset sau khi bị phạt
                return;
            }
        }

        // Xử lý trọng tài phạt – AN TOÀN THREAD
        private void TriggerReferee(int idlePlayer)
        {
            if (!gameStarted || refereeActive) return;

            // Chạy trên UI thread để tránh lỗi cross-thread
            this.Invoke((MethodInvoker)(() =>
            {
                refereeActive = true;

                // Phát còi + cổ vũ
                if (whistleReader != null) PlaySound(whistleReader, whistleOutput);
                if (cheerReader != null) PlaySound(cheerReader, cheerOutput);

                // Hiện trọng tài
                pbReferee.Visible = true;
                pbReferee.BringToFront();
                pbReferee.Left = (ClientSize.Width - pbReferee.Width) / 2;
                pbReferee.Top = (ClientSize.Height - pbReferee.Height) / 2;

                // Cộng điểm
                if (idlePlayer == 1) scorePlayer2++;
                else scorePlayer1++;

                ResetBall();
                this.Invalidate();
                gameStartTime = DateTime.Now; // ← DÒNG MỚI: GHI LẠI THỜI GIAN BẮM START

                // ẨN SAU 0 GIÂY + KIỂM TRA THẮNG
                Task.Delay(1000).ContinueWith(_ =>
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        pbReferee.Visible = false;
                        refereeActive = false;

                        // KIỂM TRA AI ĐẠT 3 BÀN TRƯỚC
                        if (scorePlayer2 >= 3)
                        {
                            ShowGameOver("PLAYER 2 THẮNG TRẬN!", $"TỈ SỐ: {scorePlayer1} - {scorePlayer2}");
                        }
                        else if (scorePlayer1 >= 3)
                        {
                            ShowGameOver("PLAYER 1 THẮNG TRẬN!", $"TỈ SỐ: {scorePlayer1} - {scorePlayer2}");
                        }
                    }));
                });
            }));
        }

        // hàm phát âm thanh an toàn
        private void PlaySound(AudioFileReader reader, WaveOutEvent output)
        {
  

            // Kiểm tra null
            if (reader == null || output == null) return;
            // An toàn khi phát âm thanh
            try
            {
                reader.Position = 0; // Quay lại đầu file

                // Tùy chỉnh volume theo loại âm thanh
                if (reader == kickReader)       
                    reader.Volume = 1.5f;        // Tiếng đá bóng
                else if (reader == cheerReader) 
                    reader.Volume = 2.0f;        // Tiếng đám đông ăn mừng
                else if (reader == whistleReader)
                    reader.Volume = 1.8f;        // Còi trọng tài to vừa đủ
                else if (reader == chaoMungReader)
                    reader.Volume = 2.0f;        // EndGame to vừa đủ 
                else if (reader == endGameReader)
                    reader.Volume = 2.5f;        // EndGame to vừa đủ
                else
                    reader.Volume = 0.05f;        // Nhạc nền 

                output.Play();
            }
            catch (Exception ex)
            {
                // Không bao giờ crash
                Console.WriteLine("Lỗi âm thanh: " + ex.Message);
            }
        }

        //  HIỂN THỊ KẾT THÚC TRẬN 
        private void ShowGameOver(string winner, string finalScore)
        {
            // 1. DỪNG TIMER & TẤT CẢ ÂM THANH KHÁC
            TimerPongGame.Stop();
            bgMusicOutput?.Stop();
            cheerOutput?.Stop();     
            kickOutput?.Stop();
            whistleOutput?.Stop();

            // 2. KIỂM TRA & PHÁT ENDGAME
            if (endGameReader == null || endGameOutput == null)
            {
                MessageBox.Show("LỖI: EndGame.mp3 KHÔNG TÌM THẤY!\nCopy vào thư mục .exe");
                this.Close();
                return;
            }
            else
            {
                // Phát nhạc kết thúc
                PlaySound(endGameReader, endGameOutput);
            }

            // 3. HIỂN THỊ MESSAGEBOX
            string message =
                "════════════════════════════════\n" +
                $" {winner}\n" +
                "════════════════════════════════\n\n" +
                $" TỈ SỐ CUỐI CÙNG\n" +
                $" {scorePlayer1} : {scorePlayer2}\n\n" +
                $" {finalScore}\n\n" +
                " Cảm ơn bạn đã chơi!\n" +
                " Nhấn OK để thoát game.\n" +
                "════════════════════════════════";
            MessageBox.Show(message, "GAME OVER - KẾT THÚC TRẬN ĐẤU", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
        }

        // Giải phóng tài nguyên khi đóng Form sau khi this.close() của ShowGameOver() được gọi
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Dừng timer
            TimerPongGame.Stop();

            // Dừng tất cả âm thanh đang phát (tránh bị kêu ngầm)
            bgMusicOutput?.Stop();
            cheerOutput?.Stop();
            kickOutput?.Stop();
            whistleOutput?.Stop();
            endGameOutput?.Stop();
            chaoMungOutput?.Stop();

            // Dispose NAudio → giải phóng file MP3 ngay lập tức
            bgMusicReader?.Dispose();
            cheerReader?.Dispose();
            kickReader?.Dispose();
            whistleReader?.Dispose();
            endGameReader?.Dispose();
            chaoMungReader?.Dispose();

            bgMusicOutput?.Dispose();
            cheerOutput?.Dispose();
            kickOutput?.Dispose();
            whistleOutput?.Dispose();
            endGameOutput?.Dispose();
            chaoMungOutput?.Dispose();

            // Dispose ảnh tự tạo
            originalBallImage?.Dispose();
            welcomeBackground?.Dispose();

            base.OnFormClosing(e); // Quan trọng: phải có dòng này!
        }

        // Các sự kiện click (không dùng nhưng phải có vì Designer tạo ra)
        private void pbBall_Click(object sender, EventArgs e) { }
        private void pbPlayer1_Click(object sender, EventArgs e) { }
        private void pbPlayer2_Click(object sender, EventArgs e) { }
        private void pbGoal1_Click(object sender, EventArgs e) { }
        private void pbGoal2_Click(object sender, EventArgs e) { }

        private void btnStart_Click(object sender, EventArgs e)
        {
          
            // Bắt đầu game
            gameStarted = true;

            // Dừng nhạc chào mừng
            chaoMungOutput?.Stop();

            // Ẩn nút + bỏ paint chào
            btnStart.Visible = false;
            this.Paint -= Form1_WelcomePaint;

            // Hiện lại tất cả
            pbPlayer1.Visible = true;
            pbPlayer2.Visible = true;
            pbBall.Visible = true;
            pbGoal1.Visible = true;
            pbGoal2.Visible = true;

            // Reset bóng
            pbBall.Left = (ClientSize.Width - pbBall.Width) / 2;
            pbBall.Top = (ClientSize.Height - pbBall.Height) / 2;
            ballSpeedX = 3;
            ballSpeedY = 3;
            bounceCount = 0;

            // Bắt đầu timer
            TimerPongGame.Interval = 6;
            TimerPongGame.Start();
            bgMusicOutput?.Play();        // BẮT ĐẦU NHẠC NỀN KHI ẤN START
            // ẩn trọng tài
            pbReferee.Visible = false;   // Đảm bảo trọng tài ẩn khi bắt đầu trận mới
            refereeActive = false;       // Reset trạng thái phạt
        }
      
    }
}