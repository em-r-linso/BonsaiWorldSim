﻿using System.Collections.Generic;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BonsaiWorldSim
{
	public class HexMap
	{
		const int   HEX_SIZE                = 20;
		const float HEX_WIDTH_RATIO         = 0.86602540378f; // sqrt(3) / 2
		const float HEX_HEIGHT              = HEX_SIZE;
		const float HEX_WIDTH               = HEX_SIZE * HEX_WIDTH_RATIO;
		const float HEX_UPPER_CORNER_HEIGHT = 0.25f;
		const float HEX_LOWER_CORNER_HEIGHT = 0.75f;
		const float MAX_ALTITUDE            = 10;

		public HexMap(Canvas canvas) => Canvas = canvas;

		public bool DoDrawConnections { get; set; }

		Canvas Canvas { get; }

		void AddHex(Vector2 position, Brush fill, Brush stroke)
		{
			var horizontalLeft   = position.X * HEX_WIDTH_RATIO * HEX_SIZE;
			var horizontalRight  = horizontalLeft + HEX_WIDTH;
			var horizontalCenter = horizontalLeft + (HEX_WIDTH / 2f);

			var verticalTop    = position.Y * HEX_LOWER_CORNER_HEIGHT * HEX_SIZE;
			var verticalBottom = verticalTop + HEX_HEIGHT;
			var verticalLower  = verticalTop + (HEX_HEIGHT * HEX_LOWER_CORNER_HEIGHT);
			var verticalUpper  = verticalTop + (HEX_HEIGHT * HEX_UPPER_CORNER_HEIGHT);

			Canvas.Children.Add(
				new Polygon
				{
					Fill   = fill   ?? Brushes.Transparent,
					Stroke = stroke ?? Brushes.Transparent,
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
		}

		public void DrawHexes(List<Tile> tiles)
		{
			// TODO: optimize by moving hexes instead of redrawing them
			Canvas.Children.Clear();

			AddHex(Vector2.Zero, null, Brushes.Fuchsia);

			foreach (var tile in tiles)
			{
				var value = (byte)((tile.Altitude / MAX_ALTITUDE) * 256);
				var color = new SolidColorBrush(Color.FromRgb(value, value, value));
				AddHex(tile.Position, color, null);
			}

			if (DoDrawConnections)
			{
				foreach (var tile in tiles)
				{
					foreach (var connection in tile.Connections)
					{
						Canvas.Children.Add(
							new Line
							{
								Stroke = Brushes.Red,
								X1     = (tile.Position.X * HEX_WIDTH_RATIO * HEX_SIZE) + (HEX_WIDTH / 2f),
								Y1     = (tile.Position.Y * HEX_LOWER_CORNER_HEIGHT * HEX_SIZE) + (HEX_HEIGHT / 2f),
								X2     = (connection.Position.X * HEX_WIDTH_RATIO * HEX_SIZE) + (HEX_WIDTH / 2f),
								Y2 = (connection.Position.Y * HEX_LOWER_CORNER_HEIGHT * HEX_SIZE)
								   + (HEX_HEIGHT            / 2f)
							}
						);
					}
				}
			}
		}
	}
}