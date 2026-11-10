# Changelog

## 1.0.1 — 2026-11-08

- No-admin, per-user installer: `MD_Navisworks_VertexMeasure_NoAdmin_Setup_v1.0.1.exe`.
- Installs to `%APPDATA%\Autodesk\ApplicationPlugins\MD.MDVertexMeasure.bundle`. No administrator rights required.
- Plugin assembly version remains 1.0.0 (installer packaging change only).

## 1.0.0 — 2026-11-08

Initial release.

- E3D-like snap bubbles (center, ends, end quadrants, corners, face centers) on selected piping, steel and equipment.
- Pick From / To bubbles (or list double-click) and read the distance in model units. Copy to clipboard.
- Overlay via a custom pick tool; works alongside Autodesk Measure.
- Administrator setup EXE for Navisworks Manage and Simulate 2017–2027.
- Core unit tests for snap math (no Navisworks required to run them).
