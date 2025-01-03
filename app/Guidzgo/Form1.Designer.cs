namespace Guidzgo
{
	partial class Form1
	{
		/// <summary>
		/// Vyžaduje se proměnná návrháře.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Uvolněte všechny používané prostředky.
		/// </summary>
		/// <param name="disposing">hodnota true, když by se měl spravovaný prostředek odstranit; jinak false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Kód generovaný Návrhářem Windows Form

		/// <summary>
		/// Metoda vyžadovaná pro podporu Návrháře - neupravovat
		/// obsah této metody v editoru kódu.
		/// </summary>
		private void InitializeComponent()
		{
			this.championC = new System.Windows.Forms.CheckBox();
			this.challengerC = new System.Windows.Forms.CheckBox();
			this.legendC = new System.Windows.Forms.CheckBox();
			this.contenderC = new System.Windows.Forms.CheckBox();
			this.amateurC = new System.Windows.Forms.CheckBox();
			this.textBox6 = new System.Windows.Forms.TextBox();
			this.textBox7 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.amateurT = new System.Windows.Forms.TextBox();
			this.contenderT = new System.Windows.Forms.TextBox();
			this.legendT = new System.Windows.Forms.TextBox();
			this.challengerT = new System.Windows.Forms.TextBox();
			this.championT = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.connLabel = new System.Windows.Forms.Label();
			this.logBox = new System.Windows.Forms.TextBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// championC
			// 
			this.championC.AutoSize = true;
			this.championC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.championC.Location = new System.Drawing.Point(6, 19);
			this.championC.Name = "championC";
			this.championC.Size = new System.Drawing.Size(113, 24);
			this.championC.TabIndex = 5;
			this.championC.Text = "CHAMPION";
			this.championC.UseVisualStyleBackColor = true;
			// 
			// challengerC
			// 
			this.challengerC.AutoSize = true;
			this.challengerC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.challengerC.Location = new System.Drawing.Point(6, 49);
			this.challengerC.Name = "challengerC";
			this.challengerC.Size = new System.Drawing.Size(138, 24);
			this.challengerC.TabIndex = 6;
			this.challengerC.Text = "CHALLENGER";
			this.challengerC.UseVisualStyleBackColor = true;
			// 
			// legendC
			// 
			this.legendC.AutoSize = true;
			this.legendC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.legendC.Location = new System.Drawing.Point(6, 79);
			this.legendC.Name = "legendC";
			this.legendC.Size = new System.Drawing.Size(95, 24);
			this.legendC.TabIndex = 7;
			this.legendC.Text = "LEGEND";
			this.legendC.UseVisualStyleBackColor = true;
			// 
			// contenderC
			// 
			this.contenderC.AutoSize = true;
			this.contenderC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.contenderC.Location = new System.Drawing.Point(6, 109);
			this.contenderC.Name = "contenderC";
			this.contenderC.Size = new System.Drawing.Size(128, 24);
			this.contenderC.TabIndex = 8;
			this.contenderC.Text = "CONTENDER";
			this.contenderC.UseVisualStyleBackColor = true;
			// 
			// amateurC
			// 
			this.amateurC.AutoSize = true;
			this.amateurC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.amateurC.Location = new System.Drawing.Point(6, 139);
			this.amateurC.Name = "amateurC";
			this.amateurC.Size = new System.Drawing.Size(107, 24);
			this.amateurC.TabIndex = 9;
			this.amateurC.Text = "AMATEUR";
			this.amateurC.UseVisualStyleBackColor = true;
			// 
			// textBox6
			// 
			this.textBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.textBox6.Location = new System.Drawing.Point(298, 32);
			this.textBox6.Name = "textBox6";
			this.textBox6.Size = new System.Drawing.Size(103, 26);
			this.textBox6.TabIndex = 10;
			// 
			// textBox7
			// 
			this.textBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.textBox7.Location = new System.Drawing.Point(298, 84);
			this.textBox7.Name = "textBox7";
			this.textBox7.Size = new System.Drawing.Size(103, 26);
			this.textBox7.TabIndex = 11;
			// 
			// button1
			// 
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.button1.Location = new System.Drawing.Point(298, 143);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(103, 51);
			this.button1.TabIndex = 12;
			this.button1.Text = "Send";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.amateurT);
			this.groupBox1.Controls.Add(this.contenderT);
			this.groupBox1.Controls.Add(this.legendT);
			this.groupBox1.Controls.Add(this.challengerT);
			this.groupBox1.Controls.Add(this.championT);
			this.groupBox1.Controls.Add(this.amateurC);
			this.groupBox1.Controls.Add(this.contenderC);
			this.groupBox1.Controls.Add(this.championC);
			this.groupBox1.Controls.Add(this.legendC);
			this.groupBox1.Controls.Add(this.challengerC);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(267, 178);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Labels";
			// 
			// amateurT
			// 
			this.amateurT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.amateurT.Location = new System.Drawing.Point(150, 137);
			this.amateurT.Name = "amateurT";
			this.amateurT.Size = new System.Drawing.Size(100, 26);
			this.amateurT.TabIndex = 14;
			// 
			// contenderT
			// 
			this.contenderT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.contenderT.Location = new System.Drawing.Point(150, 107);
			this.contenderT.Name = "contenderT";
			this.contenderT.Size = new System.Drawing.Size(100, 26);
			this.contenderT.TabIndex = 13;
			// 
			// legendT
			// 
			this.legendT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.legendT.Location = new System.Drawing.Point(150, 77);
			this.legendT.Name = "legendT";
			this.legendT.Size = new System.Drawing.Size(100, 26);
			this.legendT.TabIndex = 12;
			// 
			// challengerT
			// 
			this.challengerT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.challengerT.Location = new System.Drawing.Point(150, 47);
			this.challengerT.Name = "challengerT";
			this.challengerT.Size = new System.Drawing.Size(100, 26);
			this.challengerT.TabIndex = 11;
			// 
			// championT
			// 
			this.championT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.championT.Location = new System.Drawing.Point(150, 17);
			this.championT.Name = "championT";
			this.championT.Size = new System.Drawing.Size(100, 26);
			this.championT.TabIndex = 10;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.label1.Location = new System.Drawing.Point(298, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(135, 20);
			this.label1.TabIndex = 14;
			this.label1.Text = "Default Cooldown";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.label2.Location = new System.Drawing.Point(298, 61);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(143, 20);
			this.label2.TabIndex = 15;
			this.label2.Text = "Captcha Cooldown";
			// 
			// connLabel
			// 
			this.connLabel.AutoSize = true;
			this.connLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.connLabel.Location = new System.Drawing.Point(294, 119);
			this.connLabel.Name = "connLabel";
			this.connLabel.Size = new System.Drawing.Size(129, 20);
			this.connLabel.TabIndex = 16;
			this.connLabel.Text = "PLACEHOLDER";
			// 
			// logBox
			// 
			this.logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.logBox.Font = new System.Drawing.Font("Consolas", 8.25F);
			this.logBox.Location = new System.Drawing.Point(12, 200);
			this.logBox.Multiline = true;
			this.logBox.Name = "logBox";
			this.logBox.ReadOnly = true;
			this.logBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.logBox.Size = new System.Drawing.Size(416, 270);
			this.logBox.TabIndex = 17;
			this.logBox.Text = "Logs appear here\r\n";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::Guidzgo.Properties.Resources.Term;
			this.pictureBox1.Location = new System.Drawing.Point(407, 143);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(26, 47);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 18;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
			this.pictureBox1.DoubleClick += new System.EventHandler(this.pictureBox1_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(440, 482);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.logBox);
			this.Controls.Add(this.connLabel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textBox7);
			this.Controls.Add(this.textBox6);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Form1";
			this.Text = "KeyDrop-BOT";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.CheckBox championC;
		private System.Windows.Forms.CheckBox challengerC;
		private System.Windows.Forms.CheckBox legendC;
		private System.Windows.Forms.CheckBox contenderC;
		private System.Windows.Forms.CheckBox amateurC;
		private System.Windows.Forms.TextBox textBox6;
		private System.Windows.Forms.TextBox textBox7;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox amateurT;
		private System.Windows.Forms.TextBox contenderT;
		private System.Windows.Forms.TextBox legendT;
		private System.Windows.Forms.TextBox challengerT;
		private System.Windows.Forms.TextBox championT;
		private System.Windows.Forms.Label connLabel;
		private System.Windows.Forms.TextBox logBox;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}

