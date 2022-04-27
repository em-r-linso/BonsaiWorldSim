using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Media;

namespace BonsaiWorldSim
{
	public class Tile
	{
		public Tile(Simulation simulation)
		{
			Simulation = simulation;

			Color       = (SolidColorBrush)new BrushConverter().ConvertFrom("#44ffffff");
			Connections = new();
		}

		public bool       AssignedMovement { get; set; }
		public bool       DidPull          { get; set; }
		public Vector2    Position         { get; set; }
		public Brush      Color            { get; }
		public List<Tile> Connections      { get; }

		Simulation Simulation { get; }
		Vector2    MoveIntent { get; set; }
		int        Angle      { get; set; }

		public void Push(int degrees)
		{
			// don't assign multiple movements to the same tile
			if (AssignedMovement)
			{
				return;
			}

			AssignedMovement = true;

			// set move intent
			Angle      = degrees;
			MoveIntent = Simulation.DegreesToRandomHexDirection(Angle);

			// move tiles out of the way
			MoveTilesOutOfTheWay();
		}

		public bool Pull(Tile tileToPull)
		{
			// don't assign multiple movements to the same tile
			if (tileToPull.AssignedMovement)
			{
				Disconnect(tileToPull);
				return false;
			}

			// random chance to fail
			if (Simulation.Random.NextDouble() < Simulation.CONNECTION_FAIL_CHANCE)
			{
				Disconnect(tileToPull);
				return false;
			}

			tileToPull.AssignedMovement = true;

			// set move intent according to puller
			tileToPull.MoveIntent = MoveIntent;
			tileToPull.Angle      = Angle;

			// move tiles out of the way
			tileToPull.MoveTilesOutOfTheWay();

			// return true to indicate that a pull happened
			return true;
		}

		void MoveTilesOutOfTheWay()
		{
			// if there are any tiles in the way, push them
			foreach (var tileInTheWay in Simulation.GetTilesAtPosition(Position + MoveIntent))
			{
				tileInTheWay.Push(Angle);

				// random chance to connect to the tile in the way
				if (Simulation.Random.NextDouble() < Simulation.PUSH_CONNECTION_CHANCE)
				{
					Connect(tileInTheWay);
				}
			}
		}

		public void Move()
		{
			if (AssignedMovement)
			{
				Position += MoveIntent;
				DisconnectFromDistantTiles();
			}

			AssignedMovement = false;
			DidPull          = false;
		}

		void DisconnectFromDistantTiles()
		{
			foreach (var connection in Connections
			                          .Where(connection => Vector2.Distance(Position, connection.Position) > 1.2f)
			                          .ToArray())
			{
				Disconnect(connection);
			}
		}

		public void Connect(Tile tile)
		{
			Connections.Add(tile);
			tile.Connections.Add(this);
		}

		public void Disconnect(Tile tile)
		{
			Connections.Remove(tile);
			tile.Connections.Remove(this);
		}
	}
}