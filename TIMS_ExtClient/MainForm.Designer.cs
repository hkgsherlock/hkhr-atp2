/*
 * Created by SharpDevelop.
 * User: Charles Poon
 * Date: 13/01/14
 * Time: 05:39
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace TIMS_ExtClient
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.cmsMain = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.smiConnect = new System.Windows.Forms.ToolStripMenuItem();
			this.smiDisconnect = new System.Windows.Forms.ToolStripMenuItem();
			this.pbxSecondDecimal = new System.Windows.Forms.PictureBox();
			this.pbxSecondTen = new System.Windows.Forms.PictureBox();
			this.pbxMinuteDecimal = new System.Windows.Forms.PictureBox();
			this.pbxMinuteTen = new System.Windows.Forms.PictureBox();
			this.pbxHourDecimal = new System.Windows.Forms.PictureBox();
			this.pbxHourTen = new System.Windows.Forms.PictureBox();
			this.pbxDateDecimal = new System.Windows.Forms.PictureBox();
			this.pbxDateTen = new System.Windows.Forms.PictureBox();
			this.pbxMonthDecimal = new System.Windows.Forms.PictureBox();
			this.pbxMonthTen = new System.Windows.Forms.PictureBox();
			this.pbxYearDecimal = new System.Windows.Forms.PictureBox();
			this.pbxYearTen = new System.Windows.Forms.PictureBox();
			this.pbxYearHundred = new System.Windows.Forms.PictureBox();
			this.pbxYearThousand = new System.Windows.Forms.PictureBox();
			this.cmsMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbxSecondDecimal)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxSecondTen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxMinuteDecimal)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxMinuteTen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxHourDecimal)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxHourTen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxDateDecimal)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxDateTen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxMonthDecimal)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxMonthTen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxYearDecimal)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxYearTen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxYearHundred)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxYearThousand)).BeginInit();
			this.SuspendLayout();
			// 
			// cmsMain
			// 
			this.cmsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.smiConnect,
									this.smiDisconnect});
			this.cmsMain.Name = "cmsMain";
			this.cmsMain.Size = new System.Drawing.Size(143, 48);
			// 
			// smiConnect
			// 
			this.smiConnect.Name = "smiConnect";
			this.smiConnect.Size = new System.Drawing.Size(142, 22);
			this.smiConnect.Text = "Connect to...";
			this.smiConnect.Click += new System.EventHandler(this.SmiConnect_Click);
			// 
			// smiDisconnect
			// 
			this.smiDisconnect.Name = "smiDisconnect";
			this.smiDisconnect.Size = new System.Drawing.Size(142, 22);
			this.smiDisconnect.Text = "Disconnect";
			// 
			// pbxSecondDecimal
			// 
			this.pbxSecondDecimal.BackColor = System.Drawing.Color.Black;
			this.pbxSecondDecimal.Location = new System.Drawing.Point(686, 42);
			this.pbxSecondDecimal.Name = "pbxSecondDecimal";
			this.pbxSecondDecimal.Size = new System.Drawing.Size(24, 38);
			this.pbxSecondDecimal.TabIndex = 27;
			this.pbxSecondDecimal.TabStop = false;
			// 
			// pbxSecondTen
			// 
			this.pbxSecondTen.BackColor = System.Drawing.Color.Black;
			this.pbxSecondTen.Location = new System.Drawing.Point(656, 42);
			this.pbxSecondTen.Name = "pbxSecondTen";
			this.pbxSecondTen.Size = new System.Drawing.Size(24, 38);
			this.pbxSecondTen.TabIndex = 26;
			this.pbxSecondTen.TabStop = false;
			// 
			// pbxMinuteDecimal
			// 
			this.pbxMinuteDecimal.BackColor = System.Drawing.Color.Black;
			this.pbxMinuteDecimal.Location = new System.Drawing.Point(611, 42);
			this.pbxMinuteDecimal.Name = "pbxMinuteDecimal";
			this.pbxMinuteDecimal.Size = new System.Drawing.Size(24, 38);
			this.pbxMinuteDecimal.TabIndex = 25;
			this.pbxMinuteDecimal.TabStop = false;
			// 
			// pbxMinuteTen
			// 
			this.pbxMinuteTen.BackColor = System.Drawing.Color.Black;
			this.pbxMinuteTen.Location = new System.Drawing.Point(581, 42);
			this.pbxMinuteTen.Name = "pbxMinuteTen";
			this.pbxMinuteTen.Size = new System.Drawing.Size(24, 38);
			this.pbxMinuteTen.TabIndex = 24;
			this.pbxMinuteTen.TabStop = false;
			// 
			// pbxHourDecimal
			// 
			this.pbxHourDecimal.BackColor = System.Drawing.Color.Black;
			this.pbxHourDecimal.Location = new System.Drawing.Point(536, 42);
			this.pbxHourDecimal.Name = "pbxHourDecimal";
			this.pbxHourDecimal.Size = new System.Drawing.Size(24, 38);
			this.pbxHourDecimal.TabIndex = 23;
			this.pbxHourDecimal.TabStop = false;
			// 
			// pbxHourTen
			// 
			this.pbxHourTen.BackColor = System.Drawing.Color.Black;
			this.pbxHourTen.Location = new System.Drawing.Point(506, 42);
			this.pbxHourTen.Name = "pbxHourTen";
			this.pbxHourTen.Size = new System.Drawing.Size(24, 38);
			this.pbxHourTen.TabIndex = 22;
			this.pbxHourTen.TabStop = false;
			// 
			// pbxDateDecimal
			// 
			this.pbxDateDecimal.BackColor = System.Drawing.Color.Black;
			this.pbxDateDecimal.Location = new System.Drawing.Point(457, 58);
			this.pbxDateDecimal.Name = "pbxDateDecimal";
			this.pbxDateDecimal.Size = new System.Drawing.Size(13, 19);
			this.pbxDateDecimal.TabIndex = 21;
			this.pbxDateDecimal.TabStop = false;
			// 
			// pbxDateTen
			// 
			this.pbxDateTen.BackColor = System.Drawing.Color.Black;
			this.pbxDateTen.Location = new System.Drawing.Point(442, 58);
			this.pbxDateTen.Name = "pbxDateTen";
			this.pbxDateTen.Size = new System.Drawing.Size(13, 19);
			this.pbxDateTen.TabIndex = 20;
			this.pbxDateTen.TabStop = false;
			// 
			// pbxMonthDecimal
			// 
			this.pbxMonthDecimal.BackColor = System.Drawing.Color.Black;
			this.pbxMonthDecimal.Location = new System.Drawing.Point(402, 58);
			this.pbxMonthDecimal.Name = "pbxMonthDecimal";
			this.pbxMonthDecimal.Size = new System.Drawing.Size(13, 19);
			this.pbxMonthDecimal.TabIndex = 19;
			this.pbxMonthDecimal.TabStop = false;
			// 
			// pbxMonthTen
			// 
			this.pbxMonthTen.BackColor = System.Drawing.Color.Black;
			this.pbxMonthTen.Location = new System.Drawing.Point(387, 58);
			this.pbxMonthTen.Name = "pbxMonthTen";
			this.pbxMonthTen.Size = new System.Drawing.Size(13, 19);
			this.pbxMonthTen.TabIndex = 18;
			this.pbxMonthTen.TabStop = false;
			// 
			// pbxYearDecimal
			// 
			this.pbxYearDecimal.BackColor = System.Drawing.Color.Black;
			this.pbxYearDecimal.Location = new System.Drawing.Point(348, 58);
			this.pbxYearDecimal.Name = "pbxYearDecimal";
			this.pbxYearDecimal.Size = new System.Drawing.Size(13, 19);
			this.pbxYearDecimal.TabIndex = 17;
			this.pbxYearDecimal.TabStop = false;
			// 
			// pbxYearTen
			// 
			this.pbxYearTen.BackColor = System.Drawing.Color.Black;
			this.pbxYearTen.Location = new System.Drawing.Point(333, 58);
			this.pbxYearTen.Name = "pbxYearTen";
			this.pbxYearTen.Size = new System.Drawing.Size(13, 19);
			this.pbxYearTen.TabIndex = 16;
			this.pbxYearTen.TabStop = false;
			// 
			// pbxYearHundred
			// 
			this.pbxYearHundred.BackColor = System.Drawing.Color.Black;
			this.pbxYearHundred.Location = new System.Drawing.Point(318, 58);
			this.pbxYearHundred.Name = "pbxYearHundred";
			this.pbxYearHundred.Size = new System.Drawing.Size(13, 19);
			this.pbxYearHundred.TabIndex = 15;
			this.pbxYearHundred.TabStop = false;
			// 
			// pbxYearThousand
			// 
			this.pbxYearThousand.BackColor = System.Drawing.Color.Black;
			this.pbxYearThousand.Location = new System.Drawing.Point(303, 58);
			this.pbxYearThousand.Name = "pbxYearThousand";
			this.pbxYearThousand.Size = new System.Drawing.Size(13, 19);
			this.pbxYearThousand.TabIndex = 14;
			this.pbxYearThousand.TabStop = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(800, 600);
			this.ContextMenuStrip = this.cmsMain;
			this.Controls.Add(this.pbxSecondDecimal);
			this.Controls.Add(this.pbxSecondTen);
			this.Controls.Add(this.pbxMinuteDecimal);
			this.Controls.Add(this.pbxMinuteTen);
			this.Controls.Add(this.pbxHourDecimal);
			this.Controls.Add(this.pbxHourTen);
			this.Controls.Add(this.pbxDateDecimal);
			this.Controls.Add(this.pbxDateTen);
			this.Controls.Add(this.pbxMonthDecimal);
			this.Controls.Add(this.pbxMonthTen);
			this.Controls.Add(this.pbxYearDecimal);
			this.Controls.Add(this.pbxYearTen);
			this.Controls.Add(this.pbxYearHundred);
			this.Controls.Add(this.pbxYearThousand);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "TIMS_ExtClient";
			this.cmsMain.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbxSecondDecimal)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxSecondTen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxMinuteDecimal)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxMinuteTen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxHourDecimal)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxHourTen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxDateDecimal)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxDateTen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxMonthDecimal)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxMonthTen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxYearDecimal)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxYearTen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxYearHundred)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbxYearThousand)).EndInit();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.ToolStripMenuItem smiDisconnect;
		private System.Windows.Forms.ToolStripMenuItem smiConnect;
		private System.Windows.Forms.ContextMenuStrip cmsMain;
		private System.Windows.Forms.PictureBox pbxSecondTen;
		private System.Windows.Forms.PictureBox pbxSecondDecimal;
		private System.Windows.Forms.PictureBox pbxMinuteDecimal;
		private System.Windows.Forms.PictureBox pbxMinuteTen;
		private System.Windows.Forms.PictureBox pbxHourDecimal;
		private System.Windows.Forms.PictureBox pbxHourTen;
		private System.Windows.Forms.PictureBox pbxDateTen;
		private System.Windows.Forms.PictureBox pbxDateDecimal;
		private System.Windows.Forms.PictureBox pbxMonthTen;
		private System.Windows.Forms.PictureBox pbxMonthDecimal;
		private System.Windows.Forms.PictureBox pbxYearHundred;
		private System.Windows.Forms.PictureBox pbxYearTen;
		private System.Windows.Forms.PictureBox pbxYearDecimal;
		private System.Windows.Forms.PictureBox pbxYearThousand;
	}
}
