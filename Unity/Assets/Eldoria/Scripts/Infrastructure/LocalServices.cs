using System;
using System.IO;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEngine;

namespace Eldoria.Infrastructure
{
    public sealed class SystemClock : IClock { public long UtcTicks => DateTime.UtcNow.Ticks; }
    public sealed class SeededRandom : IRandomSource
    {
        private readonly System.Random random;
        public SeededRandom(int seed) { random = new System.Random(seed); }
        public int Next(int exclusiveMax) => random.Next(exclusiveMax);
    }
    public sealed class SliceCatalog : IContentCatalog
    {
        public string DisplayName(string id)
        {
            switch (id) { case "forest-valoria": return "Bosque de Valoria";
                case "sawmill": return "Aserradero"; case "corrupt-scout": return "Explorador corrupto";
                default: return id; }
        }
    }
    public sealed class FileStateStore : IStateStore
    {
        public string PathName { get; }
        public FileStateStore(string path) { PathName = path; }
        public PlayerState Load()
        {
            if (!File.Exists(PathName)) return null;
            try
            {
                var state = JsonUtility.FromJson<PlayerState>(File.ReadAllText(PathName));
                if (state == null || state.SchemaVersion != 1) throw new InvalidDataException("Unknown save version");
                return state;
            }
            catch (Exception e) { throw new InvalidDataException("Save unreadable. Backup preserved at " + PathName + ".bak", e); }
        }
        public void Save(PlayerState state)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PathName));
            string temp = PathName + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(state, true));
            if (File.Exists(PathName)) File.Replace(temp, PathName, PathName + ".bak");
            else File.Move(temp, PathName);
        }
    }
}
