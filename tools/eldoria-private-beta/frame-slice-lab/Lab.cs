using System.Text.Json;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using Eldoria.World4XCanon;
int checks=0;
void Need(bool v,string m){if(!v)throw new Exception(m);checks++;}
long clock=0;var budget=new RegionOneFrameSlice(()=>clock,1000000,4,16384);
Need(!budget.ShouldYield(256),"Fast work should not force a frame per batch");
clock=4000;Need(budget.ShouldYield(256),"Elapsed CPU budget must yield");
clock+=500000;budget.Resume();Need(!budget.ShouldYield(256),"Browser idle time must not consume resumed work budget");
clock=0;budget=new RegionOneFrameSlice(()=>clock,1000000,4,16384);
for(int i=0;i<63;i++)Need(!budget.ShouldYield(256),"Bound must allow incomplete work quota");
Need(budget.ShouldYield(256),"Frozen/coarse clock must still yield at work quota");
budget.Resume();Need(!budget.ShouldYield(256),"Resume must clear work quota");
try{budget.ShouldYield(0);Need(false,"invalid units");}catch(ArgumentOutOfRangeException){checks++;}
var raw=File.ReadAllBytes(args[0]);
Need(BitConverter.ToInt32(raw,8)==raw.Length,"GLB must be complete");int n=BitConverter.ToInt32(raw,12),start=20+n+8;
using var doc=JsonDocument.Parse(raw.AsMemory(20,n));var root=doc.RootElement;
var accessors=root.GetProperty("accessors");var views=root.GetProperty("bufferViews");
int Offset(JsonElement a,out int stride,int width){var view=views[a.GetProperty("bufferView").GetInt32()];stride=view.TryGetProperty("byteStride",out var s)?s.GetInt32():width;return start+view.GetProperty("byteOffset").GetInt32()+(a.TryGetProperty("byteOffset",out var b)?b.GetInt32():0);}
var vertices=new List<Vector3>();var indices=new List<int>();
var mesh=root.GetProperty("meshes").EnumerateArray().Single(x=>x.GetProperty("name").GetString()=="Authored low uplands with continuous rounded geological incision");
foreach(var p in mesh.GetProperty("primitives").EnumerateArray()){
 var a=accessors[p.GetProperty("attributes").GetProperty("POSITION").GetInt32()];Need(a.GetProperty("componentType").GetInt32()==5126,"position float32");int count=a.GetProperty("count").GetInt32(),offset=Offset(a,out int stride,12),basis=vertices.Count;
 for(int i=0;i<count;i++){int o=offset+i*stride;vertices.Add(new Vector3(BitConverter.ToSingle(raw,o),BitConverter.ToSingle(raw,o+4),BitConverter.ToSingle(raw,o+8)));}
 a=accessors[p.GetProperty("indices").GetInt32()];int type=a.GetProperty("componentType").GetInt32(),width=type==5125?4:2;offset=Offset(a,out stride,width);count=a.GetProperty("count").GetInt32();Need(type==5125||type==5123,"index unsigned integer");
 for(int i=0;i<count;i++)indices.Add(basis+(type==5125?BitConverter.ToInt32(raw,offset+i*stride):BitConverter.ToUInt16(raw,offset+i*stride)));
}
Need(indices.Count/3==733288,"Exact authored terrain triangle count");
(string hash,int frames,long entries,double milliseconds) Build(bool adaptive){
 var cells=new Dictionary<(int x,int z),List<int>>();var watch=Stopwatch.StartNew();var slice=new RegionOneFrameSlice(16384);int frames=0;long entries=0;
 for(int i=0;i<indices.Count;i+=3){var a=vertices[indices[i]];var b=vertices[indices[i+1]];var c=vertices[indices[i+2]];
  int x0=(int)MathF.Floor(MathF.Min(a.X,MathF.Min(b.X,c.X))/16),x1=(int)MathF.Floor(MathF.Max(a.X,MathF.Max(b.X,c.X))/16);
  int z0=(int)MathF.Floor(MathF.Min(a.Z,MathF.Min(b.Z,c.Z))/16),z1=(int)MathF.Floor(MathF.Max(a.Z,MathF.Max(b.Z,c.Z))/16);
  for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++){var k=(x,z);if(!cells.TryGetValue(k,out var list)){list=new List<int>();cells[k]=list;}list.Add(i);entries++;}
  if(((i/3)+1)%256==0&&(!adaptive||slice.ShouldYield(256))){frames++;slice.Resume();}
 }
 watch.Stop();using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);byte[] bytes=new byte[4];
 void Add(int x){BitConverter.TryWriteBytes(bytes,x);hash.AppendData(bytes);}
 foreach(var kv in cells.OrderBy(x=>x.Key.x).ThenBy(x=>x.Key.z)){Add(kv.Key.x);Add(kv.Key.z);Add(kv.Value.Count);foreach(int x in kv.Value)Add(x);}
 return(Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(),frames,entries,watch.Elapsed.TotalMilliseconds);
}
var baseline=Build(false);var candidate=Build(true);
Need(baseline.hash==candidate.hash&&baseline.entries==candidate.entries,"Every actual terrain bucket and ordered triangle index must be identical");
Need(baseline.frames==2864,"Reproduce forced frame count");Need(candidate.frames<baseline.frames,"Remove fixed tiny-batch frame floor");
var report=new{scope="Linux .NET contact-index algorithm laboratory on actual source GLB; NOT Unity/native/WebGL/iPhone performance",checks,source_sha256=Convert.ToHexString(SHA256.HashData(raw)).ToLowerInvariant(),triangles=indices.Count/3,buckets_sha256=baseline.hash,bucket_entries=baseline.entries,baseline_forced_frame_boundaries=baseline.frames,candidate_work_boundaries=candidate.frames,baseline_cpu_ms=baseline.milliseconds,candidate_cpu_ms=candidate.milliseconds,source_head="8ce635c61bcbee5b4da2e5f12dd5ba5b43a0d47e",branch_modified=false,unity_api_compiled=false,notes=new[]{"Single-pass CPU timings are warmup-sensitive and are NOT a causal speed comparison.","Only frame-boundary policy and exact bucket/index equality are evaluated here.","Unity mesh transforms/import, complete route queries, rendering and physical-phone stability remain unverified."}};
var output=JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true});File.WriteAllText(args[1],output+"\n");Console.WriteLine(output);
