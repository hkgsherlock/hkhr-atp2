using System;
using System.Collections.Generic;
using OpenBveApi.Runtime;

namespace HKHR_ATP2
{
	/// <summary>
	/// Description of StationsMemory.
	/// </summary>
	internal class StationsMemory
	{
		private int[] Panel;
		
		#region Station, StationsContainer classes definitions
		internal class Station
		{
			private int _doorOpen = 0;
			
			internal int DoorOpen
			{
				get { return _doorOpen; }
				private set {
					if (value >= -2 && value <= 2) {
						_doorOpen = value;
					}
				}
			}
			internal int StopPosition;
			internal bool TBSStart;
			
			public Station(int doorOpen, int stopPosition, bool tbsStart)
			{
				this.DoorOpen = doorOpen;
				this.StopPosition = stopPosition;
				this.TBSStart = tbsStart;
			}
		}
		
		internal class StationsCollection
		{
			private List<Station> _stns = new List<StationsMemory.Station>();
			internal Station this[int index]
			{
				get
				{
					return this._stns[index];
				}
				set
				{
					
					if (index >= this._stns.Count) {
						_stns.Add(value);
					} else if (value == null) {
						throw new NullReferenceException();
					} else {
						_stns[index] = value;
					}
				}
			}
			
			internal int Count
			{
				get { return this._stns.Count; }
			}
			
			internal void Add(Station item)
			{
				this._stns.Add(item);
			}
			
			internal void AddRange(Station[] items)
			{
				this._stns.AddRange(items);
			}
			
			internal int IndexOf(int stopPosition)
			{
				for (int i = 0; i < _stns.Count; i++) {
					if (stopPosition == _stns[i].StopPosition) {
						return i;
					}
				}
				return -1;
			}
			
//			private int SortStationItems(Station x, Station y)
//			{
//				if (x.StopPosition < y.StopPosition) {
//					return -1;
//				} else if (x.StopPosition > y.StopPosition) {
//					return 1;
//				} else {
//					return 0;
//				}
//			}
		}
		#endregion
		
		private StationsCollection Stations;
		
