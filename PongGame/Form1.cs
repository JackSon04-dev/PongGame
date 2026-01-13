using System;
using System.Drawing;
using System.Windows.Forms;
using NAudio.Wave;      // Thư viện phát MP3 cực mạnh và ổn định trên .NET 8
using System.IO;        // Để kiểm tra file tồn tại, lấy đường dẫn
using System.Threading.Tasks; // Cho Task.Delay trong TriggerReferee
using System.Drawing.Drawing2D; // Cho hiệu ứng vẽ đẹp
using Timer = System.Windows.Forms.Timer;  
namespace PongGame
{
    public partial class Form1 : Form
    {
        // Thông số game
        private int ballSpeedX = 5;           // Tốc độ ngang của bóng (dương = sang phải)
        private int ballSpeedY = 5;           // Tốc độ dọc của bóng (dương = xuống dưới)
        private const int PADDLE_SPEED = 12;  // Tốc độ di chuyển vợt mỗi khung hìnH
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
        private AudioFileReader bonusAppearReader;   // âm thanh khi bonus xuất hiện
        private WaveOutEvent bonusAppearOutput;
        private AudioFileReader bonusHitReader;      // âm thanh khi ball chạm bonus (biến mất)
        private WaveOutEvent bonusHitOutput;

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

        // Animation Player 1
        private Image[] p1RightFrames;
        private Image[] p1LeftFrames;
        private int p1FrameIndex = 0;
        private int p1FrameSpeed = 13;   // tốc độ chuyển frame
        private int p1FrameCounter = 0;
        private bool p1IsMovingRight = false;
        private bool p1IsMovingLeft = false;
        private bool p1IsMovingUp = false;
        private bool p1IsMovingDown = false;
        private string lastDirectionP1 = "right";
        // Animation Player 2
        private Image[] p2RightFrames;
        private Image[] p2LeftFrames;
        private int p2FrameIndex = 0;
        private int p2FrameSpeed = 13;   // tốc độ chuyển frame
        private int p2FrameCounter = 0;
        private bool p2IsMovingRight = false;
        private bool p2IsMovingLeft = false;
        private bool p2IsMovingUp = false;
        private bool p2IsMovingDown = false;
        private string lastDirectionP2 = "left";

        // === HIỆU ỨNG VỤ NỔ SAU 5 TICK ===
        private PictureBox explosionEffect;
        private Image explosionImage;
        private Timer explosionDelayTimer;
        private int explosionDelayCount = 0;
        private Point explosionTargetPos;
        private bool explosionPending = false;
        private int explosionPlayerWidth = 80; // sẽ lấy từ player khi va chạm
        private bool explosionFollowingBall = false;  // Đang theo bóng không?
        private Timer explosionFollowTimer;           // Timer để cập nhật vị trí vụ nổ

        // === BONUS ITEM – XUẤT HIỆN MỖI 10 GIÂY ===
        private PictureBox pbBonus;
        private Image bonusImage;
        private Timer bonusSpawnTimer;
        private bool bonusActive = false;
        private Random random = new Random();



