using System.Windows;
using System.Windows.Input;

namespace BonsaiWorldSim
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		const int EXPANSIONS_PER_BUTTON_PRESS = 50;

		public MainWindow()
		{
			InitializeComponent();

			Simulation = new();

			HexMap = new(Canvas);
			HexMap.DrawHexes(Simulation.Tiles);
		}

		static int LastUsedId { get; set; }

		public static string NextId
		{
			get
			{
				LastUsedId++;
				return $"{LastUsedId:000000}";
			}
		}

		bool       IsDragged  { get; set; }
		Point      Last       { get; set; }
		Simulation Simulation { get; }
		HexMap     HexMap     { get; }

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			CaptureMouse();
			Last      = e.GetPosition(this);
			IsDragged = true;
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);

			ReleaseMouseCapture();
			IsDragged = false;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (IsDragged == false)
			{
				return;
			}

			if ((e.LeftButton == MouseButtonState.Pressed) && IsMouseCaptured)
			{
				var pos    = e.GetPosition(this);
				var matrix = Mt.Matrix;
				matrix.Translate(pos.X - Last.X, pos.Y - Last.Y);
				Mt.Matrix = matrix;
				Last      = pos;
			}
		}

		void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			for (var i = 0; i < EXPANSIONS_PER_BUTTON_PRESS; i++)
			{
				Simulation.Expand();
			}

			HexMap.DrawHexes(Simulation.Tiles);
		}

		public static void Popup(string message) { MessageBox.Show(message); }
	}
}