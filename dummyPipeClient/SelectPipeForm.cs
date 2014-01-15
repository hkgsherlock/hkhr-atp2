/*
 * Created by SharpDevelop.
 * User: Charles Poon
 * Date: 14/01/14
 * Time: 23:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace dummyPipeClient
{
	/// <summary>
	/// Description of SelectPipe.
	/// </summary>
	public partial class SelectPipeForm : Form
	{
		public SelectPipeForm()
		{
			InitializeComponent();
		}
		
		public string selectedPipePath = "";
		
		private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			selectedPipePath = (string)listBox1.SelectedItem;
		}
		
		private void ListBox1_DoubleClick(object sender, EventArgs e)
		{
			selectedPipePath = (string)listBox1.SelectedItem;
			this.Close();
		}
		
		private void SelectPipe_Load(object sender, EventArgs e)
		{
			String[] listOfPipes = System.IO.Directory.GetFiles(@"\\.\pipe\");
			listBox1.Items.Clear();
			foreach (string pipe in listOfPipes) {
				if (pipe.StartsWith(@"\\.\pipe\HKHRATP2\")) 
				{
					listBox1.Items.Add(pipe);
				}
			}
			listBox1.Enabled = true;
			buttonOK.Enabled = true;
		}
	}
}
