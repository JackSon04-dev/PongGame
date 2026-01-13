namespace PongGame
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            pbBall = new PictureBox();
            pbPlayer1 = new PictureBox();
            pbPlayer2 = new PictureBox();
            TimerPongGame = new System.Windows.Forms.Timer(components);
            pbGoal2 = new PictureBox();
            pbGoal1 = new PictureBox();
            btnStart = new Button();
            pbReferee = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pbBall).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbPlayer1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbPlayer2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbGoal2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbGoal1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbReferee).BeginInit();
            SuspendLayout();
            // 
            // pbBall
            // 
            pbBall.BackColor = Color.Transparent;
            pbBall.Image = Properties.Resources.MT;
            pbBall.Location = new Point(562, 256);
            pbBall.Margin = new Padding(3, 4, 3, 4);
            pbBall.Name = "pbBall";
            pbBall.Size = new Size(84, 81);
            pbBall.SizeMode = PictureBoxSizeMode.StretchImage;
            pbBall.TabIndex = 0;
            pbBall.TabStop = false;
            pbBall.Click += pbBall_Click;
            // 
            // pbPlayer1
            // 
            pbPlayer1.BackColor = Color.Transparent;
            pbPlayer1.Image = Properties.Resources.player1_right3;
            pbPlayer1.Location = new Point(543, 256);
            pbPlayer1.Margin = new Padding(3, 4, 3, 4);
            pbPlayer1.Name = "pbPlayer1";
            pbPlayer1.Size = new Size(113, 144);
            pbPlayer1.SizeMode = PictureBoxSizeMode.Zoom;
            pbPlayer1.TabIndex = 1;
            pbPlayer1.TabStop = false;
            pbPlayer1.Click += pbPlayer1_Click;
            // 
            // pbPlayer2
            // 
            pbPlayer2.BackColor = Color.Transparent;
            pbPlayer2.Image = Properties.Resources.player2_left3;
            pbPlayer2.Location = new Point(411, 32);
            pbPlayer2.Margin = new Padding(3, 4, 3, 4);
            pbPlayer2.Name = "pbPlayer2";
            pbPlayer2.Size = new Size(113, 145);
            pbPlayer2.SizeMode = PictureBoxSizeMode.Zoom;
            pbPlayer2.TabIndex = 2;
            pbPlayer2.TabStop = false;
            pbPlayer2.Click += pbPlayer2_Click;
            // 
            // TimerPongGame
            // 
            TimerPongGame.Interval = 1000;
            TimerPongGame.Tick += TimerPongGame_Tick;
            // 
            // pbGoal2
            // 
            pbGoal2.BackColor = Color.Transparent;
            pbGoal2.BackgroundImage = Properties.Resources.spaceship21;
            pbGoal2.BackgroundImageLayout = ImageLayout.Stretch;
            pbGoal2.Location = new Point(184, 12);
            pbGoal2.Name = "pbGoal2";
            pbGoal2.Size = new Size(124, 273);
            pbGoal2.TabIndex = 3;
            pbGoal2.TabStop = false;
            pbGoal2.Click += pbGoal2_Click;
            // 
            // pbGoal1
            // 
            pbGoal1.BackColor = Color.Transparent;
            pbGoal1.BackgroundImage = Properties.Resources.spaceship1;
            pbGoal1.BackgroundImageLayout = ImageLayout.Stretch;
            pbGoal1.ErrorImage = (Image)resources.GetObject("pbGoal1.ErrorImage");
            pbGoal1.Location = new Point(25, 12);
            pbGoal1.Name = "pbGoal1";
            pbGoal1.Size = new Size(122, 273);
            pbGoal1.TabIndex = 4;
            pbGoal1.TabStop = false;
            pbGoal1.Click += pbGoal1_Click;
            // 
            // btnStart
            // 
            btnStart.AutoSize = true;
            btnStart.BackColor = Color.Tomato;
            btnStart.BackgroundImageLayout = ImageLayout.Stretch;
            btnStart.Font = new Font("Perpetua", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnStart.Location = new Point(517, 418);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(184, 78);
            btnStart.TabIndex = 5;
            btnStart.Text = "Bắt Đầu Chơi";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // pbReferee
            // 
            pbReferee.BackColor = Color.Transparent;
            pbReferee.Image = Properties.Resources.batman;
            pbReferee.Location = new Point(530, -1);
            pbReferee.Name = "pbReferee";
            pbReferee.Size = new Size(171, 235);
            pbReferee.SizeMode = PictureBoxSizeMode.Zoom;
            pbReferee.TabIndex = 6;
            pbReferee.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Desktop;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1343, 530);
            Controls.Add(pbReferee);
            Controls.Add(btnStart);
            Controls.Add(pbGoal1);
            Controls.Add(pbGoal2);
            Controls.Add(pbPlayer2);
            Controls.Add(pbPlayer1);
            Controls.Add(pbBall);
            DoubleBuffered = true;
            ForeColor = SystemColors.ActiveCaptionText;
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            StartPosition = FormStartPosition.WindowsDefaultBounds;
            Text = "A";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pbBall).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbPlayer1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbPlayer2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbGoal2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbGoal1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbReferee).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pbBall;
        private PictureBox pbPlayer1;
        private PictureBox pbPlayer2;
        private System.Windows.Forms.Timer TimerPongGame;
        private PictureBox pbGoal2;
        private PictureBox pbGoal1;
        private Button btnStart;
        private PictureBox pbReferee;
    }
}
