namespace SampleApp
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txtIndexFolder = new System.Windows.Forms.TextBox();
			this.button6 = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.txtWhere = new System.Windows.Forms.TextBox();
			this.button7 = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.button1 = new System.Windows.Forms.Button();
			this.lblIndexer = new System.Windows.Forms.Label();
			this.button5 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.btnStart = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.lblStatus = new System.Windows.Forms.Label();
			this.txtSearch = new System.Windows.Forms.TextBox();
			this.btnSearch = new System.Windows.Forms.Button();
			this.cbUseStopList = new System.Windows.Forms.CheckBox();
			this.cbIgnoreNumeric = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.txtIndexFolder);
			this.groupBox1.Controls.Add(this.button6);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(622, 63);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "1. Set Index Storage Directory";
			// 
			// txtIndexFolder
			// 
			this.txtIndexFolder.Location = new System.Drawing.Point(66, 23);
			this.txtIndexFolder.Name = "txtIndexFolder";
			this.txtIndexFolder.Size = new System.Drawing.Size(550, 23);
			this.txtIndexFolder.TabIndex = 1;
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(6, 22);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(40, 23);
			this.button6.TabIndex = 0;
			this.button6.Text = "...";
			this.button6.UseVisualStyleBackColor = true;
			this.button6.Click += new System.EventHandler(this.button6_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.cbIgnoreNumeric);
			this.groupBox2.Controls.Add(this.cbUseStopList);
			this.groupBox2.Controls.Add(this.txtWhere);
			this.groupBox2.Controls.Add(this.button7);
			this.groupBox2.Location = new System.Drawing.Point(12, 81);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(622, 85);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "2. Set Folder to Index";
			// 
			// txtWhere
			// 
			this.txtWhere.Location = new System.Drawing.Point(66, 20);
			this.txtWhere.Name = "txtWhere";
			this.txtWhere.Size = new System.Drawing.Size(550, 23);
			this.txtWhere.TabIndex = 1;
			// 
			// button7
			// 
			this.button7.Location = new System.Drawing.Point(6, 19);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(40, 23);
			this.button7.TabIndex = 0;
			this.button7.Text = "...";
			this.button7.UseVisualStyleBackColor = true;
			this.button7.Click += new System.EventHandler(this.button7_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.button1);
			this.groupBox3.Controls.Add(this.lblIndexer);
			this.groupBox3.Controls.Add(this.button5);
			this.groupBox3.Controls.Add(this.button3);
			this.groupBox3.Controls.Add(this.button4);
			this.groupBox3.Controls.Add(this.button2);
			this.groupBox3.Controls.Add(this.btnStop);
			this.groupBox3.Controls.Add(this.btnStart);
			this.groupBox3.Location = new System.Drawing.Point(12, 172);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(622, 83);
			this.groupBox3.TabIndex = 0;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "3. Start/Stop Indexer";
			this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(53, 51);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 8;
			this.button1.Text = "Load Hoot";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// lblIndexer
			// 
			this.lblIndexer.AutoSize = true;
			this.lblIndexer.Location = new System.Drawing.Point(8, 55);
			this.lblIndexer.Name = "lblIndexer";
			this.lblIndexer.Size = new System.Drawing.Size(39, 15);
			this.lblIndexer.TabIndex = 7;
			this.lblIndexer.Text = "Status";
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(511, 22);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(97, 23);
			this.button5.TabIndex = 5;
			this.button5.Text = "Save Words";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(410, 22);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(97, 23);
			this.button3.TabIndex = 4;
			this.button3.Text = "Free Memory";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(309, 22);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(97, 23);
			this.button4.TabIndex = 3;
			this.button4.Text = "Save Index";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(208, 22);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(97, 23);
			this.button2.TabIndex = 2;
			this.button2.Text = "Count Words";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// btnStop
			// 
			this.btnStop.Location = new System.Drawing.Point(107, 22);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(97, 23);
			this.btnStop.TabIndex = 1;
			this.btnStop.Text = "Stop";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(6, 22);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(97, 23);
			this.btnStart.TabIndex = 0;
			this.btnStart.Text = "Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.listBox1);
			this.groupBox4.Controls.Add(this.lblStatus);
			this.groupBox4.Controls.Add(this.txtSearch);
			this.groupBox4.Controls.Add(this.btnSearch);
			this.groupBox4.Location = new System.Drawing.Point(12, 261);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(622, 225);
			this.groupBox4.TabIndex = 0;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "4. Search";
			// 
			// listBox1
			// 
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 15;
			this.listBox1.Location = new System.Drawing.Point(6, 73);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(610, 139);
			this.listBox1.TabIndex = 3;
			this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.Location = new System.Drawing.Point(6, 55);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(66, 15);
			this.lblStatus.TabIndex = 2;
			this.lblStatus.Text = "Last Search";
			// 
			// txtSearch
			// 
			this.txtSearch.Location = new System.Drawing.Point(66, 23);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(550, 23);
			this.txtSearch.TabIndex = 1;
			this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
			// 
			// btnSearch
			// 
			this.btnSearch.Location = new System.Drawing.Point(6, 22);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(40, 23);
			this.btnSearch.TabIndex = 0;
			this.btnSearch.Text = "Go";
			this.btnSearch.UseVisualStyleBackColor = true;
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// cbUseStopList
			// 
			this.cbUseStopList.AutoSize = true;
			this.cbUseStopList.Checked = true;
			this.cbUseStopList.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbUseStopList.Location = new System.Drawing.Point(66, 55);
			this.cbUseStopList.Name = "cbUseStopList";
			this.cbUseStopList.Size = new System.Drawing.Size(125, 19);
			this.cbUseStopList.TabIndex = 2;
			this.cbUseStopList.Text = "Use Stop Word List";
			this.cbUseStopList.UseVisualStyleBackColor = true;
			// 
			// cbIgnoreNumeric
			// 
			this.cbIgnoreNumeric.AutoSize = true;
			this.cbIgnoreNumeric.Checked = true;
			this.cbIgnoreNumeric.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbIgnoreNumeric.Location = new System.Drawing.Point(222, 55);
			this.cbIgnoreNumeric.Name = "cbIgnoreNumeric";
			this.cbIgnoreNumeric.Size = new System.Drawing.Size(145, 19);
			this.cbIgnoreNumeric.TabIndex = 3;
			this.cbIgnoreNumeric.Text = "Ignore Numeric Works";
			this.cbIgnoreNumeric.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(657, 486);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "Form1";
			this.Text = "hOOt - Desktop Search";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox txtIndexFolder;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox txtWhere;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label lblIndexer;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.TextBox txtSearch;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.CheckBox cbIgnoreNumeric;
		private System.Windows.Forms.CheckBox cbUseStopList;
	}
}

