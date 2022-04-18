using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BonsaiWorldSim
{
	public class Simulation
	{
		public const float CHANCE_OF_CONNECTING    = 10;
		public const float CHANCE_OF_DISCONNECTING = 10;
		const        int   MIN_TILES_PER_EXPANSION = 5;
		const        int   MAX_TILES_PER_EXPANSION = 10;
		public const float CENTER_HOLE_RADIUS      = 1.5f;

		public static readonly Vector2[] DIRECTIONS =
		{
			new(1, 0),      //1
			new(0.5f, -1),  //2
			new(-0.5f, -1), //3
			new(-1, 0),     //4
			new(-0.5f, 1),  //5
			new(0.5f, 1)    //6
		};

		public static Random     Random { get; } = new();
		public static List<Tile> Tiles  { get; } = new();

		public static Tile GetTileAtPosition(Vector2 position)
		{
			return Tiles.FirstOrDefault(tile => tile.Position == position);
		}

		public static void Expand()
		{
			// make a new tile, then move it out
			var newTiles = new Tile[Random.Next(MIN_TILES_PER_EXPANSION, MAX_TILES_PER_EXPANSION)];
			for (var i = 0; i < newTiles.Length; i++)
			{
				switch (i)
				{
					// first 7 tiles make a solid hexagon (because a solid hexagon is made of smaller hexagons)
					case > 0 and <= 7:
						newTiles[i - 1].Translate(DIRECTIONS[i % DIRECTIONS.Length]);
						break;

					// tiles after 7 are added on randomly
					case > 7:
						newTiles[i - 1].Translate(DIRECTIONS[Random.Next(DIRECTIONS.Length)]);
						break;
				}

				newTiles[i] = new(Vector2.Zero) {IgnoreExclusionZone = true};

				// reset moved status of all tiles
				foreach (var tile in Tiles)
				{
					tile.AttemptedTranslation = false;
				}
			}

			// connect all new tiles
			foreach (var a in newTiles)
			{
				foreach (var direction in DIRECTIONS)
				{
					var b = GetTileAtPosition(a.Position + direction);
					if ((b != null) && newTiles.Contains(b))
					{
						a.Connections.Add(direction, b);
					}
				}
			}

			// push new tiles (all connected together) out from the center
			var directionA = Random.Next(DIRECTIONS.Length);
			var directionB = (directionA + 1) % DIRECTIONS.Length;
			var weight     = Random.Next(100);
			while (newTiles.Any(tile => tile.Position.Length() < CENTER_HOLE_RADIUS))
			{
				var direction = Random.Next(100) > weight ? directionA : directionB;

				newTiles[0].Translate(DIRECTIONS[direction]);

				// reset moved status of all tiles
				foreach (var tile in Tiles)
				{
					tile.AttemptedTranslation = false;
				}
			}

			foreach (var tile in newTiles)
			{
				tile.IgnoreExclusionZone = false;
			}

			foreach (var tile in Tiles)
			{
				foreach (var direction in DIRECTIONS)
				{
					if (tile.Connections.ContainsKey(direction))
					{
						if (Random.Next(100) < CHANCE_OF_DISCONNECTING)
						{
							tile.Connections[direction].Connections.Remove(direction * -1);
							tile.Connections.Remove(direction);
						}
					}

					else
					{
						var neighbor = GetTileAtPosition(tile.Position + direction);
						if ((neighbor != null) && (Random.Next(100) < CHANCE_OF_CONNECTING))
						{
							tile.Connections.Add(direction, neighbor);
							neighbor.Connections.Add(direction * -1, tile);
						}
					}
				}
			}
		}
	}
}