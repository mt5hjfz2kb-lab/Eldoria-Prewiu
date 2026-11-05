using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class SyntaxProbe
{
    static int Main()
    {
        var files=new[]{
            "Unity/Assets/Eldoria/ConstructionDelight/ValoriaConstructionDelightV1.cs",
            "Unity/Assets/Eldoria/ConstructionDelight/ConstructionDelightArchitecturalReveal.cs",
            "Unity/Assets/Eldoria/ConstructionDelight/ConstructionDelightStageAssembly.cs",
            "Unity/Assets/Eldoria/ConstructionDelight/Editor/ConstructionDelightNativeCapture.cs",
            "Unity/Assets/Eldoria/Scripts/Presentation/ConstructionDelightState.cs"
        };
        int errors=0;
        foreach(string path in files)
        {
            if(!File.Exists(path))
            {
                Console.Error.WriteLine("MISSING "+path);
                errors++;
                continue;
            }
            var text=File.ReadAllText(path);
            var tree=CSharpSyntaxTree.ParseText(text,
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest),
                path);
            var failures=tree.GetDiagnostics()
                .Where(x=>x.Severity==DiagnosticSeverity.Error).ToArray();
            foreach(var error in failures)
            {
                Console.Error.WriteLine("C_SHARP_SYNTAX_ERROR "+error.Id+
                    " "+error.Location.GetLineSpan()+" "+error.GetMessage());
                errors++;
            }
            Console.WriteLine("CONSTRUCTION_DELIGHT_SYNTAX_FILE "+
                              path+" syntax_errors="+failures.Length);
            if(path.EndsWith("/ConstructionDelightStageAssembly.cs",StringComparison.Ordinal))
            {
                // Prevent the expensive visual regression where a completed
                // building disappears and reassembles from zero again.
                var complete=tree.GetCompilationUnitRoot().DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .SingleOrDefault(m=>m.Identifier.ValueText=="Complete");
                string body=complete?.Body?.ToFullString()??"";
                bool preserves=body.Contains("bool retain=working && Running",
                    StringComparison.Ordinal) &&
                    body.Contains("if(!retain)",StringComparison.Ordinal) &&
                    body.Contains("part.transitionPosition",StringComparison.Ordinal);
                bool notResetUpfront=!body.TrimStart().StartsWith("{\n            Clear();",
                    StringComparison.Ordinal);
                if(!preserves || !notResetUpfront)
                {
                    Console.Error.WriteLine("CONSTRUCTION_DELIGHT_STAGE_CONTINUITY_FAIL "+
                        "completion destroys authentic in-progress mesh poses");
                    errors++;
                }
                else Console.WriteLine("CONSTRUCTION_DELIGHT_STAGE_CONTINUITY_PASS "+
                                       "work-to-finish geometry maintained");
            }
            if(path.EndsWith("/ValoriaConstructionDelightV1.cs",StringComparison.Ordinal))
            {
                // Prevent a real regression found in Unity's scene-site
                // fallback: VisibleBounds accidentally called itself forever.
                // A syntax-only green check cannot catch that recursion.
                var methods=tree.GetCompilationUnitRoot().DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m=>m.Identifier.ValueText=="VisibleBounds").ToArray();
                if(methods.Length!=1)
                {
                    Console.Error.WriteLine("CONSTRUCTION_DELIGHT_BOUNDS_GUARD_FAIL "+
                        "expected exactly one VisibleBounds method");
                    errors++;
                }
                else
                {
                    var method=methods[0];
                    bool callsSelf=method.DescendantNodes().OfType<InvocationExpressionSyntax>()
                        .Any(call=>call.Expression is IdentifierNameSyntax ident &&
                                   ident.Identifier.ValueText=="VisibleBounds");
                    string body=method.Body?.ToFullString()??"";
                    bool inspectsActualRenderers=body.Contains("GetComponentsInChildren<Renderer>",
                                                              StringComparison.Ordinal);
                    bool aggregatesActualBounds=body.Contains("bounds.Encapsulate",
                                                              StringComparison.Ordinal);
                    if(callsSelf || !inspectsActualRenderers || !aggregatesActualBounds)
                    {
                        Console.Error.WriteLine("CONSTRUCTION_DELIGHT_BOUNDS_GUARD_FAIL "+
                            "recursive or non-renderer VisibleBounds fallback");
                        errors++;
                    }
                    else Console.WriteLine("CONSTRUCTION_DELIGHT_BOUNDS_GUARD_PASS "+
                                           "scaffold bounds from visible renderers; no self recursion");
                }
            }
        }
        // StageAssembly must render authentic building parts, never leave the
        // source hidden after completion, and never copy gameplay components.
        var staging="Unity/Assets/Eldoria/ConstructionDelight/"+
                    "ConstructionDelightStageAssembly.cs";
        if(File.Exists(staging))
        {
            string src=File.ReadAllText(staging);
            bool realMeshes=src.Contains("GetComponentsInChildren<MeshRenderer>(true)",
                                         StringComparison.Ordinal) &&
                            src.Contains("sharedMesh=filter.sharedMesh",
                                         StringComparison.Ordinal) &&
                            src.Contains("r.sharedMaterials=src.sharedMaterials",
                                         StringComparison.Ordinal);
            bool reversible=src.Contains("p.original.enabled=p.originalEnabled",
                                         StringComparison.Ordinal) &&
                            src.Contains("public void Clear()",StringComparison.Ordinal);
            bool noFakeProgress=src.Contains("deadline-nowTicks",StringComparison.Ordinal) &&
                                !src.Contains("Gateway.Execute(",StringComparison.Ordinal) &&
                                !src.Contains("PlayerState.",StringComparison.Ordinal);
            bool stageContinuity=src.Contains("bool retain=working && Running",
                                               StringComparison.Ordinal) &&
                                  src.Contains("if(!retain)",StringComparison.Ordinal) &&
                                  src.Contains("part.transitionPosition=part.wasVisible",
                                               StringComparison.Ordinal) &&
                                  src.Contains("p.transitionPosition,p.position",
                                               StringComparison.Ordinal) &&
                                  src.Contains("Mathf.Min(.69f",StringComparison.Ordinal);
            // The previous check expected an artificial hard cutoff at 70%
            // and forced all lower pieces to teleport into their final pose.
            // That produced visible resets. Validate real work-pose continuity
            // instead, without losing any save/renderer restoration guarantees.
            if(!realMeshes || !reversible || !noFakeProgress || !stageContinuity)
            {
                Console.Error.WriteLine("CONSTRUCTION_DELIGHT_STAGE_GUARD_FAIL");
                errors++;
            }
            else Console.WriteLine("CONSTRUCTION_DELIGHT_STAGE_GUARD_PASS "+
                "original mesh/material copy; reversible flags; real timer read-only");
        }
        else { Console.Error.WriteLine("CONSTRUCTION_DELIGHT_STAGE_MISSING");errors++; }

        // A real gateway completion must hand off the currently assembled
        // mesh copies before doing any visual reset or start-of-build rewind.
        var ownerSource="Unity/Assets/Eldoria/ConstructionDelight/ValoriaConstructionDelightV1.cs";
        if(File.Exists(ownerSource))
        {
            var owner=File.ReadAllText(ownerSource);
            if(!owner.Contains("StopWork(preserveStage)",StringComparison.Ordinal) ||
               !owner.Contains("bool preserveStage=actualCompletion && CanShow()",
                               StringComparison.Ordinal))
            {
                Console.Error.WriteLine("CONSTRUCTION_DELIGHT_GATEWAY_HANDOFF_FAIL "+
                    "world assembler cleared before authentic completion");
                errors++;
            }
            else Console.WriteLine("CONSTRUCTION_DELIGHT_GATEWAY_HANDOFF_PASS");
        }

        // The controlled technique A/B must compare the exact same Gateway
        // receipt, player camera, URP shader/Bloom and particles, changing only
        // the reversible authentic 3D staged geometry. No fake screenshots.
        var qa="Unity/Assets/Eldoria/ConstructionDelight/Editor/"+
               "ConstructionDelightNativeCapture.cs";
        if(!File.Exists(qa) || !File.Exists(ownerSource))
        {
            Console.Error.WriteLine("CONSTRUCTION_DELIGHT_TECHNIQUE_AB_MISSING");
            errors++;
        }
        else
        {
            var capture=File.ReadAllText(qa);
            var runtime=File.ReadAllText(ownerSource);
            bool exactCaseNames=capture.Contains("A-architectural-",StringComparison.Ordinal) &&
                                capture.Contains("B-staged-",StringComparison.Ordinal);
            bool bothStates=capture.Contains("fx.SetTechniqueForCapture(false)",StringComparison.Ordinal) &&
                            capture.Contains("fx.SetTechniqueForCapture(true)",StringComparison.Ordinal);
            bool sharedView=capture.Contains("TechniqueComparison(host,fx,\"work\"",StringComparison.Ordinal) &&
                            capture.Contains("TechniqueComparison(host,fx,\"finish\"",StringComparison.Ordinal) &&
                            capture.Contains("camera,\n                    \"technique-\"",StringComparison.Ordinal);
            bool geometrySwitch=runtime.Contains("public void SetTechniqueForCapture(bool useStagedAssembly)",StringComparison.Ordinal) &&
                                runtime.Contains("stagedBuilding.SetVisible(false)",StringComparison.Ordinal) &&
                                runtime.Contains("stagedBuilding.SetVisible(true)",StringComparison.Ordinal);
            bool matchingEffects=runtime.Contains("architectureOnlyReveal.Complete(completed,city.PlayerCamera,",StringComparison.Ordinal) &&
                                 runtime.Contains("architectureOnlyLustre.Begin(completed,Time.unscaledTime)",StringComparison.Ordinal) &&
                                 runtime.Contains("if(staged)stagedBuilding.SetVisible(false)",StringComparison.Ordinal) &&
                                 runtime.Contains("if(staged)stagedBuilding.SetVisible(true)",StringComparison.Ordinal) &&
                                 runtime.Contains("if(city && city.Probe)",StringComparison.Ordinal);
            bool expectedCount=capture.Contains("\\\"images\\\": 64",StringComparison.Ordinal) ||
                               capture.Contains("images=64",StringComparison.Ordinal);
            if(!exactCaseNames || !bothStates || !sharedView ||
               !geometrySwitch || !matchingEffects || !expectedCount)
            {
                Console.Error.WriteLine("CONSTRUCTION_DELIGHT_TECHNIQUE_AB_GUARD_FAIL "+
                    "two comparable techniques, real same-time camera and shader/Bloom required");
                errors++;
            }
            else Console.WriteLine("CONSTRUCTION_DELIGHT_TECHNIQUE_AB_GUARD_PASS "+
                "same Gateway and camera, synchronized URP reveal and 3D-only toggle");
        }

        // A resource path referenced by the runtime must exist in the exact
        // Unity checkout. Roslyn syntax alone cannot detect a missing shader.
        var shader="Unity/Assets/Eldoria/ConstructionDelight/Resources/"+
                   "ConstructionDelightArchitecturalReveal.shader";
        if(!File.Exists(shader) || !File.Exists(shader+".meta"))
        {
            Console.Error.WriteLine("CONSTRUCTION_DELIGHT_REVEAL_SHADER_MISSING");
            errors++;
        }
        else Console.WriteLine("CONSTRUCTION_DELIGHT_REVEAL_SHADER_RESOURCE_PRESENT");

        if(errors>0)return 1;
        Console.WriteLine("CONSTRUCTION_DELIGHT_SYNTAX_PASS files="+files.Length+
                          " compiler=RoslynSDK runtime_Unity_API_not_verified");
        return 0;
    }
}
