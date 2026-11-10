MD Vertex Measure v1.0.1  -  E3D-like vertex / center snap bubbles for measure
Author / publisher: MD (Mayank Dholariya)
Copyright (c) 2026 MD (Mayank Dholariya)

NEEDS
  Autodesk Navisworks Manage or Simulate, 2017 to 2027, 64-bit.
  (Navisworks Freedom cannot run plugins.)

INSTALL
  1. Close Navisworks.
  2. Run MD_Navisworks_VertexMeasure_NoAdmin_Setup_v1.0.1.exe (no administrator rights).
  3. Next > Install. The add-in goes into your user profile and Navisworks 2017-2027 loads it automatically.
  Uninstall: Windows Settings > Apps > "MD Vertex Measure".

USE
  1. Open the NWD / NWF model in Navisworks.
  2. Tool Add-ins tab > MD Vertex Measure.
  3. Select piping, steel or equipment items.
  4. Tick snap types (Center / Ends / Quadrants recommended for elbows).
  5. Show snaps + activate pick tool — coloured bubbles appear.
  6. Click bubble A (From), then bubble B (To) — distance shows in the panel.
     Or double-click rows in the snap list.
  7. Copy distance to clipboard if needed. Clear snaps when done.

BUBBLE COLOURS
  White  = center / bend center
  Blue   = end centers
  Amber  = end quadrants (4 per end)
  Purple = corners
  Cyan   = face centers
  Orange / green = currently selected From / To

NOTES
  - Snaps are approximated from each item's bounding box (E3D-like key points).
    They are NOT guaranteed to match true E3D P-points / leave points.
  - Prefer current selection; entire-model extract is capped and can clutter the view.
  - Log: %APPDATA%\MDVertexMeasure\mdvertexmeasure.log
  - Problems? Send the log to MD.

LIMITS
  - Approximation from AABB / longest-axis heuristics — not full mesh vertices.
  - Does not replace Autodesk Measure tools; works in parallel via its own pick tool.
  - Does not write back to E3D / Plant 3D.
  - Freedom not supported.
