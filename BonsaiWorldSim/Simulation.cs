using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BonsaiWorldSim
{
	public class Simulation
	{
		public const float CHANCE_OF_PARENTING = 50;

		public static readonly Vector2[] DIRECTIONS =
		{
			new(1, 0),
			new(-1, 0),
			new(0.5f, 1),
			new(0.5f, -1),
			new(-0.5f, 1),
			new(-0.5f, -1)
		};

		public Simulation()
		{
			CenterTile = new();
		}

		public static CenterTile CenterTile { get; set; }

		public static Random Random { get; } = new();

		public static List<Tile> AllTiles
		{
			get
			{
				var result = new List<Tile>();

				result.AddRange(CenterTile.AllChildren);
				result.Add(CenterTile);

				return result;
			}
		}

		public static Tile TileAtPosition(Vector2 position)
		{
			return AllTiles.FirstOrDefault(tile => tile.Position == position);
		}
	}
}