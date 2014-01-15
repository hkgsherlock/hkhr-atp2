using Newtonsoft.Json;
//using OpenBveApi.Runtime;
using System;
using System.IO.Pipes;
using System.Resources;
using System.Windows.Forms;

namespace TIMS_ExtClient
{
	public partial class MainForm : Form
	{
		NamedPipeClientStream pipeClient = null;
		byte[] readBuffer = new byte[4096];
		string readStringBuffer = "";
		
		ResourceManager resources;
		
		private enum PageSections { SysHome };
		private PageSections workingPageSection = PageSections.SysHome; 
		
		public MainForm()
		{
			InitializeComponent();
		}
		
		private void SelectPipe()
		{
			SelectPipeForm selectPipe = new SelectPipeForm();
			if (selectPipe.ShowDialog(this) == DialogResult.OK) {
				string pipeName = selectPipe.selectedPipePath.Substring(9); // @"\\.\pipe\".length = 9
				pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
				try {
					pipeClient.Connect(1000);
					pipeClient.ReadMode = PipeTransmissionMode.Message;
					BeginRead();
				} catch (TimeoutException) {
					MessageBox.Show("Connection timeout when connecting to: " + selectPipe.selectedPipePath);
				}
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
				// trim message starting from \0 to prevent from reading excessive data causing exception being thrown out.
				readStringBuffer += System.Text.Encoding.UTF8.GetString(readBuffer);
				readStringBuffer += readStringBuffer.Substring(0, readStringBuffer.IndexOf('\0'));
				if (pipeClient.IsMessageComplete) {
					ParseReceiveMessage(readStringBuffer);
					readStringBuffer = "";
				}
			}
			
			BeginRead();
		}
		
		private void ParseReceiveMessage(string message)
		{
			message = message.Substring(0, message.IndexOf('\0'));
			
			string typeOfReceivedObjectAsString = message.Substring(0, message.IndexOf("|"));
			string ReceivedObjectAsString = message.Substring(message.IndexOf("|") + 1);
			switch (typeOfReceivedObjectAsString) {
				case "OpenBveApi.Runtime.ElapseData":
					System.Diagnostics.Debug.WriteLine(ReceivedObjectAsString + System.Environment.NewLine);
//					ElapseData deserialisedResult = (ElapseData)JsonConvert.DeserializeObjectAsync(ReceivedObjectAsString, 
//					                                                                               typeof(ElapseData), 
//					                                                                               new JsonSerializerSettings() 
//					                                                                               {
//					                                                                               		ObjectCreationHandling = ObjectCreationHandling.Replace, 
//					                                                                               		TypeNameHandling = TypeNameHandling.All
//					                                                                               } ).Result;
					ElapseData deserialisedResult = (ElapseData)JsonConvert.DeserializeObjectAsync(ReceivedObjectAsString, typeof(ElapseData), null).Result;
					System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(deserialisedResult) + System.Environment.NewLine);
					ElapseDataProcessor(deserialisedResult);
					break;
				default:
					
					break;
			}
			
			#if DEBUG
			this.BackgroundImage = (System.Drawing.Image)(resources.GetObject("TIMS.SysHome.Base"));
			#endif
		}
		
		private void ElapseDataProcessor(ElapseData input)
		{
			resources = new ResourceManager("TIMS_ExtClient.TIMS_Images", this.GetType().Assembly);
			
			#region Date & Time
			// Year
			pbxYearThousand.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.date_2"));
			pbxYearHundred.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.date_0"));
			pbxYearTen.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.date_0"));
			pbxYearDecimal.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.date_5"));
			
			// Month
			pbxMonthTen.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.date_0"));
			pbxMonthDecimal.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.date_2"));
			
			// Date
			pbxDateTen.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.date_1"));
			pbxDateDecimal.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.date_4"));
			
			// Time Extraction -- copied from openBVE source code
			int hour = (int)input.TotalTime.Seconds;
			int second = hour % 60; hour /= 60;
			int minute = hour % 60; hour /= 60;
			hour %= 24;
			
			System.Diagnostics.Debug.WriteLine(hour + ":" + minute + ":" + second + Environment.NewLine);
			
			// Hour
			pbxHourTen.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.time_" + (int)(hour / 10)));
			pbxHourDecimal.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.time_" + (int)(hour % 10)));
			
			// Minute
			pbxMinuteTen.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.time_" + (int)(minute / 10)));
			pbxMinuteDecimal.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.time_" + (int)(minute % 10)));
			
			// Second
			pbxSecondTen.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.time_" + (int)(second / 10)));
			pbxSecondDecimal.Image = (System.Drawing.Image)(resources.GetObject("TIMS.Header.time_" + (int)(second % 10)));
			#endregion
		}
		
		private void SmiConnect_Click(object sender, EventArgs e)
		{
			SelectPipe();
		}
	}
}
