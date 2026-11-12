# Issue186 runtime joins its existing Royal Gold assembly

Actual native43130030593/source3dff8222 failed compilation before import/capture. Artifact13905322901, ZIP55870abf564ebb3138490c966a6b3d1ae8cf8483bec4af9dfc2569bc5055edf9. Canonical RoyalGoldV41MailboxScreen belongs to Eldoria.RoyalGold.V2.asmdef, while Issue186Art Runtime had no assembly asset and therefore compiled into predefined Assembly-CSharp. A custom assembly cannot reference the predefined assembly. No16sprite/4PNG/input pass from this run.

Official Unity6000.3 documentation: an Assembly Definition Reference includes scripts from another folder in the referenced existing assembly, without making a new assembly. GUID reference JSON is supported. Sources:
https://docs.unity3d.com/6000.3/Documentation/Manual/class-AssemblyDefinitionReferenceImporter.html
https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definition-file-format.html

Add an asmref only inside the owned Issue186Art/Runtime folder, referencing the EXISTING Eldoria.RoyalGold.V2 asset GUID b60f4c3eae6b4a7a9733390e28114142. The bag, art adapter and saved UI bridge become members of that one existing Royal Gold assembly, which already references Domain, Application, Core, UI and Unity.ugui. Editor fixture siblings remain Editor-only. No new assembly, reference cycle, copied type, moved script/GUID, secondCanvas, reflection workaround or external asmdef edit.

This joins the actual mailbox and its real controls across the compiler boundary. JSON/GUID topology is inspected off-PC; actual Unity API compilation and input screenshots still require the next legitimate bounded native batch. Windows released187→179 main daef59c5 after clean exit; next181, then187. Preserve source ownership and no artistic approval/main/publication.
