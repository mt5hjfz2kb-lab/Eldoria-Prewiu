using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

record struct Point(float X,float Y,float Z);
sealed class Baseline {
 public Dictionary<(int,int),List<(Point,int)>> Cells=new();
 public Baseline(Point[] points) { for(int i=0;i<points.Length;i++) {
  var p=points[i];var k=((int)Math.Floor(p.X*4f),(int)Math.Floor(p.Z*4f));
  if(!Cells.TryGetValue(k,out var a))Cells[k]=a=new();a.Add((p,i));
 }}
 public int Mark(Point p,HashSet<int> covered) {
  int x=(int)Math.Floor(p.X*4f),z=(int)Math.Floor(p.Z*4f),added=0;bool found=false;
  for(int dz=-1;dz<=1;dz++)for(int dx=-1;dx<=1;dx++)
   if(Cells.TryGetValue((x+dx,z+dz),out var a))foreach(var (v,i) in a){
    float vx=v.X-p.X,vy=v.Y-p.Y,vz=v.Z-p.Z;
    if(vx*vx+vy*vy+vz*vz<.002f*.002f){found=true;if(covered.Add(i))added++;}
   }
  return found?added:-1;
 }
}
static class Program {
 static void Need(bool b,string m){if(!b)throw new Exception(m);}
 static CompactTerrainVerification Compact(Point[] points){var c=new CompactTerrainVerification(points.Length);for(int i=0;i<points.Length;i++){var p=points[i];c.Add(i,p.X,p.Y,p.Z);}return c;}
 static Point[] Read(string path){using var g=new GZipStream(File.OpenRead(path),CompressionMode.Decompress);using var r=new BinaryReader(g);
  Need(r.ReadInt32()==0x31424c45,"query header");int nx=r.ReadInt32(),nz=r.ReadInt32();Need(r.ReadSingle()==1820f&&r.ReadSingle()==3600f,"world size");
  for(int i=0;i<nx*nz;i++)r.ReadSingle();Need(r.ReadBytes(nx*nz*4).Length==nx*nz*4,"mask bytes");
  Need(r.ReadInt32()==0x32564c45,"vertex header");int n=r.ReadInt32();var a=new Point[n];for(int i=0;i<n;i++)a[i]=new(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());return a;
 }
 static object Check(Point[] points,Point[] queries,string label){
  long before=GC.GetAllocatedBytesForCurrentThread();var b=new Baseline(points);long oldAllocation=GC.GetAllocatedBytesForCurrentThread()-before;
  before=GC.GetAllocatedBytesForCurrentThread();var c=Compact(points);long newAllocation=GC.GetAllocatedBytesForCurrentThread()-before;
  var oldSeen=new HashSet<int>();var newSeen=new bool[points.Length];int[] expected=new int[queries.Length];var watch=Stopwatch.StartNew();
  for(int i=0;i<queries.Length;i++)expected[i]=b.Mark(queries[i],oldSeen);watch.Stop();double oldMs=watch.Elapsed.TotalMilliseconds;
  watch.Restart();int covered=0,missing=0;
  for(int i=0;i<queries.Length;i++){var p=queries[i];int got=c.MarkMatches(p.X,p.Y,p.Z,newSeen);Need(got==expected[i],label+" mismatch query "+i);if(got>=0)covered+=got;else missing++;}
  watch.Stop();Need(covered==oldSeen.Count,label+" coverage count");for(int i=0;i<points.Length;i++)Need(newSeen[i]==oldSeen.Contains(i),label+" vertex coverage "+i);
  if(points.Length>100000)Need(newAllocation<oldAllocation*.4,"Expected substantial index allocation reduction");
  return new {label,vertices=points.Length,queries=queries.Length,covered,missing,buckets=b.Cells.Count,oldAllocation,newAllocation,bufferBytes=c.BufferBytes,oldMs,newMs=watch.Elapsed.TotalMilliseconds,passed=true};
 }
 public static void Main(string[] args){
  Need(args.Length==2,"Exact Unity source required for syntax check");
  var syntax=Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(File.ReadAllText(args[1]),new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9));
  var syntaxErrors=syntax.GetDiagnostics().Where(d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error).ToArray();
  Need(syntaxErrors.Length==0,string.Join("\n",syntaxErrors.Select(d=>d.ToString())));
  Console.WriteLine("FULL_UNITY_SOURCE_SYNTAX_PASS (not Unity assembly/API compilation)");
  var points=Read(args[0]);Need(points.Length==369163,"Exact known terrain vertex count");var results=new List<object>();
  results.Add(Check(points,points,"exact authored terrain"));
  var random=new Random(7341);var adversarial=new List<Point>();
  for(int i=0;i<12000;i++){var p=points[random.Next(points.Length)];float d=(i%7-3)*.001f;adversarial.Add(new(p.X+d,p.Y,p.Z));adversarial.Add(new(p.X,p.Y+10,p.Z));}
  results.Add(Check(points,adversarial.ToArray(),"real terrain near tolerance and rejected offsets"));
  var edge=new List<Point>();foreach(float x in new[]{-.251f,-.25f,-.249f,-.002f,0f,.002f,.249f,.25f,.251f})foreach(float z in new[]{-.25f,0,.25f}){edge.Add(new(x,0,z));edge.Add(new(x,0,z));}
  var edgeQueries=edge.Concat(edge.Select(p=>new Point(p.X+.002f,p.Y,p.Z))).Concat(edge.Select(p=>new Point(p.X,p.Y+2,p.Z))).ToArray();results.Add(Check(edge.ToArray(),edgeQueries,"cell boundaries duplicates strict tolerance"));
  var incomplete=points.Skip(1).Where(p=>p!=points[0]).ToArray();results.Add(Check(points,incomplete,"missing source coverage must remain missing"));
  string json=System.Text.Json.JsonSerializer.Serialize(new{scope="Exact C# compact index versus original algorithm, real exported terrain; NOT Unity runtime or iPhone",results},new System.Text.Json.JsonSerializerOptions{WriteIndented=true});File.WriteAllText("compact-index-results.json",json);Console.WriteLine(json);
 }
}
