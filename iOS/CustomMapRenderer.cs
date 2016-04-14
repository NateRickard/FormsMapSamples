using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CoreGraphics;
using CoreLocation;
using CustomRenderer.iOS;
using FormsMapSamples;
using FormsMapSamples.iOS;
using MapKit;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer (typeof (CustomMap), typeof (CustomMapRenderer))]
namespace CustomRenderer.iOS
{
	public class CustomMapRenderer : MapRenderer
	{
		UIView customPinView;
		List<CustomPin> customPins;
		MKPolygonRenderer polygonRenderer;
		IMKOverlay blockOverlay;

		protected override void OnElementChanged (ElementChangedEventArgs<View> e)
		{
			base.OnElementChanged (e);

			if (e.OldElement != null) {
				var formsMap = (CustomMap)e.OldElement;
				var nativeMap = Control as MKMapView;
				nativeMap.GetViewForAnnotation = null;

				nativeMap.CalloutAccessoryControlTapped -= OnCalloutAccessoryControlTapped;
				nativeMap.DidSelectAnnotationView -= OnDidSelectAnnotationView;
				nativeMap.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;

				var recognizer = nativeMap.GestureRecognizers.FirstOrDefault (g => g is UILongPressGestureRecognizer);

				if (recognizer != null) {
					nativeMap.RemoveGestureRecognizer (recognizer);
				}

				nativeMap.OverlayRenderer = null;

				formsMap.RegionSelected -= regionSelected;
			}

			if (e.NewElement != null) {
				var formsMap = (CustomMap)e.NewElement;
				var nativeMap = Control as MKMapView;
				customPins = formsMap.CustomPins;

				nativeMap.GetViewForAnnotation = GetViewForAnnotation;

				nativeMap.CalloutAccessoryControlTapped += OnCalloutAccessoryControlTapped;
				nativeMap.DidSelectAnnotationView += OnDidSelectAnnotationView;
				nativeMap.DidDeselectAnnotationView += OnDidDeselectAnnotationView;

				nativeMap.AddGestureRecognizer (new UILongPressGestureRecognizer (pressed));

				nativeMap.OverlayRenderer = GetOverlayRenderer;

				CLLocationCoordinate2D [] coords = new CLLocationCoordinate2D [formsMap.ShapeCoordinates.Count];

				int index = 0;
				foreach (var position in formsMap.ShapeCoordinates) {
					coords [index] = new CLLocationCoordinate2D (position.Latitude, position.Longitude);
					index++;
				}

				var blockOverlay = MKPolygon.FromCoordinates (coords);
				nativeMap.AddOverlay (blockOverlay);

				formsMap.RegionSelected += regionSelected;
			}
		}

		void pressed (UILongPressGestureRecognizer gestureRecognizer)
		{
			var formsMap = (CustomMap)Element;

			if (gestureRecognizer.State == UIGestureRecognizerState.Ended) {
				formsMap.OnPressed (true);
			} else if (gestureRecognizer.State == UIGestureRecognizerState.Began) {
				formsMap.OnPressed ();
			}
		}

		MKOverlayRenderer GetOverlayRenderer (MKMapView mapView, IMKOverlay overlay)
		{
			if (polygonRenderer == null) {
				polygonRenderer = new MKPolygonRenderer (overlay as MKPolygon);
				polygonRenderer.FillColor = UIColor.Red;
				polygonRenderer.StrokeColor = UIColor.Blue;
				polygonRenderer.Alpha = 0.4f;
				polygonRenderer.LineWidth = 9;
			}

			return polygonRenderer;
		}

		void regionSelected (object sender, EventArgs e)
		{
			var formsMap = (CustomMap)Element;
			var nativeMap = Control as MKMapView;

			if (blockOverlay != null && formsMap.ShapeCoordinates.Count == 0) {
				nativeMap.RemoveOverlay (blockOverlay);
			} else {
				CLLocationCoordinate2D [] coords = new CLLocationCoordinate2D [formsMap.ShapeCoordinates.Count];

				int index = 0;
				foreach (var position in formsMap.ShapeCoordinates) {
					coords [index] = new CLLocationCoordinate2D (position.Latitude, position.Longitude);
					index++;
				}

				blockOverlay = MKPolygon.FromCoordinates (coords);
				nativeMap.AddOverlay (blockOverlay);
			}
		}

		MKAnnotationView GetViewForAnnotation (MKMapView mapView, IMKAnnotation annotation)
		{
			MKAnnotationView annotationView = null;

			if (annotation is MKUserLocation)
				return null;

			var anno = annotation as MKPointAnnotation;
			var customPin = GetCustomPin (anno);
			if (customPin == null) {
				throw new Exception ("Custom pin not found");
			}

			annotationView = mapView.DequeueReusableAnnotation (customPin.Id);
			if (annotationView == null) {
				annotationView = new CustomMKPinAnnotationView (annotation, customPin.Id);
				annotationView.Image = UIImage.FromFile ("pin.png");
				annotationView.CalloutOffset = new CGPoint (0, 0);
				annotationView.LeftCalloutAccessoryView = new UIImageView (UIImage.FromFile ("monkey.png"));
				annotationView.RightCalloutAccessoryView = UIButton.FromType (UIButtonType.DetailDisclosure);
				((CustomMKPinAnnotationView)annotationView).Id = customPin.Id;
				((CustomMKPinAnnotationView)annotationView).Url = customPin.Url;
			}
			annotationView.CanShowCallout = true;

			return annotationView;
		}

		void OnCalloutAccessoryControlTapped (object sender, MKMapViewAccessoryTappedEventArgs e)
		{
			var customView = e.View as CustomMKPinAnnotationView;
			if (!string.IsNullOrWhiteSpace (customView.Url)) {
				UIApplication.SharedApplication.OpenUrl (new Foundation.NSUrl (customView.Url));
			}
		}

		void OnDidSelectAnnotationView (object sender, MKAnnotationViewEventArgs e)
		{
			var customView = e.View as CustomMKPinAnnotationView;
			customPinView = new UIView ();

			if (customView.Id == "Xamarin") {
				customPinView.Frame = new CGRect (0, 0, 200, 84);
				var image = new UIImageView (new CGRect (0, 0, 200, 84));
				image.Image = UIImage.FromFile ("xamarin.png");
				customPinView.AddSubview (image);
				customPinView.Center = new CGPoint (0, -(e.View.Frame.Height + 75));
				e.View.AddSubview (customPinView);
			}
		}

		void OnDidDeselectAnnotationView (object sender, MKAnnotationViewEventArgs e)
		{
			if (!e.View.Selected) {
				customPinView.RemoveFromSuperview ();
				customPinView.Dispose ();
				customPinView = null;
			}
		}

		CustomPin GetCustomPin (MKPointAnnotation annotation)
		{
			var position = new Position (annotation.Coordinate.Latitude, annotation.Coordinate.Longitude);
			foreach (var pin in customPins) {
				if (pin.Pin.Position == position) {
					return pin;
				}
			}
			return null;
		}
	}
}