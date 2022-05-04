using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BonsaiWorldSim
{
	public class Simulation
	{
		public const float CONNECTION_FAIL_CHANCE = 0.05f;
		public const float PUSH_CONNECTION_CHANCE = 0.1f;

		const float CONNECT_CHANCE    = 0.05f;
		const float DISCONNECT_CHANCE = 0.01f;

		static readonly Vector2[] DIRECTIONS =
		{
			new(1, 0),      // right
			new(0.5f, -1),  // up-right
			new(-0.5f, -1), // up-left
			new(-1, 0),     // left
			new(-0.5f, 1),  // down-left
			new(0.5f, 1)    // down-right
		};

		// TODO: move Random out of Simulation
		public static Random Random { get; } = new();

		public List<Tile> Tiles { get; } = new();

		/// <summary>
		///     Returns one of the two hex directions that are closest to the given degree, weighted such that the hex direction
		///     closer to the given degree is more likely to be returned.
		///     <para />
		///     If used every time a tile moves, the tile will move in approximately the direction of the given degree, while
		///     always moving along the hex grid.
		/// </summary>
		public static Vector2 DegreesToRandomHexDirection(int degrees)
		{
			var closestDirectionLow  = DIRECTIONS[(int)((degrees        / 60f) % 6)];
			var closestDirectionHigh = DIRECTIONS[(int)(((degrees + 60) / 60f) % 6)];
			var percentFromLowToHigh = ((degrees % 60) / 60f) * 100;
			var randomValue          = Random.Next(100);

			return randomValue > percentFromLowToHigh ? closestDirectionLow : closestDirectionHigh;
		}

		/// <summary>
		///     Returns an array of all tiles at the given position.
		/// </summary>
		public Tile[] GetTilesAtPosition(Vector2 position)
		{
			return Tiles.Where(tile => tile.Position == position).ToArray();
		}

		/// <summary>
		///     Makes random connections (and disconnections) between tiles based on the constants CONNECT_CHANCE and
		///     DISCONNECT_CHANCE.
		/// </summary>
		void MakeRandomConnections()
		{
			foreach (var tile in Tiles)
			{
				foreach (var direction in DIRECTIONS)
				{
					foreach (var tileInDirection in GetTilesAtPosition(tile.Position + direction))
					{
						if (tile.Connections.Contains(tileInDirection))
						{
							if (Random.NextDouble() < DISCONNECT_CHANCE)
							{
								tile.Disconnect(tileInDirection);
							}
						}
						else if (Random.NextDouble() < CONNECT_CHANCE)
						{
							tile.Connect(tileInDirection);
						}
					}
				}
			}
		}

		/// <summary>
		///     Recursively calls Pull() on every tile based on their connections.
		/// </summary>
		void PullTiles()
		{
			var keepTrying = true;
			while (keepTrying)
			{
				keepTrying = false;

				// for every tile that is going to move...
				// (only once per tile)
				foreach (var tile in Tiles.Where(tile => tile.AssignedMovement && !tile.DidPull))
				{
					tile.DidPull = true;

					// pull every connection
					foreach (var connection in tile.Connections.ToArray())
					{
						if (tile.Pull(connection))
						{
							// if the pull was successful, there might be more connections to pull
							keepTrying = true;
						}
					}
				}
			}
		}

		/// <summary>
		///     Moves all tiles, disconnects invalid connections, then processes overlapping tiles.
		/// </summary>
		void MoveTiles()
		{
			foreach (var tile in Tiles)
			{
				tile.Move();
			}

			foreach (var tile in Tiles)
			{
				tile.DisconnectFromDistantTiles();
			}

			foreach (var tile in Tiles.ToArray())
			{
				var tilesAtPosition = GetTilesAtPosition(tile.Position);
				if (tilesAtPosition.Length > 1)
				{
					// create a new tile at this position, which merges features from the overlapping tiles
					Tiles.Add(
						new(this)
						{
							Position    = tile.Position,
							Connections = tilesAtPosition.SelectMany(t => t.Connections).ToList(),
							Altitude    = tilesAtPosition.Sum(t => t.Altitude)
						}
					);

					// delete the old tiles at this position
					foreach (var tileAtPosition in tilesAtPosition)
					{
						tileAtPosition.Delete();
					}
				}
			}
		}

		/// <summary>
		///     Creates a new tile at (0,0) and pushes outward.
		/// </summary>
		public void Expand()
		{
			// create new tile
			var newTile = new Tile(this);
			Tiles.Add(newTile);

			// make random connections
			MakeRandomConnections();

			// move tiles
			newTile.Push(Random.Next(360));
			PullTiles();
			MoveTiles();
		}
	}
}