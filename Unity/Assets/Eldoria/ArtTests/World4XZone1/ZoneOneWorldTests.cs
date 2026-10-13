using NUnit.Framework;
using Eldoria.World4XPrototype;

public class ZoneOneWorldTests
{
    [Test] public void StableIds()
    {
        var world = new ZoneOneWorld();
        var cell = world.GetCell(50,50);
        Assert.AreEqual(cell.StableId, world.GetCell(50,50).StableId);
    }
    [Test] public void ChunksHave256Cells()
    {
        var world = new ZoneOneWorld();
        int count = 0;
        foreach(var cell in world.GetChunk(2,3)) count++;
        Assert.AreEqual(256,count);
    }
}
