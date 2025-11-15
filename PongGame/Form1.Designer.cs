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
            pbBall = new PictureBox();
            pbPlayer1 = new PictureBox();
            pbPlayer2 = new PictureBox();
            TimerPongGame = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)pbBall).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbPlayer1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbPlayer2).BeginInit();
            SuspendLayout();
            // 
            // pbBall
            // 
            pbBall.Image = Properties.Resources.Ball_removebg_preview;
            pbBall.Location = new Point(888, 441);
            pbBall.Margin = new Padding(3, 4, 3, 4);
            pbBall.Name = "pbBall";
            pbBall.Size = new Size(67, 85);
            pbBall.SizeMode = PictureBoxSizeMode.Zoom;
            pbBall.TabIndex = 0;
            pbBall.TabStop = false;
            pbBall.Click += pbBall_Click;
            // 
            // pbPlayer1
            // 
            pbPlayer1.Image = Properties.Resources.player1_removebg_preview;
            pbPlayer1.Location = new Point(77, 396);
            pbPlayer1.Margin = new Padding(3, 4, 3, 4);
            pbPlayer1.Name = "pbPlayer1";
            pbPlayer1.Size = new Size(62, 157);
            pbPlayer1.SizeMode = PictureBoxSizeMode.Zoom;
            pbPlayer1.TabIndex = 1;
            pbPlayer1.TabStop = false;
            pbPlayer1.Click += pbPlayer1_Click;
            // 
            // pbPlayer2
            // 
            pbPlayer2.Image = Properties.Resources.player2_removebg_preview;
            pbPlayer2.Location = new Point(1616, 406);
            pbPlayer2.Margin = new Padding(3, 4, 3, 4);
            pbPlayer2.Name = "pbPlayer2";
            pbPlayer2.Size = new Size(65, 157);
            pbPlayer2.SizeMode = PictureBoxSizeMode.Zoom;
            pbPlayer2.TabIndex = 2;
            pbPlayer2.TabStop = false;
            pbPlayer2.Click += pbPlayer2_Click;
            // 
            // TimerPongGame
            // 
            TimerPongGame.Tick += TimerPongGame_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.GradientInactiveCaption;
            BackgroundImage = Properties.Resources.SanCo;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1840, 965);
            Controls.Add(pbPlayer2);
            Controls.Add(pbPlayer1);
            Controls.Add(pbBall);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pbBall).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbPlayer1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbPlayer2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pbBall;
        private PictureBox pbPlayer1;
        private PictureBox pbPlayer2;
        private System.Windows.Forms.Timer TimerPongGame;
    }
}
