﻿using System;
using MapKit;

namespace FormsMapSamples.iOS
{
	public class CustomMKPinAnnotationView : MKAnnotationView
	{
		public string Id { get; set; }

		public string Url { get; set; }

		public CustomMKPinAnnotationView (IMKAnnotation annotation, string id)
			: base (annotation, id)
		{
		}
	}
}