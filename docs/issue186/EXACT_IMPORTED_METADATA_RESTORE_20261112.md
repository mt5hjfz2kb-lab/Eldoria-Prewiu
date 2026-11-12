# Exact temporary metadata recovery

Actual R21 audit 43149686136 archived64 .meta changes across245 overlay paths. All60 pinned PNG metadata keep the same GUID; 50 importers changed12→13 and10 stayed13. The four generated folder/preview/manifest metadata had no pinned installed metadata. The detailed forensic report in this directory is authoritative; this does not assert every new field has identical semantics.

Restoration recognizes only the exact reviewed current/installed/original SHA triples. Every original backup and every live path is checked before any restoration. Changed bytes are archived again. No generic metadata normalization is accepted. Before restoring the four generated metadata, the next Windows run scans serialized assets outside the temporary overlay for their four current GUIDs and refuses any reference. It then requires all245 original file states and a clean tracked source diff before a new checkout. No PR173 file is modified.

Future installation retains existing original metadata when the pinned UI contains its corresponding non-runtime folder/preview/manifest asset but omits its .meta, preserving the GUID. Actual recovery, combined input and final cleanup still need a new native pass. Art/main/physical phone remain unapproved.
