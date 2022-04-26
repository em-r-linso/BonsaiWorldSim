using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Media;

namespace BonsaiWorldSim
{
	public class Tile
	{
		public Tile(Vector2 position, Brush color = null)
		{
			Position = position;

			Id = MainWindow.NextId;

			Connections = new();

			Color = color
			     ?? new[]
			        {
				        Brushes.White,
				        Brushes.Brown,
				        Brushes.Green,
				        Brushes.Coral,
				        Brushes.Blue
			        }[Simulation.Random.Next(5)];

			Simulation.Tiles.Add(this);
		}

		string Id { get; }

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

		public override string ToString() => $"t{Id}";

		public void Connect(Vector2 direction)
		{
			// give up if the tile is overlapping with this one
			if (direction == Vector2.Zero)
			{
				return;
			}

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

			// DELETEME: handling weird bug
			if (connection.Connections.ContainsKey(direction * -1))
			{
				MainWindow.Popup("Connection already exists, but only from the other side???");
				return;
			}

			connection.Connections.Add(direction * -1, this);
		}

		public void Disconnect(Vector2 direction)
		{
			// give up if there's no connection in that direction
			if (!Connections.ContainsKey(direction))
			{
				return;
			}

			var connection = Connections[direction];

			// disconnect the tiles
			Connections.Remove(direction);
			connection.Connections.Remove(direction * -1);
		}

		void Translate(Vector2 translation, int degrees)
		{
			// don't translate if the tile is already moved
			if (AttemptedTranslation)
			{
				return;
			}

			AttemptedTranslation = true;

			// move connected tiles
			foreach (var (_, value) in Connections)
			{
				value.Translate(translation, degrees);
			}

			// push another tile out of the way if necessary
			var tileInTheWay = Simulation.GetTileAtPosition(Position + translation);
			tileInTheWay?.Translate(degrees);

			// if this would move the tile into the center hole, break all connections and don't translate
			if (!IgnoreExclusionZone && ((Position + translation) == Vector2.Zero))
			{
				// Connections.Clear();
				foreach (var (direction, _) in Connections.ToArray())
				{
					Disconnect(direction);
				}

				return;
			}

			// move the tile
			Position += translation;
		}

		public void Translate(int degrees)
		{
			var translation = Simulation.DegreesToRandomHexDirection(degrees);
			Translate(translation, degrees);
		}
	}
}