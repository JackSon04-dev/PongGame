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
            pbBall.Image = Properties.Resources.Ball_removebg_preview;
            pbBall.Location = new Point(288, 196);
            pbBall.Margin = new Padding(3, 4, 3, 4);
            pbBall.Name = "pbBall";
            pbBall.Size = new Size(89, 90);
            pbBall.SizeMode = PictureBoxSizeMode.Zoom;
            pbBall.TabIndex = 0;
            pbBall.TabStop = false;
            pbBall.Click += pbBall_Click;
            // 
            // pbPlayer1
            // 
            pbPlayer1.Image = Properties.Resources.player1_removebg_preview;
            pbPlayer1.Location = new Point(12, 13);
            pbPlayer1.Margin = new Padding(3, 4, 3, 4);
            pbPlayer1.Name = "pbPlayer1";
            pbPlayer1.Size = new Size(82, 159);
            pbPlayer1.SizeMode = PictureBoxSizeMode.Zoom;
            pbPlayer1.TabIndex = 1;
            pbPlayer1.TabStop = false;
            pbPlayer1.Click += pbPlayer1_Click;
            // 
            // pbPlayer2
            // 
            pbPlayer2.Image = Properties.Resources.player2_removebg_preview;
            pbPlayer2.Location = new Point(111, 13);
            pbPlayer2.Margin = new Padding(3, 4, 3, 4);
            pbPlayer2.Name = "pbPlayer2";
            pbPlayer2.Size = new Size(89, 159);
            pbPlayer2.SizeMode = PictureBoxSizeMode.Zoom;
            pbPlayer2.TabIndex = 2;
            pbPlayer2.TabStop = false;
            pbPlayer2.Click += pbPlayer2_Click;
            // 
            // TimerPongGame
            // 
            TimerPongGame.Tick += TimerPongGame_Tick;
            // 
            // pbGoal2
            // 
            pbGoal2.BackColor = SystemColors.MenuHighlight;
            pbGoal2.BackgroundImage = (Image)resources.GetObject("pbGoal2.BackgroundImage");
            pbGoal2.Location = new Point(671, 23);
            pbGoal2.Name = "pbGoal2";
            pbGoal2.Size = new Size(50, 10);
            pbGoal2.TabIndex = 3;
            pbGoal2.TabStop = false;
            pbGoal2.Click += pbGoal2_Click;
            // 
            // pbGoal1
            // 
            pbGoal1.BackgroundImage = Properties.Resources.KhungThanh;
            pbGoal1.Location = new Point(532, 23);
            pbGoal1.Name = "pbGoal1";
            pbGoal1.Size = new Size(53, 10);
            pbGoal1.TabIndex = 4;
            pbGoal1.TabStop = false;
            pbGoal1.Click += pbGoal1_Click;
            // 
            // btnStart
            // 
            btnStart.BackColor = SystemColors.WindowFrame;
            btnStart.Location = new Point(12, 196);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(200, 100);
            btnStart.TabIndex = 5;
            btnStart.Text = "Bắt Đầu Chơi";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // pbReferee
            // 
            pbReferee.BackColor = Color.Transparent;
            pbReferee.Image = Properties.Resources.TrongTai_removebg_preview;
            pbReferee.Location = new Point(251, 2);
            pbReferee.Name = "pbReferee";
            pbReferee.Size = new Size(173, 159);
            pbReferee.SizeMode = PictureBoxSizeMode.Zoom;
            pbReferee.TabIndex = 6;
            pbReferee.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1343, 530);
            Controls.Add(pbReferee);
            Controls.Add(btnStart);
            Controls.Add(pbGoal1);
            Controls.Add(pbGoal2);
            Controls.Add(pbPlayer2);
            Controls.Add(pbPlayer1);
            Controls.Add(pbBall);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Text = "wd";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pbBall).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbPlayer1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbPlayer2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbGoal2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbGoal1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbReferee).EndInit();
            ResumeLayout(false);
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
