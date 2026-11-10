# MD Vertex Measure — quick use notes

**Setup:** the no-admin installer from Releases (`MD_Navisworks_VertexMeasure_NoAdmin_Setup_v1.0.1.exe`). Close Navisworks first; no administrator rights needed.  
**Author:** MD (Mayank Dholariya)

1. Close Navisworks → run the no-admin setup EXE → Install.
2. Open a model → **Tool Add-ins** → **MD Vertex Measure**.
3. Select an elbow / pipe / beam / equipment.
4. Keep **Center**, **Ends**, **End quadrants** ticked (elbow style: ~4 points per end + center).
5. **Show snaps + activate pick tool** → coloured bubbles appear in the view.
6. Click **From** bubble, then **To** bubble → distance (model units) in the panel.
7. Optional: **Copy distance to clipboard**. **Clear snaps** removes overlays.

**Snap rules (v1)**
| Category | Default key points |
|----------|-------------------|
| Piping (pipe, elbow, tee, reducer, flange, valve…) | Center (+ bend center for elbow/bend), EndA/EndB, 4 quadrants per end |
| Steel | Center, EndA/EndB, 4 section corners per end |
| Equipment / unknown | Center, ends/quadrants if toggled; face centers auto-enabled for equipment when corners/faces both off |

**Log:** `%APPDATA%\MDVertexMeasure\mdvertexmeasure.log`
