using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xaml;

namespace BonsaiWorldSim
{
	public class HexMap
	{
		public HexMap(Canvas canvas) { Canvas = canvas; }

		Canvas Canvas { get; }

		public void DrawHexes(List<Tile> tiles)
		{
			if (Canvas == null)
			{
				throw new XamlException("Canvas not found");
			}

			Canvas.Children.Clear();

			foreach (var tile in tiles)
			{
				var scale = 50;
				var x     = tile.Position.X     * (Math.Sqrt(3f) / 2f) * scale;
				var y     = tile.Position.Y     * (3f            / 4f) * scale;
				var h     = 1                   * scale;
				var w     = (h * Math.Sqrt(3f)) / 2f;

				var horizontalLeft   = x;
				var horizontalRight  = x + w;
				var horizontalCenter = x + (w / 2f);

				var verticalTop    = y;
				var verticalBottom = y + h;
				var verticalLower  = y + (h * (3f / 4f));
				var verticalUpper  = y + (h * (1f / 4f));

				Canvas.Children.Add(
					new Polygon
					{
						Fill = Brushes.White,
						Points = new()
						{
							new(horizontalCenter, verticalTop),
							new(horizontalRight, verticalUpper),
							new(horizontalRight, verticalLower),
							new(horizontalCenter, verticalBottom),
							new(horizontalLeft, verticalLower),
							new(horizontalLeft, verticalUpper)
						}
					}
				);

				if(tile.Children == null)
				{
					return;
				}

				foreach (var child in tile.Children)
				{

					Canvas.Children.Add(
						new Line()
						{
							Stroke = Brushes.Red,
							X1     = x + (w            /2f),
							Y1     = y + (h            /2f),
							X2     = (child.Position.X * (Math.Sqrt(3f) / 2f) * scale) + (w / 2f),
							Y2     = (child.Position.Y * (3f            / 4f) * scale) + (h / 2f)
						}
					);
				}
			}
		}
	}
}