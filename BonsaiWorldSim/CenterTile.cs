using System.Linq;
using System.Numerics;

namespace BonsaiWorldSim
{
	public class CenterTile : Tile
	{
		public CenterTile() : base(null, new(0, 0)) {}

		public void Expand()
		{
			// pick one of the six connecting coordinates at random
			// var targetPosition = Simulation.DIRECTIONS[Simulation.Random.Next(Simulation.DIRECTIONS.Length)];

			foreach (var targetPosition in Simulation.DIRECTIONS)
			{


				// make a new tile at that position
				var newTile = new HabitableTile(this, targetPosition);

				// if there is already a tile there, make that tile a child of the new one and push it away
				var tileOccupyingTargetPosition = AllChildren.FirstOrDefault(tile => tile.Position == targetPosition);
				if (tileOccupyingTargetPosition != null)
				{
					tileOccupyingTargetPosition.ReParent(newTile);
					tileOccupyingTargetPosition.Move(targetPosition);
				}

				// add the new tile as a direct child of the center tile
				Children.Add(newTile);
			}

			// let new tiles try to parent neighboring tiles
			for (var i = 0; i < Simulation.DIRECTIONS.Length; i++)
			{
				if(Simulation.Random.Next(100) < Simulation.CHANCE_OF_PARENTING)
				{
					var nextDirection = i + 1;
					if (nextDirection >= Simulation.DIRECTIONS.Length)
					{
						nextDirection = 0;
					}

					var a = Simulation.TileAtPosition(Simulation.DIRECTIONS[i]);
					var b = Simulation.TileAtPosition(Simulation.DIRECTIONS[nextDirection]);

					b.ReParent(a);
					// TODO: instead of parenting, tiles should be simply connected—there's no point in having a head and tail
					// groups of tiles should move like tetrominos
				}
			}
		}
	}
}