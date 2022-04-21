using System.Linq;
using Xunit;

namespace BonsaiWorldSim.Tests
{
	public class TileConnections
	{
		[Theory]
		[InlineData(
			1,
			0,
			0.5f,
			-1
		)]
		[InlineData(
			999,
			888,
			777,
			666
		)]
		public void Connect_GivenNonOverlappingTiles_ShouldCreateMutualConnections(float aX,
			float                                                                        aY,
			float                                                                        bX,
			float                                                                        bY)
		{
			// Arrange
			Simulation.Tiles.Clear();
			var tileA = new Tile(new(aX, aY));
			var tileB = new Tile(new(bX, bY));

			// Act
			tileA.Connect(tileB.Position - tileA.Position);

			// Assert: tiles are connected to each other at all
			var isTileAToBConnected = tileA.Connections.ContainsValue(tileB);
			var isTileBToAConnected = tileB.Connections.ContainsValue(tileA);
			Assert.True(isTileAToBConnected && isTileBToAConnected, "Tiles didn't establish mutual connections.");

			// Assert: tiles are connected to each other with proper keys
			var tileAToBDirection = tileA.Connections.FirstOrDefault(connection => connection.Value == tileB).Key;
			var tileBToADirection = tileB.Connections.FirstOrDefault(connection => connection.Value == tileA).Key;
			Assert.True(
				tileAToBDirection == (tileBToADirection * -1),
				"Tile connections aren't at inverted directions."
			);
		}

		[Theory]
		[InlineData(
			0,
			0,
			0,
			0
		)]
		[InlineData(
			9,
			9,
			9,
			9
		)]
		public void Connect_GivenOverlappingTiles_ShouldNotCreateConnections(float aX,
		                                                                     float aY,
		                                                                     float bX,
		                                                                     float bY)
		{
			// Arrange
			Simulation.Tiles.Clear();
			var tileA = new Tile(new(aX, aY));
			var tileB = new Tile(new(bX, bY));

			// Act
			tileA.Connect(tileB.Position - tileA.Position);

			// Assert: tiles are connected to each other at all
			var isTileAToBConnected = tileA.Connections.ContainsValue(tileB);
			var isTileBToAConnected = tileB.Connections.ContainsValue(tileA);
			Assert.False(isTileAToBConnected && isTileBToAConnected, "Tiles didn't establish mutual connections.");
		}

		[Theory]
		[InlineData(
			1,
			0,
			0.5f,
			-1
		)]
		[InlineData(
			999,
			888,
			777,
			666
		)]
		public void Disconnect_ShouldRemoveConnection(float aX,
		                                              float aY,
		                                              float bX,
		                                              float bY)
		{
			// Arrange
			Simulation.Tiles.Clear();
			var tileA         = new Tile(new(aX, aY));
			var tileB         = new Tile(new(bX, bY));
			var directionAtoB = tileB.Position - tileA.Position;
			tileA.Connect(directionAtoB);

			// Act
			tileA.Disconnect(directionAtoB);

			// Assert: tiles are disconnected from each other
			var isTileAToBConnected = tileA.Connections.ContainsValue(tileB);
			var isTileBToAConnected = tileB.Connections.ContainsValue(tileA);
			Assert.True(!isTileAToBConnected && !isTileBToAConnected, "Tiles didn't disconnect.");

			// Assert: keys are totally free
			var isTileAKeyFree = !tileA.Connections.ContainsKey(directionAtoB);
			var isTileBKeyFree = !tileB.Connections.ContainsKey(directionAtoB);
			Assert.True(isTileAKeyFree && isTileBKeyFree, "Keys weren't freed up.");
		}
	}
}