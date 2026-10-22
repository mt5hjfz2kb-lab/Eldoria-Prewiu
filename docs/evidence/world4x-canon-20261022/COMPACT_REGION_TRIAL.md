# Protected-centre geographic composition trial

Camera correction3957e9dd80867d38f5a655bb443535f31e32d29c retained after native39861900214, artifact12462342243,7/7passed,15.0544491sec. Same actual near city models now show their facade; portrait panorama again zoom456 and width421.42m. Bounds+/-700 are inside actual+/-780m terrain.

Dominant remaining composition defect: distant woodland/landmarks span roughly800m horizontally, so portrait still shows a central strip. Changing only a reported core width would be false. This candidate physically reauthors outer geography in the same one-world coordinate system.

Mapping: X remains identical within[-110,+110]world units; outside, distance from the protected edge is multiplied by0.26. Z is unchanged. This mapping is constant at all three zooms, not a camera-specific map. Real gameplay object transforms, colliders and saves never move. The original126near trees remain at actual source coordinates. Authored distant groves and200decorative midrange copies are remapped; crown proportions stay unchanged. Painted pigment uses the inverse map; height field, coastline, river/tributary/lake masks, road masks and forest acceptance use the same domain. Breach heart remains fixed at(8,61).

The actual support mesh still spans1820x1560m. Its unchanged200x200grid assigns70%of columns to mapped authored terrain and30%to the outer support area, improving coast sampling without increasing80000triangles. This is a real vertex/UV field change, not a background reference image or fake region bound. No new reference image is generated.

Acceptance: existing coast/near target/scale/Breach gates unchanged; additionally require all THREE actual distant canopy renderer bounds to fit inside the portrait camera's world-X interval and retain at least600of the688trees (two quads each). Compare real native captures against retained camera/ground source. Watch for compressed rocks/terrain brush marks, coast distortion, trees in water, protected central drift or overlap. Do not claim entire-region/reference approval just because the new bounds gate passes. Original CoreHalfWidth constants are deliberately not relabelled before physical evidence.

Native artifact transfer lesson: whole-file signed-URL downloads sometimes returned truncatedZIPs despite exit0. HTTP Range requests of4MiB, checking Content-Range, exact total bytes and ZIP CRC, reliably recovered the full actual artifacts. Local helper transfer-helper/artifact_ranges.py. No source or image content reconstructed.