		private int _lastCompDkingIdx;
		private int lastCompletedDockingIndex
		{
			get { return _lastCompDkingIdx; }
			set {
				if (value >= -1 && value < Stations.Count) {
					_lastCompDkingIdx = value;
					OnLastDockIndexRefreshed(value);
				} else {
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		private ElapseData vState;
		
		/// <summary>
		/// Creates a new empty StationMemory instance on the train set. 
		/// </summary>
		internal StationsMemory()
		{
			Stations = new StationsCollection();
			_lastCompDkingIdx = -1;
		}
		
		public void Load(LoadProperties properties)
		{
			Panel = properties.Panel;
			Panel[PanelID.DistanceToNextStation.FirstDigit] = 12;
			Panel[PanelID.DistanceToNextStation.SecondDigit] = 12;
			Panel[PanelID.DistanceToNextStation.ThirdDigit] = 11;
			Panel[PanelID.DistanceToNextStation.ForthDigit] = 12;
			Panel[PanelID.DistanceToNextStation.Unit] = 0;
		}
		
		public void Elapse(ElapseData data) {
			vState = data;
			
			if (Stations.Count > 0) {
				if (lastCompletedDockingIndex + 1 < Stations.Count) { // if next stop is in range 
					
					if (Stations[lastCompletedDockingIndex + 1].DoorOpen == -2 
					    && vState.Vehicle.Location >= Stations[lastCompletedDockingIndex + 1].StopPosition - 0.3)
					{ // if next stop is passing stop
						lastCompletedDockingIndex++;
					} else if (vState.Vehicle.Speed.KilometersPerHour == 0				           
						    && Stations[lastCompletedDockingIndex + 1].DoorOpen == 0 
						    && vState.Vehicle.Location >= Stations[lastCompletedDockingIndex + 1].StopPosition - 10
						    && vState.Vehicle.Location <= Stations[lastCompletedDockingIndex + 1].StopPosition + 10)
					{ // if next stop is a stop which does not need to open doors
						lastCompletedDockingIndex++;
					}
					
					// passed a station/stop which should stop
					if (vState.Vehicle.Location > Stations[lastCompletedDockingIndex + 1].StopPosition + 45 ) {
	//					System.Windows.Forms.MessageBox.Show("passed a stop");
						lastCompletedDockingIndex++;
					}
				}
				
				// display distance to next stop
				double distance = lastCompletedDockingIndex < Stations.Count ? Stations[lastCompletedDockingIndex + 1].StopPosition - vState.Vehicle.Location : -999;
				if (distance == -999) {
					Panel[PanelID.DistanceToNextStation.FirstDigit] = 12;
					Panel[PanelID.DistanceToNextStation.SecondDigit] = 12;
					Panel[PanelID.DistanceToNextStation.ThirdDigit] = 11;
					Panel[PanelID.DistanceToNextStation.ForthDigit] = 12;
					Panel[PanelID.DistanceToNextStation.Unit] = 1;
				} else if (distance >= 1000) {
					if (distance >= 10000) {
						string kmStr = Math.Round(distance/1000).ToString().PadLeft(4);
						Panel[PanelID.DistanceToNextStation.FirstDigit] = kmStr.Substring(0,1) == " " ? 11 : Convert.ToInt32(kmStr.Substring(0,1));
						Panel[PanelID.DistanceToNextStation.SecondDigit] = kmStr.Substring(1,1) == " " ? 11 : Convert.ToInt32(kmStr.Substring(1,1));
						Panel[PanelID.DistanceToNextStation.ThirdDigit] = kmStr.Substring(2,1) == " " ? 11 : Convert.ToInt32(kmStr.Substring(2,1));
						Panel[PanelID.DistanceToNextStation.ForthDigit] = Convert.ToInt32(kmStr.Substring(3,1));
					} else {
						string kmStr = Math.Round(distance/1000, 1).ToString().PadLeft(4);
						Panel[PanelID.DistanceToNextStation.FirstDigit] = kmStr.Substring(0,1) == " " ? 11 : Convert.ToInt32(kmStr.Substring(0,1));
						Panel[PanelID.DistanceToNextStation.SecondDigit] = kmStr.Substring(1,1) == " " ? 11 : Convert.ToInt32(kmStr.Substring(1,1));
						Panel[PanelID.DistanceToNextStation.ThirdDigit] = kmStr.Substring(2,1) == " " ? 11 : kmStr.Substring(2,1) == "." ? 10 : Convert.ToInt32(kmStr.Substring(2,1));
						Panel[PanelID.DistanceToNextStation.ForthDigit] = Convert.ToInt32(kmStr.Substring(3,1));
					}
					
					Panel[PanelID.DistanceToNextStation.Unit] = 1;
				} else if (Math.Abs(distance) < 10) {
					string mStr = Math.Round(distance, 1).ToString("0.0").PadLeft(4);
					Panel[PanelID.DistanceToNextStation.FirstDigit] = mStr.Substring(0,1) == "-" ? 12 : 11;
					Panel[PanelID.DistanceToNextStation.SecondDigit] = mStr.Substring(1,1) == "-" ? 12 : mStr.Substring(1,1) == " " ? 11 : Convert.ToInt32(mStr.Substring(1,1));
					Panel[PanelID.DistanceToNextStation.ThirdDigit] = mStr.Substring(1,1) == "-" ? 12 : 10;
					Panel[PanelID.DistanceToNextStation.ForthDigit] = mStr.Substring(3,1) == " " ? 11 : Convert.ToInt32(mStr.Substring(3,1));
					
					Panel[PanelID.DistanceToNextStation.Unit] = 0;
				} else {
					string mStr = Math.Round(distance).ToString().PadLeft(4);
					Panel[PanelID.DistanceToNextStation.FirstDigit] = 11;
					Panel[PanelID.DistanceToNextStation.SecondDigit] = mStr.Substring(1,1) == "-" ? 12 : mStr.Substring(1,1) == " " ? 11 : Convert.ToInt32(mStr.Substring(1,1));
					Panel[PanelID.DistanceToNextStation.ThirdDigit] = Convert.ToInt32(mStr.Substring(2,1));
					Panel[PanelID.DistanceToNextStation.ForthDigit] = Convert.ToInt32(mStr.Substring(3,1));
					
					Panel[PanelID.DistanceToNextStation.Unit] = 0;
				}
			}
		}
		
		public void DoorChange(DoorStates oldState, DoorStates newState) {
			if (Stations.Count > 0) {
				if (/*oldState != DoorStates.None && */newState == DoorStates.None 
				    && vState.Vehicle.Speed.KilometersPerHour == 0 
				    && (Stations[lastCompletedDockingIndex + 1].DoorOpen == -1 || Stations[lastCompletedDockingIndex + 1].DoorOpen >= 1)
				    && vState.Vehicle.Location >= Stations[lastCompletedDockingIndex + 1].StopPosition - 0.3
				    && vState.Vehicle.Location <= Stations[lastCompletedDockingIndex + 1].StopPosition + 0.3
				    && lastCompletedDockingIndex < Stations.Count)
				{
					lastCompletedDockingIndex++;
				}
			}
		}
		
		internal void SetBeacon(BeaconData beacon)
		{
			if (beacon.Type == BeaconID.StationsMemory.StationStopProvider)
			{
				int param_doorside = beacon.Optional / 10000000;
				bool param_tbsStart = Math.Abs(beacon.Optional / 1000000 - param_doorside * 10) == 1 ? true : false;
				int param_trackPos = Math.Abs(beacon.Optional - param_doorside * 10000000 - (param_tbsStart ? 1000000 : 0));
				
				int idxOfStopAtHere = Stations.IndexOf(param_trackPos);
				
				if (idxOfStopAtHere > 0) {
					// stop exists
					Stations[idxOfStopAtHere] = new Station(param_doorside, param_trackPos, param_tbsStart);
				} else {
					// stop not exist, add one then
					Stations.Add(new Station(param_doorside, param_trackPos, param_tbsStart));
				}
				
				OnStationDataRefreshed(Stations);
			}
		}
		
		internal delegate void NewStationDataReceiver(StationsCollection stations);
		internal event NewStationDataReceiver OnStationDataRefreshed;
		
		internal delegate void LastDockIndexReceiver(int index);
		internal event LastDockIndexReceiver OnLastDockIndexRefreshed;
	}
}
