using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Input.Manipulations;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Effects;

namespace TouchOne
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		LoggerWindow _LoggerWindow;

		public Window1()
		{
			InitializeComponent();

			_LoggerWindow = new LoggerWindow();
			_LoggerWindow.Show();
			FullScreen();

			AddNewShape();
		}

		void FullScreen()
		{
			this.WindowStyle = System.Windows.WindowStyle.None;
			this.WindowState = System.Windows.WindowState.Maximized;
			this.ResizeMode = System.Windows.ResizeMode.NoResize;
		}


		#region Event handlers
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_LoggerWindow.IsClosingProcess = true;
			_LoggerWindow.Close();
		}

		private void ToggleImages(object sender, RoutedEventArgs e)
		{
			ToggleButton toggle = sender as ToggleButton;
			if(toggle.IsChecked.Value)
			{
				RandomImage ri = new RandomImage();
				foreach(Shape c in TouchPanel.Children)
				{
					c.Fill = new ImageBrush
					{
						ImageSource = ri.Next(),
					};
				}
			}
			else
			{
				RandomSolidColorBrush rscb = new RandomSolidColorBrush();
				foreach(Shape c in TouchPanel.Children)
				{
					c.Fill = rscb.Next();
				}
			}
		}

		private void NewShape(object sender, RoutedEventArgs e)
		{
			AddNewShape();
		}
		#endregion

		#region Finestra di logging
		private StringBuilder LogText
		{
			get { return _LoggerWindow.LogText; }
		}

		void Log()
		{
			_LoggerWindow.ShowLog();
		}

		private void LogWindow_Click(object sender, RoutedEventArgs e)
		{
			_LoggerWindow.Show();
		}		
		#endregion

		private void AddNewShape()
		{
			Rectangle rc = new Rectangle();
			rc.IsManipulationEnabled = true;
			rc.Width = 300;
			rc.Height = rc.Width / 1.333;	// aspect ratio fisso

			double ContainerWidth = TouchPanel.ActualWidth;
			double ContainerHeight = TouchPanel.ActualHeight;
			if(ContainerWidth == 0 || ContainerWidth == double.NaN)
			{
				ContainerWidth = SystemParameters.PrimaryScreenWidth;
				ContainerHeight = SystemParameters.PrimaryScreenHeight;
			}
			Canvas.SetLeft(rc, ContainerWidth / 2 - rc.Width / 2);
			Canvas.SetTop(rc, ContainerHeight / 2 - rc.Height / 2);

			DropShadowEffect dse = new DropShadowEffect();
			dse.Color = Colors.Gray;
			dse.Opacity = 0.75;
			dse.ShadowDepth = 8;
			rc.Effect = dse;

			if(FillToggleButton.IsChecked.Value)
			{
				RandomImage ri = new RandomImage();
				rc.Fill = new ImageBrush(ri.Next());
			}
			else
			{
				RandomSolidColorBrush rscb = new RandomSolidColorBrush();
				rc.Fill = rscb.Next();
			}


			TouchPanel.Children.Add(rc);
			ZTop = Canvas.GetZIndex(rc);
		}

		private void ResetShapes_Click(object sender, RoutedEventArgs e)
		{
			foreach(UIElement v in TouchPanel.Children)
			{
				v.RenderTransform = new MatrixTransform(Matrix.Identity);
			}
		}

		#region Dependency properties
		public double InertiaFactor
		{
			get { return (double)GetValue(InertiaFactorProperty); }
			set { SetValue(InertiaFactorProperty, value); }
		}
		public static readonly DependencyProperty InertiaFactorProperty =
			DependencyProperty.Register("InertiaFactor", typeof(double), typeof(Window1), new UIPropertyMetadata(1000.0));
		#endregion

		#region Eventi di Manipulation
		int ZTop = 0;
		private void Window_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
		{
			Shape shape = e.OriginalSource as Shape;
			int Z = Canvas.GetZIndex(shape);
			ZTop = Math.Max(Z, ZTop) + 2;
			Canvas.SetZIndex(shape, ZTop);

			e.Mode = ManipulationModes.All;
			e.ManipulationContainer = TouchPanel;
			e.Handled = true;
			LogText.AppendLine("Window_ManipulationStarting");
		}

		private void Window_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
		{
			ManipulationDelta manipulation = e.DeltaManipulation;
			Shape shape = e.OriginalSource as Shape;
			Matrix matrix = (shape.RenderTransform as MatrixTransform).Matrix;
			var center = new Point(shape.ActualWidth / 2, shape.ActualHeight / 2);

			// bring to front
			//TouchPanel.Children.Remove(shape);
			//TouchPanel.Children.Add(shape);

			// 1. Traslazione
			matrix.Translate(manipulation.Translation.X, manipulation.Translation.Y);
			Point newCenter = matrix.Transform(center);

			// 2. Rotazione
			matrix.RotateAt(manipulation.Rotation, newCenter.X, newCenter.Y);
			//newCenter = matrix.Transform(center);	//x

			// 3. Scale
			matrix.ScaleAt(manipulation.Scale.X, manipulation.Scale.Y, newCenter.X, newCenter.Y);
			//newCenter = matrix.Transform(center);

			shape.RenderTransform = new MatrixTransform(matrix);

			Rect containingRect = new Rect(((FrameworkElement)e.ManipulationContainer).RenderSize);
			double OriginX = Canvas.GetLeft(shape);
			double OriginY = Canvas.GetTop(shape);
			Rect shapeBounds = shape.RenderTransform.TransformBounds(
				new Rect(OriginX, OriginY, shape.RenderSize.Width, shape.RenderSize.Height));

			// Check if the rectangle is completely in the window.
			// If it is not and intertia is occuring, stop the manipulation.
			if(e.IsInertial && !containingRect.Contains(shapeBounds))
			{
	//			e.ReportBoundaryFeedback(e.DeltaManipulation);
				e.Complete();
			}
			e.Handled = true;

			// adesso Logging
			//LogText.AppendLine("Window_ManipulationDelta");
			if(manipulation.Rotation >0)
				LogText.AppendLine("Rotate by " + manipulation.Rotation);

            //ManipulationVelocities v = e.Velocities;
            //LogText.AppendLine(string.Format("Delta: Scale={0}, Rot={1}, Transla={2}, Velocity Expans={3}, Angular={4}, Linear={5}, IsInertial={6}",
            //    manipulation.Scale.F(), manipulation.Rotation.F(), manipulation.Translation.F(),
            //    v.ExpansionVelocity.F(), v.AngularVelocity.F(), v.LinearVelocity.F(),
            //    e.IsInertial));
			//Trace.WriteLine("Delta");
		}

		//private void Window_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
		//{
		//    //Manipulation manipulation = e.GetBoundaryFeedback(this);
		//    //LogText.AppendLine(string.Format("BoundaryFeedback: Scale={0}, Rot={1}, Transla={2}",
		//    //    manipulation.Scale.F(), manipulation.Rotation.F(), manipulation.Translation.F()));
		//    ////Trace.WriteLine("BoundaryFeedback");

		//    ManipulationDelta manipulation = e.BoundaryFeedback;
		//    LogText.AppendLine(string.Format("BoundaryFeedback: Scale={0}, Rot={1}, Transla={2}",
		//        manipulation.Scale.F(), manipulation.Rotation.F(), manipulation.Translation.F()));
		//    //Trace.WriteLine("BoundaryFeedback");
		//    e.Handled = true;

		//}

		private void Window_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
		{
			LogText.AppendLine("Window_ManipulationCompleted");
			Log();
		}



		private void Window_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
		{
			// Decrease the velocity of the Rectangle's movement by 
			// 10 inches per second every second.
			// (10 inches * 96 pixels per inch / 1000ms^2)
			e.TranslationBehavior = new InertiaTranslationBehavior()
			{
				InitialVelocity = e.InitialVelocities.LinearVelocity,
				DesiredDeceleration = 10.0 * 96.0 / Acceleration
			};

			// Decrease the velocity of the Rectangle's resizing by 
			// 0.1 inches per second every second.
			// (0.1 inches * 96 pixels per inch / (1000ms^2)
			e.ExpansionBehavior = new InertiaExpansionBehavior()
			{
				InitialVelocity = e.InitialVelocities.ExpansionVelocity,
				DesiredDeceleration = 0.1 * 96 / Acceleration
			};

			// Decrease the velocity of the Rectangle's rotation rate by 
			// 2 rotations per second every second.
			// (2 * 360 degrees / (1000ms^2)
			e.RotationBehavior = new InertiaRotationBehavior()
			{
				InitialVelocity = e.InitialVelocities.AngularVelocity,
				DesiredDeceleration = 720 / Acceleration
			};

			e.Handled = true;

			LogText.AppendLine("Window_ManipulationInertiaStarting");

			////LogText.AppendLine(string.Format("InertiaStarting: Origin={0}, Velocity Expans={1}, Angular={2}, Linear={3}",
			////    p.F(),
			////    v.ExpansionVelocity.F(), v.AngularVelocity.F(), v.LinearVelocity.F()));
			//Trace.WriteLine("InertiaStarting");
		}

		private double Acceleration
		{
			get { return this.InertiaFactor * this.InertiaFactor; }
		}

		#endregion




	}




}
