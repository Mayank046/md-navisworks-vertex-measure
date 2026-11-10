# MD Vertex Measure — developer notes

## Overlay
Uses `ToolPlugin.OverlayRender` + `Graphics.Sphere` (fallback `Cuboid`). Activated via
`Document.Tool.SetCustomToolPlugin`. Pick uses screen-projected nearest bubble within ~16 px.

## Snap math
`VertexMeasure.Core.SnapMath` — pure AABB heuristics. True NW primitive dump / mesh
tessellation is a future option when performance and API coverage allow.

## Years
Build against your own Navisworks API references (NwRefDir / NW_REFS) for 2017–2027. Freedom is excluded by the installer.
