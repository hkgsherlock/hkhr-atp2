using System;
using OpenBveApi.Runtime;

namespace HKHR_ATP2
{
	/// <summary>
	/// Description of ATO.
	/// </summary>
	internal class ATO
	{
		internal int handlePower;
		internal int handleBrake;
		
		internal void Elapse(ATP2.ATPElapseData vState)
		{
			throw new NotImplementedException("ATO is not yet implemeted. \nPress \"Ignore\" button and switch to any other modes to drive the train please.");
		}
	}
}
