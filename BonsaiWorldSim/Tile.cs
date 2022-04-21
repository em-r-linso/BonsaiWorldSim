using System.Collections.Generic;
using System.IO.Packaging;
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

			Id = MainWindow.NextId;

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

		public override string ToString() => $"t{Id}";

		string Id { get; set; }

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

		public void Connect(Vector2 direction)
		{
			// give up if there's already a connection in that direction
			if (Connections.ContainsKey(direction))
			{
				return;
			}

			// give up if there's no tile in that direction
			var connection = Simulation.GetTileAtPosition(Position + direction);
			if (connection == null)
			{
				return;
			}

			// connect the tiles
			Connections.Add(direction, connection);
			connection.Connections.Add(direction * -1, this);
		}

		public void Disconnect(Vector2 direction)
		{
			// give up if there's no connection in that direction
			if (!Connections.ContainsKey(direction))
			{
				return;
			}

			// TODO: fix connections sometimes not being mutual
			var connection = Connections[direction];

			if (!connection.Connections.ContainsValue(this))
			{
				throw new("Connection not mutual!");
			}

			// disconnect the tiles
			Connections.Remove(direction);
			connection.Connections.Remove(direction * -1);
		}

		// EXPERIMENTAL DEGREE TRANSLATION
		public void Translate(int degrees)
		{
			// don't translate if the tile is already moved
			if (AttemptedTranslation)
			{
				return;
			}

			AttemptedTranslation = true;

			// translation that all connected tiles will be affected by
			// (pushed tiles will use degrees to generate a random translation)
			var translation = Simulation.DegreesToRandomHexDirection(degrees);

			// move connected tiles
			foreach (var (_, value) in Connections)
			{
				value.Translate(translation);
			}

			// push another tile out of the way if necessary
			// (ignore connected tiles)
			var tileInTheWay = Simulation.GetTileAtPosition(Position + translation);
			if ((tileInTheWay != null) && !Connections.ContainsValue(tileInTheWay))
			{
				tileInTheWay.Translate(degrees);
			}

			// if this would move the tile into the center hole, break all connections and don't translate
			if (!IgnoreExclusionZone && ((Position + translation).Length() < Simulation.CENTER_HOLE_RADIUS))
			{
				Connections.Clear();
				return;
			}

			// move the tile
			Position += translation;
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