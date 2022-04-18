using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Media;

namespace BonsaiWorldSim
{
	public class Tile
	{
		public Tile(Vector2 position)
		{
			Position = position;

			Connections = new();

			Color = new[]
			{
				Brushes.White,
				Brushes.Brown,
				Brushes.Green,
				Brushes.Coral,
				Brushes.Blue
			}[Simulation.Random.Next(5)];

			Simulation.Tiles.Add(this);
		}

		public Vector2 Position             { get; set; }
		public Brush   Color                { get; set; }
		public bool    AttemptedTranslation { get; set; }
		public bool    IgnoreExclusionZone  { get; set; }

		public Dictionary<Vector2, Tile> Connections { get; }

		public List<Tile> Neighbors
		{
			get
			{
				return Simulation.DIRECTIONS.Select(direction => Simulation.GetTileAtPosition(Position + direction))
				                 .Where(tile => tile != null)
				                 .ToList();
			}
		}

		public void Translate(Vector2 translation)
		{
			// don't translate if the tile is already moved
			if (AttemptedTranslation)
			{
				return;
			}

			AttemptedTranslation = true;

			// if this would move the tile into the center hole, break all connections and don't translate
			if (!IgnoreExclusionZone && ((Position + translation).Length() < Simulation.CENTER_HOLE_RADIUS))
			{
				Connections.Clear();
				return;
			}

			// push another tile out of the way if necessary
			var tileInTheWay = Simulation.GetTileAtPosition(Position + translation);
			tileInTheWay?.Translate(translation);

			// move connected tiles
			foreach (var (_, value) in Connections)
			{
				value.Translate(translation);
			}

			// move the tile
			Position += translation;
		}
	}
}