using System;
using OpenBveApi.Runtime;
using System.Collections.Generic;

namespace HKHR_ATP2
{
	internal class TrackGradientMemory
	{
		internal class GradientPoint
		{
			internal double Pitch = 0;
			internal int TrackPosition = 0;
			public GradientPoint(double pitch, int trackPosition)
			{
				this.Pitch = pitch;
				this.TrackPosition = trackPosition;
			}
		}
		
		internal class GradientPointCollection
		{
			private List<GradientPoint> _gPts = new List<GradientPoint>();
			internal GradientPoint this[int index]
			{
				get
				{
					return this._gPts[index];
				}
				set
				{
					
					if (index >= this._gPts.Count) {
						_gPts.Add(value);
					} else if (value == null) {
						throw new NullReferenceException();
					} else {
						_gPts[index] = value;
					}
				}
			}
			
			internal int Count
			{
				get { return this._gPts.Count; }
			}
			
			internal void Add(GradientPoint item)
			{
				this._gPts.Add(item);
			}
			
			internal void AddRange(GradientPoint[] items)
			{
				this._gPts.AddRange(items);
			}
			
			internal int IndexOf(double position)
			{
				for (int i = 0; i < _gPts.Count; i++) {
					if (position == _gPts[i].TrackPosition) {
						return i;
					}
				}
				return -1;
			}
			
			internal int NextIndex(double position)
			{
				for (int i = 0; i < _gPts.Count; i++)
				{
					if (_gPts[i].TrackPosition > position) {
						return i;
					}
				}
				return -1;
			}
			
			internal int CurrentIndex(double position)
			{
				for (int i = _gPts.Count - 1; i >= 0; i--)
				{
					if (_gPts[i].TrackPosition <= position) {
						return i;
					}
				}
				return -1;
			}
		}
		
		private GradientPointCollection _gPtC;
		
		// =================
		
		internal void Load(LoadProperties properties) {
			_gPtC = new GradientPointCollection();
			_gPtC.Add(new GradientPoint(0, 0));
		}
		
		internal void SetBeacon(BeaconData beacon) {
			if (beacon.Type == BeaconID.TrackGradientMemory.TrackGradientPoint)
			{
				int param_trackPos = Math.Abs(beacon.Optional % 1000000);
				int param_gradient = beacon.Optional / 1000000;
				
				int idxOfGradientPtAtHere = _gPtC.IndexOf(param_trackPos);
				
				if (idxOfGradientPtAtHere > -1) {
					// stop exists
					_gPtC[idxOfGradientPtAtHere] = new GradientPoint(param_gradient, param_trackPos);
				} else {
					// stop not exist, add one then
					_gPtC.Add(new GradientPoint(param_gradient, param_trackPos));
				}
				
				OnGradientPointsDataRefreshed(_gPtC);
			}
		}
		
		internal delegate void NewGradientPointsDataReceiver(GradientPointCollection gPtC);
		internal event NewGradientPointsDataReceiver OnGradientPointsDataRefreshed;
	}
}