        public Form1()
        {
            InitializeComponent(); // Khởi tạo các control trên Form

            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

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
            welcomeBackground = Image.FromFile("nenGalaxyGame.png");

            // Load ảnh animation Player 1
            string root = Path.Combine(Application.StartupPath, "Resources", "images_player");
            p1RightFrames = new Image[]
            {
                Image.FromFile(Path.Combine(root, "player1_right0.png")),
                Image.FromFile(Path.Combine(root, "player1_right1.png")),
                Image.FromFile(Path.Combine(root, "player1_right2.png")),
                Image.FromFile(Path.Combine(root, "player1_right3.png"))
            };

            p1LeftFrames = new Image[]
            {
                Image.FromFile(Path.Combine(root, "player1_left0.png")),
                Image.FromFile(Path.Combine(root, "player1_left1.png")),
                Image.FromFile(Path.Combine(root, "player1_left2.png")),
                Image.FromFile(Path.Combine(root, "player1_left3.png"))
            };
            p2RightFrames = new Image[]
            {
                Image.FromFile(Path.Combine(root, "player2_right0.png")),
                Image.FromFile(Path.Combine(root, "player2_right1.png")),
                Image.FromFile(Path.Combine(root, "player2_right2.png")),
                Image.FromFile(Path.Combine(root, "player2_right3.png"))
            };
            p2LeftFrames = new Image[]
            {
                Image.FromFile(Path.Combine(root, "player2_left0.png")),
                Image.FromFile(Path.Combine(root, "player2_left1.png")),
                Image.FromFile(Path.Combine(root, "player2_left2.png")),
                Image.FromFile(Path.Combine(root, "player2_left3.png"))
            };

            // Khởi tạo ảnh đứng yên ban đầu
            //pbPlayer1.Image = p1RightFrames[0];
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
            string cheerPath = Path.Combine(basePath, "1diem.mp3");
            string kickPath = Path.Combine(basePath, "No.mp3");
            string whistlePath = Path.Combine(basePath, "RefereeWhistleVoice.mp3");
            string endGamePath = Path.Combine(basePath, "EndGame.mp3");
            string chaoMungPath = Path.Combine(basePath, "intro.mp3");
            string bonusAppearPath = Path.Combine(basePath, "bonusStart.mp3");  // âm thanh "ting ting" lấp lánh
            string bonusHitPath = Path.Combine(basePath, "bonusBall.mp3");        // âm thanh "poof" biến mất

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

            //  Tiếng hiệu ứng: kick, cheer, whistle, endgame,..
            cheerOutput = new WaveOutEvent();
            kickOutput = new WaveOutEvent();
            whistleOutput = new WaveOutEvent();
            endGameOutput = new WaveOutEvent();
            chaoMungOutput = new WaveOutEvent();
            bonusAppearOutput = new WaveOutEvent();
            bonusHitOutput = new WaveOutEvent();

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
            if (File.Exists(bonusAppearPath))
            {
                bonusAppearReader = new AudioFileReader(bonusAppearPath);
                SafeInit(bonusAppearOutput, bonusAppearReader);
            }
            if (File.Exists(bonusHitPath))
            {
                bonusHitReader = new AudioFileReader(bonusHitPath);
                SafeInit(bonusHitOutput, bonusHitReader);
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

            // Timer chạy mỗi 16ms → ~60 FPS
            TimerPongGame.Interval = 1000;
            TimerPongGame.Stop();

            // Phát tiếng chào mừng AI
            if (chaoMungReader != null)
            {
                PlaySound(chaoMungReader, chaoMungOutput);
            }

            // Load ảnh vụ nổ nhỏ (PNG trong suốt)
            // === TẠO VỤ NỔ TRONG SUỐT HOÀN HẢO + ĐẸP NHẤT ===
            string expPath = Path.Combine(Application.StartupPath, "Resources", "vuno.png");
            if (File.Exists(expPath))
            {
                explosionImage = Image.FromFile(expPath);

                // Tạo PictureBox trong suốt 100%
                explosionEffect = new PictureBox
                {
                    Size = new Size(80, 80),
                    BackColor = Color.Transparent,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Visible = false,
                    Image = explosionImage
                };

                // BẬT TRONG SUỐT THẬT SỰ (2 dòng bắt buộc)
                explosionEffect.Parent = this;                    // Dòng quan trọng nhất!
                this.Controls.Add(explosionEffect);
                explosionEffect.BringToFront();

                // Timer delay 5 tick
                explosionDelayTimer = new Timer();
                explosionDelayTimer.Interval = 8;                 // ~120 FPS → 5 tick = ~40ms
                explosionDelayTimer.Tick += (s, e) =>
                {
                    explosionDelayCount++;
                    if (explosionDelayCount >= 1)
                    {
                        // Hiện vụ nổ đúng vị trí
                        explosionEffect.Location = new Point(
                            explosionTargetPos.X - explosionEffect.Width / 2,
                            explosionTargetPos.Y - explosionEffect.Height / 2
                        );
                        explosionEffect.Visible = true;
                        explosionEffect.BringToFront();

                        // Tự ẩn sau 280ms
                        Task.Delay(95).ContinueWith(_ =>
                        {
                            this.Invoke((MethodInvoker)(() => explosionEffect.Visible = false));
                        });

                        explosionDelayTimer.Stop();
                        explosionDelayCount = 0;
                        explosionPending = false;
                    }
                };
            }

            // === TẠO BONUS ITEM ===
            string bonusPath = Path.Combine(Application.StartupPath, "Resources", "Moon.png");
            if (File.Exists(bonusPath))
            {
                bonusImage = Image.FromFile(bonusPath);

                pbBonus = new PictureBox();
                pbBonus.Size = new Size(60, 60);
                pbBonus.BackColor = Color.Transparent;
                pbBonus.SizeMode = PictureBoxSizeMode.StretchImage;
                pbBonus.Image = bonusImage;
                pbBonus.Visible = false;
                pbBonus.BringToFront();
                this.Controls.Add(pbBonus);
            }

            bonusSpawnTimer = new Timer();
            bonusSpawnTimer.Interval = 7000; // 10 giây
            bonusSpawnTimer.Tick += (s, e) =>
            {
                SpawnBonus();
            };
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

            GameLoop();          // Vòng lặp game (animation player)
            UpdateBall();       // Di chuyển bóng
            RotateBall();       // Xoay bóng (hiệu ứng đẹp)
            CheckCollisions();  // Kiểm tra va chạm tường, vợt, thua cuộc
            CheckRefereeIdle(); //  kiểm tra đứng im
            TrySlowDownBall();  // Giảm tốc độ bóng theo thời gian
            this.Invalidate();  // Vẽ lại toàn bộ form → mượt mà
        }

        // ANIMATION PLAYER 1
        private void AnimatePlayer1()
        {
            // Xử lí trường hợp nhấn cả 2 phím trái phải animation dừng tại frame trước đó ko reset về 0
            if (p1IsMovingLeft && p1IsMovingRight)
            {
                p1IsMovingLeft = false;
                p1IsMovingRight = false;
                return;
            }
            // Xử lí trường hợp nhấn cả 2 phím lên xuống animation dừng tại frame trước đó ko reset về 0
            if (p1IsMovingUp && p1IsMovingDown)
            {
                p1IsMovingUp = false;
                p1IsMovingDown = false;
                return;
            }

            // Không di chuyển → đứng yên theo hướng cuối cùng ← MỞ RỘNG: bao gồm up/down
            if (!p1IsMovingLeft && !p1IsMovingRight && !p1IsMovingUp && !p1IsMovingDown)
            {
                if (lastDirectionP1 == "right")
                    pbPlayer1.Image = p1RightFrames[0];
                else
                    pbPlayer1.Image = p1LeftFrames[0];
                return;
            }

            // Đang di chuyển → cập nhật hướng cuối cùng
            if (p1IsMovingRight) lastDirectionP1 = "right";
            if (p1IsMovingLeft) lastDirectionP1 = "left";

            // Animation frames
            p1FrameCounter++;
            if (p1FrameCounter >= p1FrameSpeed)
            {
                p1FrameCounter = 0;
                p1FrameIndex++;
                if (p1FrameIndex >= 4) p1FrameIndex = 0;

                // ← THAY: Ưu tiên up/down/left/right, up/down reuse frame right/left dựa lastDirectionP1
                if (p1IsMovingUp || p1IsMovingDown || p1IsMovingRight || p1IsMovingLeft)  // Bất kỳ di chuyển nào
                {
                    if (lastDirectionP1 == "right")
                        pbPlayer1.Image = p1RightFrames[p1FrameIndex];
                    else if (lastDirectionP1 == "left")
                        pbPlayer1.Image = p1LeftFrames[p1FrameIndex];
                }
            }
        }

        // ANIMATION PLAYER 2
        private void AnimatePlayer2()
        {
            // Xử lí trường hợp nhấn cả 2 phím trái phải animation dừng tại frame trước đó ko reset về 0
            if (p2IsMovingLeft && p2IsMovingRight)
            {
                p2IsMovingLeft = false;
                p2IsMovingRight = false;
                return;
            }

            // Xử lí trường hợp nhấn cả 2 phím lên xuống animation dừng tại frame trước đó ko reset về 0
            if (p2IsMovingUp && p2IsMovingDown)
            {
                p2IsMovingUp = false;
                p2IsMovingDown = false;
                return;
            }

            // Không di chuyển → đứng yên theo hướng cuối cùng
            if (!p2IsMovingLeft && !p2IsMovingRight && !p2IsMovingUp && !p2IsMovingDown)
            {
                if (lastDirectionP2 == "right")
                    pbPlayer2.Image = p2RightFrames[0];
                else
                    pbPlayer2.Image = p2LeftFrames[0];

                return;
            }

            // Đang di chuyển → cập nhật hướng cuối cùng
            if (p2IsMovingRight) lastDirectionP2 = "right";
            if (p2IsMovingLeft) lastDirectionP2 = "left";

            // Animation frames
            p2FrameCounter++;
            if (p2FrameCounter >= p2FrameSpeed)
            {
                p2FrameCounter = 0;

                p2FrameIndex++;
                if (p2FrameIndex >= 4) p2FrameIndex = 0;

                // ← THAY: Ưu tiên up/down/left/right, up/down reuse frame right/left dựa lastDirectionP1
                if (p2IsMovingUp || p2IsMovingDown || p2IsMovingRight || p2IsMovingLeft)  // Bất kỳ di chuyển nào
                {
                    if (lastDirectionP2 == "right")
                        pbPlayer2.Image = p2RightFrames[p2FrameIndex];
                    else if (lastDirectionP2 == "left")
                        pbPlayer2.Image = p2LeftFrames[p2FrameIndex];
                }
            }
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

            // Player1 chạm Goal1 (trái)
            if (pbPlayer1.Bounds.IntersectsWith(pbGoal1.Bounds))
            {
                // CHỈ đẩy nếu đang nhấn phím (đang cố di chuyển vào goal)
                bool p1Moving = wPressed || sPressed || aPressed || dPressed;
                if (p1Moving)
                {
                    if (pbPlayer1.Left <= pbGoal1.Right)               
                        pbPlayer1.Left = pbGoal1.Right + 1;                                                          
                }
                // Nếu đứng im → không làm gì → được dính sát goal thoải mái
            }

            // Player2 chạm Goal2 (phải)
            if (pbPlayer2.Bounds.IntersectsWith(pbGoal2.Bounds))
            {
                // CHỈ đẩy nếu đang nhấn phím
                bool p2Moving = upPressed || downPressed || leftPressed || rightPressed;
                if (p2Moving)
                {
                    if (pbPlayer2.Right >= pbGoal2.Left)
                        pbPlayer2.Left = pbGoal2.Left - pbPlayer2.Width - 1;                 
                }
                // Nếu đứng im → không đẩy → dính sát goal bình thường
            }
        }

        // VÒNG LẶP GAME (ANIMATION PLAYER 1)
        private void GameLoop()
        {
            // Xác định hướng Player1
            p1IsMovingLeft = aPressed;
            p1IsMovingRight = dPressed;
            p1IsMovingUp = wPressed;
            p1IsMovingDown = sPressed;


            p2IsMovingLeft = leftPressed;
            p2IsMovingRight = rightPressed;
            p2IsMovingUp = upPressed;
            p2IsMovingDown = downPressed;

            // Gọi animation
            AnimatePlayer1();
            AnimatePlayer2();

            // Gọi code gốc
            MovePaddles();
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
            rotationAngle += 7.2f;                    // Tăng góc xoay mỗi khung hình
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
            Random r = new Random();
            ballSpeedX = r.Next(0, 2) == 0 ? 4 : -4;   // Random trái/phải
            ballSpeedY = r.Next(0, 2) == 0 ? 4 : -4;   // Random lên/xuống
            bounceCount = 0;
            UpdateAllPositions();
        }

        // KIỂM TRA VA CHẠM (4 MẶT ĐỀU NẢY LẠI)
        private void CheckCollisions()
        {
            CheckPaddleBounce();
            // Goal Player 2 thắng (bóng vào pbGoal1)
            if (pbBall.Bounds.IntersectsWith(pbGoal1.Bounds))
            {
                // Cộng điểm Player 2
                scorePlayer2++;
                lastMoveP1 = DateTime.Now;       // coi như P1 vừa mới di chuyển
                lastMoveP2 = DateTime.Now;       // coi như P2 vừa mới di chuyển
                idleStartP1 = DateTime.MinValue; // reset bộ đếm idle của P1
                idleStartP2 = DateTime.MinValue; // reset bộ đếm idle của P2
                ResetBonusTimer();
                // Kiểm tra thắng cuộc
                if (scorePlayer2 >= 5)
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
                lastMoveP1 = DateTime.Now;       // coi như P1 vừa mới di chuyển
                lastMoveP2 = DateTime.Now;       // coi như P2 vừa mới di chuyển
                idleStartP1 = DateTime.MinValue; // reset bộ đếm idle của P1
                idleStartP2 = DateTime.MinValue; // reset bộ đếm idle của P2
                ResetBonusTimer();
                if (scorePlayer1 >= 5)
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

            // === BONUS ITEM VA CHẠM ===
            if (bonusActive && pbBonus.Visible)
            {
                // Ball chạm bonus → biến mất
                if (pbBall.Bounds.IntersectsWith(pbBonus.Bounds))
                {
                    PlaySound(bonusHitReader, bonusHitOutput);
                    ResetBonusTimer();
                    return;
                }

                // Player1 chạm → +1 điểm
                if (pbPlayer1.Bounds.IntersectsWith(pbBonus.Bounds))
                {
                    scorePlayer1++;
                    ResetBonusTimer();
                    // Kiểm tra thắng cuộc
                    if (scorePlayer1 >= 5)
                    {
                        ShowGameOver("PLAYER 1 THẮNG TRẬN!", $"TỈ SỐ: {scorePlayer1} - {scorePlayer2}");
                        return;
                    }
                    else
                    {
                        PlaySound(cheerReader, cheerOutput);
                    }                  
                    return;
                }

                // Player2 chạm → +1 điểm
                if (pbPlayer2.Bounds.IntersectsWith(pbBonus.Bounds))
                {
                    scorePlayer2++;                  
                    ResetBonusTimer();
                    // Kiểm tra thắng cuộc
                    if (scorePlayer2 >= 5)
                    {
                        ShowGameOver("PLAYER 2 THẮNG TRẬN!", $"TỈ SỐ: {scorePlayer1} - {scorePlayer2}");
                        return;
                    }
                    else
                    {
                        PlaySound(cheerReader, cheerOutput);
                    }                   
                    return;
                }
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
        }

        // NẢY VỢT + TĂNG TỐC ĐỘ 
        private void CheckPaddleBounce()
        {
            //Nảy vợt Player 1(trái) → bóng bay sang phải
            if (pbBall.Bounds.IntersectsWith(pbPlayer1.Bounds))
            {
                bounceCount++;
                lastBounceTime = DateTime.Now;
                if (kickReader != null) PlaySound(kickReader, kickOutput);

                // Kích hoạt hiệu ứng vụ nổ sau 5 tick
                Point ballCenter = new Point(pbBall.Left + pbBall.Width / 2, pbBall.Top + pbBall.Height / 2);
                TriggerExplosionAfter5Ticks(ballCenter, pbPlayer1.Width);

                int currentSpeed = Math.Abs(ballSpeedX);
                int newSpeed = Math.Min(currentSpeed + bounceCount * 10, 25);

                // Tính tâm bóng theo trục X
                float ballCenterX = pbBall.Left + pbBall.Width / 2f;
                float paddleRightEdge = pbPlayer1.Left;

                // PHÂN BIỆT CHÍNH XÁC: chạm mặt trước hay sau lưng
                if (ballCenterX < paddleRightEdge)
                {
                    // CHẠM MẶT TRƯỚC (mặt phải của vợt P1) → đánh mạnh về đối thủ     
                    ballSpeedX = -newSpeed;
                }
                else
                {
                    // CHẠM SAU LƯNG (bóng đã lọt qua mặt phải vợt) → nảy yếu + ngược lại
                    ballSpeedX = newSpeed * 2 / 3;   // chỉ 66% lực, về bên trái
                }
                return;
            }
            // Nảy vợt Player 2 (phải) → bóng bay sang trái
            if (pbBall.Bounds.IntersectsWith(pbPlayer2.Bounds))
            {
                bounceCount++;
                lastBounceTime = DateTime.Now;
                if (kickReader != null) PlaySound(kickReader, kickOutput);
                // KÍCH HOẠT HIỆU ỨNG VỤ NỔ SAU 5 TICK               
                Point ballCenter = new Point(pbBall.Left + pbBall.Width / 2, pbBall.Top + pbBall.Height / 2);
                TriggerExplosionAfter5Ticks(ballCenter, pbPlayer2.Width);

                int currentSpeed = Math.Abs(ballSpeedX);
                int newSpeed = Math.Min(currentSpeed + bounceCount * 10, 25);

                // Tính tâm bóng theo trục X
                float ballCenterX = pbBall.Left + pbBall.Width / 2f;
                float paddleRightEdge = pbPlayer2.Right;

                // PHÂN BIỆT CHÍNH XÁC: chạm mặt trước hay sau lưng
                if (ballCenterX < paddleRightEdge)
                {
                    //CHẠM MẶT TRƯỚC(mặt phải của vợt P1) → đánh mạnh về đối thủ
                    ballSpeedX = -newSpeed;
                }
                else
                {
                    //CHẠM SAU LƯNG(bóng đã lọt qua mặt phải vợt) → nảy yếu +ngược lại
                    ballSpeedX = newSpeed * 2 / 3;   // chỉ 66% lực, về bên trái
                }
                return;
            }
        }

        // KÍCH HOẠT HIỆU ỨNG VỤ NỔ SAU 5 TICK
        private void TriggerExplosionAfter5Ticks(Point ballCenter, int playerWidth)
        {
            if (explosionImage == null || explosionPending) return;

            // Tính vị trí cách bóng 40px theo hướng đang bay
            int offset = -5;
            int targetX = ballCenter.X + (ballSpeedX > 0 ? offset : -offset);
            int targetY = ballCenter.Y + (ballSpeedY > 0 ? offset : -offset);

            explosionTargetPos = new Point(targetX, targetY);
            explosionPlayerWidth = playerWidth;

            // Kích thước = so với player
            int size = playerWidth * 3 / 4 ;
            explosionEffect.Size = new Size(size, size);

            explosionDelayCount = 0;
            explosionPending = true;
            explosionDelayTimer.Start();
        }

        private void SpawnBonus()
        {
            if (gameStarted && !bonusActive)
            {
                int formWidth = ClientSize.Width;
                int formHeight = ClientSize.Height;

                // Random vị trí GIỮA MÀN HÌNH (20% - 80% để tránh biên + player)
                int x = random.Next((int)(formWidth * 0.2), (int)(formWidth * 0.8) - pbBonus.Width);
                int y = random.Next((int)(formHeight * 0.2), (int)(formHeight * 0.8) - pbBonus.Height);

                pbBonus.Left = x;
                pbBonus.Top = y;
                pbBonus.Visible = true;
                pbBonus.BringToFront();
                bonusActive = true;
                PlaySound(bonusAppearReader, bonusAppearOutput);
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

            DateTime now = DateTime.Now;

            // === PLAYER 1 ===
            bool p1Moving = wPressed || sPressed || aPressed || dPressed;

            if (p1Moving)
            {
                lastMoveP1 = now;
                idleStartP1 = DateTime.MinValue; // đang di chuyển → reset idle
            }
            else if (lastMoveP1 != DateTime.MinValue) // đã từng di chuyển ít nhất 1 lần
            {
                if (idleStartP1 == DateTime.MinValue)
                    idleStartP1 = now; // bắt đầu đếm idle

                if ((now - idleStartP1).TotalSeconds > 5)
                {
                    TriggerReferee(1);
                    return;
                }
            }

            // === PLAYER 2 ===
            bool p2Moving = upPressed || downPressed || leftPressed || rightPressed;

            if (p2Moving)
            {
                lastMoveP2 = now;
                idleStartP2 = DateTime.MinValue;
            }
            else if (lastMoveP2 != DateTime.MinValue)
            {
                if (idleStartP2 == DateTime.MinValue)
                    idleStartP2 = now;

                if ((now - idleStartP2).TotalSeconds > 5)
                {
                    TriggerReferee(2);
                    return;
                }
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

                // DỪNG GAME HOÀN TOÀN KHI TRỌNG TÀI XUẤT HIỆN
                TimerPongGame.Stop();

                // Phát còi + cổ vũ
                if (whistleReader != null) PlaySound(whistleReader, whistleOutput);


                // Hiện trọng tài
                pbReferee.Visible = true;
                pbReferee.BringToFront();
                pbReferee.Left = (ClientSize.Width - pbReferee.Width) / 2;
                pbReferee.Top = (ClientSize.Height - pbReferee.Height) / 2;

                // Cộng điểm
                if (idlePlayer == 1) 
                { 
                    scorePlayer2++; 
                }
                else { 
                    scorePlayer1++;
                }

                ResetBonusTimer();

                lastMoveP1 = DateTime.Now;       // coi như P1 vừa mới di chuyển
                lastMoveP2 = DateTime.Now;       // coi như P2 vừa mới di chuyển
                idleStartP1 = DateTime.MinValue; // reset bộ đếm idle của P1
                idleStartP2 = DateTime.MinValue; // reset bộ đếm idle của P2

                ResetBall();
                this.Invalidate();


                // ẨN SAU 0 GIÂY + KIỂM TRA THẮNG
                Task.Delay(3000).ContinueWith(_ =>
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        pbReferee.Visible = false;
                        refereeActive = false;

                        // KHỞI ĐỘNG LẠI TOÀN BỘ GAME
                        TimerPongGame.Start();
                        

                        // KIỂM TRA AI ĐẠT 3 BÀN TRƯỚC
                        if (scorePlayer2 >= 5)
                        {
                            ShowGameOver("PLAYER 2 THẮNG TRẬN!", $"TỈ SỐ: {scorePlayer1} - {scorePlayer2}");
                        }
                        else if (scorePlayer1 >= 5)
                        {
                            ShowGameOver("PLAYER 1 THẮNG TRẬN!", $"TỈ SỐ: {scorePlayer1} - {scorePlayer2}");
                        }
                    }));
                });
            }));
        }

        // RESET BONUS TIMER
        private void ResetBonusTimer()
        {
            // XÓA BONUS ĐANG HIỆN TRÊN SÂN (nếu có)
            pbBonus.Visible = false;
            bonusActive = false;
            if (bonusSpawnTimer != null)
            {
                bonusSpawnTimer.Stop();
                bonusSpawnTimer.Start(); // đếm lại 7 giây từ đầu
            }
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
                else if (reader == bonusAppearReader)
                    reader.Volume = 3.8f;           // BONUS XUẤT HIỆN → TO VÃI TRỜI!
                else if (reader == bonusHitReader)
                    reader.Volume = 4.2f;

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
            bonusSpawnTimer.Stop();
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

            // Bắt đầu timer
            TimerPongGame.Interval = 6;
            TimerPongGame.Start();
            // Bắt đầu bonus spawn timer
            bonusSpawnTimer.Start();
            bgMusicOutput?.Play();        // BẮT ĐẦU NHẠC NỀN KHI ẤN START
            // ẩn trọng tài
            pbReferee.Visible = false;   // Đảm bảo trọng tài ẩn khi bắt đầu trận mới
            refereeActive = false;       // Reset trạng thái phạt
        }

    }
}