using System;
using System.IO.Pipes;
using System.Text;
using System.Windows.Forms;

namespace dummyPipeClient
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		NamedPipeClientStream pipeClient = null;
		
		byte[] readBuffer = new byte[4096];
		string readStringBuffer = "";
		
		public MainForm()
		{
			InitializeComponent();
		}
		
		private void BtnConnect_Click(object sender, EventArgs e)
		{
			SelectPipeForm selectPipeForm = new SelectPipeForm();
			if (selectPipeForm.ShowDialog(this) == DialogResult.OK) {
				string pipeName = selectPipeForm.selectedPipePath.Substring(9); // @"\\.\pipe\".length = 9
				pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
				try {
					pipeClient.Connect(1000);
					BeginRead();
				} catch (TimeoutException) {
					MessageBox.Show("Connection timeout when connecting to: " + selectPipeForm.selectedPipePath);
				}
			}
		}
		
		private void BtnDisconnect_Click(object sender, EventArgs e)
		{
			if (pipeClient != null) {
				if (pipeClient.IsConnected)
					pipeClient.WaitForPipeDrain(); // :o)?
				pipeClient.Close();
			}
		}
		
		private void BeginRead()
		{
			pipeClient.BeginRead(readBuffer, 0, readBuffer.Length, BeginReadCallback, pipeClient);
		}
		
		private void BeginReadCallback(IAsyncResult iar) {
			pipeClient = (NamedPipeClientStream)iar.AsyncState;
			
			int bytesRead = pipeClient.EndRead(iar);
			
			if (bytesRead > 0) {
				readStringBuffer += Encoding.UTF8.GetString(readBuffer);
				
//				if (pipeClient.IsMessageComplete) {
					textBox1.Text += readStringBuffer + Environment.NewLine;
//					readStringBuffer = "";
//				}
			}
			
			BeginRead();
		}
		
		private void ButtonClear_Click(object sender, EventArgs e)
		{
			textBox1.Text = "";
		}
	}
}
