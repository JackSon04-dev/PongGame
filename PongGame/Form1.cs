    using System; // Dành cho Random và Timer
    namespace PongGame
    {
        public partial class Form1 : Form
        {
            // Tốc độ di chuyển của bóng (pixel mỗi lần tick)
            private int ballSpeedX = 20;
            private int ballSpeedY = 20;

            // Tốc độ di chuyển của player
            private const int PADDLE_SPEED = 100;

            // Góc xoay hiện tại của bóng
            private float rotationAngle = 0f;
            // Lưu hình bóng gốc để xoay mãi mãi
            private Bitmap originalBallImage;


            // === TRẠNG THÁI PHÍM  ===
            private bool wPressed = false;
            private bool sPressed = false;
            private bool aPressed = false;
            private bool dPressed = false;

            private bool upPressed = false;
            private bool downPressed = false;
            private bool leftPressed = false;
            private bool rightPressed = false;

            // Biến khóa di chuyển vợt khi đang countdown
            private bool isCountdownActive = false;
        public Form1()
            {
                InitializeComponent();

                // Bật KeyPreview để form nhận sự kiện phím
                this.KeyPreview = true;

                // Gán sự kiện phím
                this.KeyDown += Form1_KeyDown;
                this.KeyUp += Form1_KeyUp;
            }

            private void Form1_Load(object sender, EventArgs e)
            {
                // Bắt đầu timer khi form load
                TimerPongGame.Interval = 20; // Càng nhỏ càng mượt
                TimerPongGame.Enabled = true;
                TimerPongGame.Start();

                // LƯU HÌNH GỐC MỘT LẦN DUY NHẤT
                if (pbBall.Image != null)
                {
                    originalBallImage = new Bitmap(pbBall.Image);
                }

                pbPlayer1.BackColor = Color.Transparent;
                pbPlayer2.BackColor = Color.Transparent;
                pbBall.BackColor = Color.Transparent;
            }

            // XỬ LÝ NHẤN PHÍM
            private void Form1_KeyDown(object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    // Player 1: W A S D
                    case Keys.W: wPressed = true; break;
                    case Keys.S: sPressed = true; break;
                    case Keys.A: aPressed = true; break;
                    case Keys.D: dPressed = true; break;

                    // Player 2: Mũi tên
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

            private void TimerPongGame_Tick(object sender, EventArgs e)
            {
                // === 0. DI CHUYỂN PLAYER ===
                MovePaddles();

                // === 1. Di chuyển bóng ===
                pbBall.Left += ballSpeedX;
                pbBall.Top += ballSpeedY;

                // === 2. Xoay bóng (hiệu ứng quay) ===
                rotationAngle += 10f; // Tốc độ quay
                if (rotationAngle >= 360f) rotationAngle = 0f;

                // Xoay từ HÌNH GỐC (originalBallImage) → không bị mờ
                if (originalBallImage != null)
                {
                    Bitmap rotated = RotateImage(originalBallImage, rotationAngle);
                    pbBall.Image?.Dispose(); // Giải phóng hình cũ
                    pbBall.Image = rotated;
                }

                // === 3. XỬ LÝ VA CHẠM VỚI 4 VIỀN + TÍNH ĐIỂM + GAME OVER ===
                if (pbBall.Left <= 0)
                {
                    // BÓNG RA KHỎI BIỂN TRÁI → PLAYER 1 THUA
                    
                    ShowGameOver("Player 2 thắng!", $"Điểm: 1");
                }
                else if (pbBall.Right >= this.ClientSize.Width)
                {
                    // BÓNG RA KHỎI BIỂN PHẢI → PLAYER 2 THUA
   
                    ShowGameOver("Player 1 thắng!", $"Điểm: 1");
                }
                else
                {
                    // Chỉ đảo chiều nếu chạm trên/dưới
                    if (pbBall.Top <= 0 || pbBall.Bottom >= this.ClientSize.Height)
                    {
                        ballSpeedY = -ballSpeedY;
                        if (pbBall.Top <= 0) pbBall.Top = 0;
                        if (pbBall.Bottom >= this.ClientSize.Height) pbBall.Top = this.ClientSize.Height - pbBall.Height;
                    }
                }
                // === 4. XỬ LÝ NẢY VỢT (4 GÓC) ===
                CheckPaddleBounce();
        }
        // ========================================
        // HIỂN THỊ NGƯỜI THẮNG + THOÁT GAME (KHÔNG CHƠI TIẾP)
        // ========================================
        private void ShowGameOver(string winner, string score)
        {
            TimerPongGame.Stop();

            MessageBox.Show(
                $"{winner}\n\n{score}\n\nCảm ơn bạn đã chơi!\nNhấn OK để thoát.",
                "GAME OVER",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            this.Close(); // Thoát chương trình luôn
        }
    
        // Hàm xoay hình ảnh
        private Bitmap RotateImage(Bitmap bmp, float angle)
            {
                int width = bmp.Width;
                int height = bmp.Height;
                Bitmap rotated = new Bitmap(width, height);

                using (Graphics g = Graphics.FromImage(rotated))
                {
                    g.TranslateTransform(width / 2f, height / 2f);
                    g.RotateTransform(angle);
                    g.TranslateTransform(-width / 2f, -height / 2f);
                    g.DrawImage(bmp, 0, 0, width, height);
                }
                return rotated;
            }

            // DI CHUYỂN PLAYER
            private void MovePaddles()
            {
            // NẾU ĐANG COUNTDOWN → KHÔNG CHO DI CHUYỂN VỢT
            if (isCountdownActive) return;
            // Player 1: W A S D
            if (wPressed && pbPlayer1.Top > 0)
                    pbPlayer1.Top -= PADDLE_SPEED;
                if (sPressed && pbPlayer1.Bottom < this.ClientSize.Height)
                    pbPlayer1.Top += PADDLE_SPEED;
                if (aPressed && pbPlayer1.Left > 0)
                    pbPlayer1.Left -= PADDLE_SPEED;
                if (dPressed && pbPlayer1.Right < this.ClientSize.Width)
                    pbPlayer1.Left += PADDLE_SPEED;

                // Player 2: Mũi tên
                if (upPressed && pbPlayer2.Top > 0)
                    pbPlayer2.Top -= PADDLE_SPEED;
                if (downPressed && pbPlayer2.Bottom < this.ClientSize.Height)
                    pbPlayer2.Top += PADDLE_SPEED;
                if (leftPressed && pbPlayer2.Left > 0)
                    pbPlayer2.Left -= PADDLE_SPEED;
                if (rightPressed && pbPlayer2.Right < this.ClientSize.Width)
                    pbPlayer2.Left += PADDLE_SPEED;
            }
            // ========================================
            // ENUM: XÁC ĐỊNH 4 GÓC CỦA VỢT
            // ========================================
            private enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }

            // ========================================
            // HÀM: KIỂM TRA BÓNG CHẠM GÓC NÀO CỦA VỢT
            // ========================================
            private bool IsBallHittingCorner(PictureBox ball, PictureBox paddle, Corner corner)
            {
                Rectangle cornerArea = new Rectangle();

                // Xác định vùng 4 góc (kích thước bằng bóng)
                switch (corner)
                {
                    case Corner.TopLeft:
                        cornerArea = new Rectangle(paddle.Left, paddle.Top, ball.Width, ball.Height);
                        break;
                    case Corner.TopRight:
                        cornerArea = new Rectangle(paddle.Right - ball.Width, paddle.Top, ball.Width, ball.Height);
                        break;
                    case Corner.BottomLeft:
                        cornerArea = new Rectangle(paddle.Left, paddle.Bottom - ball.Height, ball.Width, ball.Height);
                        break;
                    case Corner.BottomRight:
                        cornerArea = new Rectangle(paddle.Right - ball.Width, paddle.Bottom - ball.Height, ball.Width, ball.Height);
                        break;
                }

                return ball.Bounds.IntersectsWith(cornerArea);
            }

            // ========================================
            // HÀM: TÍNH NẢY KHI CHẠM 4 GÓC
            // ========================================
            private void HandleCornerBounce(PictureBox paddle, bool isTopCorner)
            {
                // Tăng tốc cơ bản
                int baseSpeed = Math.Abs(ballSpeedX) + 2;

                // Xác định hướng X: Player 1 → sang phải, Player 2 → sang trái
                ballSpeedX = (paddle == pbPlayer1) ? baseSpeed : -baseSpeed;

                // Nảy mạnh theo góc
                ballSpeedY = isTopCorner ? -baseSpeed : baseSpeed; // Trên → lên, Dưới → xuống

                // Tăng tốc xoay khi nảy góc
                rotationAngle += 40f;
            }
            // ========================================
            // XỬ LÝ VA CHẠM VỢT + NẢY 4 GÓC
            // ========================================
            private void CheckPaddleBounce()
            {
                // === PLAYER 1 (TRÁI) ===
                if (pbBall.Bounds.IntersectsWith(pbPlayer1.Bounds))
                {
                    // Kiểm tra 4 góc
                    if (IsBallHittingCorner(pbBall, pbPlayer1, Corner.TopLeft) ||
                        IsBallHittingCorner(pbBall, pbPlayer1, Corner.TopRight))
                    {
                        HandleCornerBounce(pbPlayer1, true); // Góc trên
                    }
                    else if (IsBallHittingCorner(pbBall, pbPlayer1, Corner.BottomLeft) ||
                             IsBallHittingCorner(pbBall, pbPlayer1, Corner.BottomRight))
                    {
                        HandleCornerBounce(pbPlayer1, false); // Góc dưới
                    }
                    else
                    {
                        // Nảy giữa vợt → thẳng + tăng nhẹ
                        ballSpeedX = Math.Abs(ballSpeedX) + 1;
                        ballSpeedY = 0;
                    }

                    // Đẩy bóng ra khỏi vợt
                    pbBall.Left = pbPlayer1.Right + 1;
                }

                // === PLAYER 2 (PHẢI) ===
                if (pbBall.Bounds.IntersectsWith(pbPlayer2.Bounds))
                {
                    if (IsBallHittingCorner(pbBall, pbPlayer2, Corner.TopLeft) ||
                        IsBallHittingCorner(pbBall, pbPlayer2, Corner.TopRight))
                    {
                        HandleCornerBounce(pbPlayer2, true);
                    }
                    else if (IsBallHittingCorner(pbBall, pbPlayer2, Corner.BottomLeft) ||
                             IsBallHittingCorner(pbBall, pbPlayer2, Corner.BottomRight))
                    {
                        HandleCornerBounce(pbPlayer2, false);
                    }
                    else
                    {
                        ballSpeedX = -Math.Abs(ballSpeedX) - 1;
                        ballSpeedY = 0;
                    }

                    pbBall.Left = pbPlayer2.Left - pbBall.Width - 1;
                }
            }

            // ========================================
            // CÁC SỰ KIỆN CLICK (chưa dùng, giữ lại)
            // ========================================
            private void pbBall_Click(object sender, EventArgs e)
            {

            }

            private void pbPlayer1_Click(object sender, EventArgs e)
            {

            }

            private void pbPlayer2_Click(object sender, EventArgs e)
            {

            }
        }
    }
