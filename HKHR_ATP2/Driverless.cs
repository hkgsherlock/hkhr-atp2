using System;
using OpenBveApi.Runtime;

namespace HKHR_ATP2
{
	/// <summary>
	/// Description of Driverless.
	/// </summary>
	internal class Driverless
	{
		private int[] Panel;
		private StationsMemory.StationsCollection Stations = new StationsMemory.StationsCollection();
		private int lastCompletedDockingIndex = -1;
		private bool doorCls = false;
		
		internal void WriteRefreshedStationsCollection(StationsMemory.StationsCollection stations)
		{
			this.Stations = stations;
		}
		
		internal void WriteRefreshedLastDockedIndex(int index)
		{
			this.lastCompletedDockingIndex = index;
		}
		
		public void Load(LoadProperties properties)
		{
			Panel = properties.Panel;
		}
		
		internal void Elapse(ElapseData vState) {
			Panel[PanelID.StatusLEDs.NoSignal] = 0;
			if (Stations.Count > 0) {
				if (lastCompletedDockingIndex > 0 && Stations[lastCompletedDockingIndex].TBSStart && doorCls) {
					// can operate in driverless using ATO
					OnDriverlessAvailableElapse(vState);
				}
			}
		}
		
		public void DoorChange(DoorStates oldState, DoorStates newState) {
			doorCls = newState == DoorStates.None;
		}
		
		internal delegate void CallDriverlessAvailableEventHandlers(ElapseData data);
		internal event CallDriverlessAvailableEventHandlers OnDriverlessAvailableElapse;
	}
}
