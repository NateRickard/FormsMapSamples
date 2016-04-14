using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.Maps;

namespace FormsMapSamples
{
	public class CustomMap : Map
	{
		public List<CustomPin> CustomPins { get; set; }

		public List<Position> ShapeCoordinates { get; set; }

		public event EventHandler RegionSelected;

		public CustomMap ()
		{
			CustomPins = new List<CustomPin> ();
			ShapeCoordinates = new List<Position> ();
		}

		public void OnPressed (bool regionSelected = false)
		{
			ShapeCoordinates.Clear ();

			if (regionSelected) {
				ShapeCoordinates.Add (new Position (37.797513, -122.402058));
				ShapeCoordinates.Add (new Position (37.798433, -122.402256));
				ShapeCoordinates.Add (new Position (37.798582, -122.401071));
				ShapeCoordinates.Add (new Position (37.797658, -122.400888));
			}

			RegionSelected?.Invoke (this, EventArgs.Empty);
		}
	}
}