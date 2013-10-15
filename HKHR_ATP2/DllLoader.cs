using System;
using System.Windows.Forms;

namespace HKHR_ATP2
{
	/// <summary>
	/// Description of DllLoader.
	/// </summary>
	public static class DllLoader
	{
		public static bool ConfirmUseDll()
		{
			switch (MessageBox.Show("Use DLL as train system?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1)) {
				case DialogResult.Yes:
					return true;
				default:
					return false;
			}
		}
	}
}
