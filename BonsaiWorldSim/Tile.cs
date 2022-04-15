using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BonsaiWorldSim
{
	public class Tile
	{
		protected Tile(Tile parent, Vector2 position)
		{
			Parent   = parent;
			Position = position;
			Children = new();
		}

		public Vector2    Position { get; set; }
		public List<Tile> Children { get; set; }

		public Tile Parent { get; set; }

		/// <summary>
		///     Returns all neighboring tiles.
		/// </summary>
		public List<Tile> Neighbors
		{
			get
			{
				return Simulation.DIRECTIONS.Select(direction => Simulation.TileAtPosition(Position + direction))
				                 .Where(neighbor => neighbor != null)
				                 .ToList();
			}
		}

		/// <summary>
		///     Returns all children and their children, etc.
		/// </summary>
		internal List<Tile> AllChildren
		{
			get
			{
				var result = new List<Tile>();

				if (Children != null)
				{
					result.AddRange(Children);
					foreach (var child in Children)
						result.AddRange(child.AllChildren);
				}

				return result;
			}
		}

		/// <summary>
		///     Changes the position of the tile and all of its children.
		/// </summary>
		public void Move(Vector2 delta)
		{
			Position += delta;
			foreach (var tile in AllChildren)
				tile.Position += delta;
		}

		/// <summary>
		/// Changes this tile's parent and removes itself from its old parent's children.
		/// </summary>
		public void ReParent(Tile newParent)
		{
			Parent.Children.Remove(this);
			newParent.Children.Add(this);
			Parent = newParent;
		}
	}
}