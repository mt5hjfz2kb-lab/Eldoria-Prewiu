using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.World4XPrototype
{
    // Isolated logical world. No dependency on WorldRegion1Runtime or Valoria scene.
    public enum ZoneTerrain : byte { Plain, Forest, Mountain, Breach, Sea }
    public enum ZoneFeature : byte { None, City, Wood, Stone, Food, EnemyCamp, Ruin }

    public readonly struct ZoneCell
    {
        public readonly int X, Z;
        public readonly ZoneTerrain Terrain;
        public readonly ZoneFeature Feature;
        public readonly ulong StableId;
        public bool Buildable => Terrain == ZoneTerrain.Plain && Feature == ZoneFeature.None;
        public ZoneCell(int x, int z, ZoneTerrain terrain, ZoneFeature feature)
        {
            X=x; Z=z; Terrain=terrain; Feature=feature;
            StableId=((ulong)(uint)x<<32)|(uint)z;
        }
    }

    /// <summary>Data-first 128x128 world, 16x16 chunks. Never creates Unity GameObjects.</summary>
    public sealed class ZoneOneWorld
    {
        public const int Size=128;
        public const int ChunkSize=16;
        public const int ChunkCount=Size/ChunkSize;
        public const int Seed=20261013;
        public const float CellWorldSize=2f;
        public bool Contains(int x,int z)=>x>=0&&z>=0&&x<Size&&z<Size;
        public Vector3 ToWorld(int x,int z)=>new Vector3((x-Size/2)*CellWorldSize,0,(z-Size/2)*CellWorldSize);
        static float Hash01(int x,int z,uint salt)
        {
            unchecked
            {
                uint h=(uint)x*374761393u+(uint)z*668265263u+salt+Seed;
                h=(h^(h>>13))*1274126177u;
                return (h^(h>>16))/4294967295f;
            }
        }
        public ZoneCell GetCell(int x,int z)
        {
            if(!Contains(x,z))throw new ArgumentOutOfRangeException();
            float dx=x-63.5f,dz=z-63.5f;
            float broad=Hash01(x/4,z/4,1);
            float fine=Hash01(x,z,2);
            float radius=Mathf.Sqrt(dx*dx+dz*dz);
            bool breach=radius<14+(broad-.5f)*5f;
            bool sea=z>Size-9+Mathf.RoundToInt((broad-.5f)*6);
            ZoneTerrain terrain=breach?ZoneTerrain.Breach:sea?ZoneTerrain.Sea:
                broad>.84f?ZoneTerrain.Mountain:broad>.70f?ZoneTerrain.Forest:ZoneTerrain.Plain;
            ZoneFeature feature=ZoneFeature.None;
            if(terrain==ZoneTerrain.Plain)
            {
                if(fine<.008f)feature=ZoneFeature.City;
                else if(fine<.020f)feature=ZoneFeature.EnemyCamp;
                else if(fine<.030f)feature=ZoneFeature.Ruin;
                else if(fine<.11f)
                {
                    int kind=Mathf.Min(2,Mathf.FloorToInt(Hash01(x,z,3)*3f));
                    feature=(ZoneFeature)((int)ZoneFeature.Wood+kind);
                }
            }
            return new ZoneCell(x,z,terrain,feature);
        }
        public IEnumerable<ZoneCell> GetChunk(int chunkX,int chunkZ)
        {
            if(chunkX<0||chunkZ<0||chunkX>=ChunkCount||chunkZ>=ChunkCount)
                yield break;
            for(int z=chunkZ*ChunkSize;z<(chunkZ+1)*ChunkSize;z++)
                for(int x=chunkX*ChunkSize;x<(chunkX+1)*ChunkSize;x++)
                    yield return GetCell(x,z);
        }
        public IEnumerable<Vector2Int> VisibleChunks(Vector3 cameraTarget,int radiusChunks)
        {
            int cx=Mathf.FloorToInt(cameraTarget.x/CellWorldSize+Size/2f)/ChunkSize;
            int cz=Mathf.FloorToInt(cameraTarget.z/CellWorldSize+Size/2f)/ChunkSize;
            for(int z=Mathf.Max(0,cz-radiusChunks);z<=Mathf.Min(ChunkCount-1,cz+radiusChunks);z++)
                for(int x=Mathf.Max(0,cx-radiusChunks);x<=Mathf.Min(ChunkCount-1,cx+radiusChunks);x++)
                    yield return new Vector2Int(x,z);
        }
    }
}
