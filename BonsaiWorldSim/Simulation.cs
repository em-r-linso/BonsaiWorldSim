using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BonsaiWorldSim
{
	public class Simulation
	{
		const float CHANCE_OF_CONNECTING       = 10;
		const float CHANCE_OF_DISCONNECTING    = 10;
		const int   MIN_TILES_PER_EXPANSION    = 5;
		const int   MAX_TILES_PER_EXPANSION    = 15;
		const int   MAX_ATTEMPTS_TO_CLEAR_HOLE = 100;

		public static readonly Vector2[] DIRECTIONS =
		{
			new(1, 0),      // right
			new(0.5f, -1),  // up-right
			new(-0.5f, -1), // up-left
			new(-1, 0),     // left
			new(-0.5f, 1),  // down-left
			new(0.5f, 1)    // down-right
		};

		public static Random     Random { get; } = new();
		public static List<Tile> Tiles  { get; } = new();

		static Tile[] GetTilesAtPosition(Vector2 position)
		{
			return Tiles.Where(t => t.Position == position).ToArray();
		}

		public static Tile GetTileAtPosition(Vector2 position)
		{
			//TODO: move overlapping tiles processing out of this method
			ProcessOverlappingTilesAtPosition(position);

			var tilesAtPosition = GetTilesAtPosition(position);
			return tilesAtPosition.Length switch
			{
				0 => null,
				1 => tilesAtPosition[0],
				_ => throw new("Overlapping tiles at position " + position)
			};
		}

		static void ProcessOverlappingTilesAtPosition(Vector2 position)
		{
			var tilesAtPosition = GetTilesAtPosition(position);
			switch (tilesAtPosition.Length)
			{
				case 0 or 1:
					break;
				default:

					// remove all tiles except the first one
					//TODO: modify the first tile based on the others—e.g. it might become a mountain
					for (var i = 1; i < tilesAtPosition.Length; i++)
					{
						RemoveTile(tilesAtPosition[i]);
					}

					break;
			}
		}

		static void RemoveTile(Tile tile)
		{
			foreach (var (direction, _) in tile.Connections)
			{
				tile.Disconnect(direction);
			}

			Tiles.Remove(tile);
		}

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

		public static int HexDirectionToDegrees(Vector2 direction)
		{
			var directionIndex = Array.IndexOf(DIRECTIONS, direction);
			return directionIndex * 60;
		}

		static void NewTurn()
		{
			foreach (var tile in Tiles.ToArray()) // ToArray to avoid self-modifying collection
			{
				ProcessOverlappingTilesAtPosition(tile.Position);
				tile.AttemptedTranslation = false;
			}
		}

		public static void Expand()
		{
			// make a new tile, then move it out
			var newTiles = new Tile[Random.Next(MIN_TILES_PER_EXPANSION, MAX_TILES_PER_EXPANSION)];
			for (var i = 0; i < newTiles.Length; i++)
			{
				NewTurn();

				switch (i)
				{
					// first 7 tiles make a solid hexagon (because a solid hexagon is made of smaller hexagons)
					case > 0 and <= 7:
						newTiles[i - 1].Translate(DIRECTIONS[i % DIRECTIONS.Length]);
						break;

					// tiles after 7 are added on randomly
					case > 7:
						newTiles[i - 1].Translate(Random.Next(360));
						break;
				}

				newTiles[i] = new(Vector2.Zero) {IgnoreExclusionZone = true};
			}

			// connect all new tiles
			foreach (var a in newTiles)
			{
				foreach (var direction in DIRECTIONS)
				{
					var b = GetTileAtPosition(a.Position + direction);
					if ((b != null) && newTiles.Contains(b))
					{
						// a.Connect(direction);
					}
				}
			}

			// push new tiles (all connected together) out from the center
			var degrees  = Random.Next(360);
			for (var attempt = 0; attempt < MAX_ATTEMPTS_TO_CLEAR_HOLE; attempt++)
			{
				NewTurn();
				newTiles[0].Translate(degrees);

				// if no tile is at 0,0, then we've cleared the hole
				if(Tiles.All(tile => tile.Position != Vector2.Zero))
				{
					break;
				}

				// if it just isn't getting out of there, delete the tile at 0,0
				// NOTE: I'm honestly not sure if this check is a good idea,
				//       because it really shouldn't be getting stuck for 100 attempts
				if (attempt == (MAX_ATTEMPTS_TO_CLEAR_HOLE - 1))
				{
					RemoveTile(GetTileAtPosition(Vector2.Zero));
				}
			}

			foreach (var tile in newTiles)
			{
				tile.IgnoreExclusionZone = false;
			}

			// foreach (var tile in Tiles)
			// {
			// 	foreach (var direction in DIRECTIONS)
			// 	{
			// 		if (tile.Connections.ContainsKey(direction))
			// 		{
			// 			if (Random.Next(100) < CHANCE_OF_DISCONNECTING)
			// 			{
			// 				tile.Connections[direction].Connections.Remove(direction * -1);
			// 				tile.Connections.Remove(direction);
			// 			}
			// 		}
			//
			// 		else
			// 		{
			// 			var neighbor = GetTileAtPosition(tile.Position + direction);
			// 			if ((neighbor != null) && (Random.Next(100) < CHANCE_OF_CONNECTING))
			// 			{
			// 				tile.Connections.Add(direction, neighbor);
			// 				neighbor.Connections.Add(direction * -1, tile);
			// 			}
			// 		}
			// 	}
			// }
		}
	}
}