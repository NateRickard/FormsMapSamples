﻿using System;

using Xamarin.Forms;

namespace FormsMapSamples
{
	public class App : Application
	{
		public static double ScreenHeight;
		public static double ScreenWidth;

		public App ()
		{
			// The root page of your application
			//MainPage = new MapPage ();

			MainPage = new CustomMapPage ();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}