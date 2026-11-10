# MD Vertex Measure

E3D-style **snap bubbles** (center, end centers, end quadrants, corners, face centers) on selected **piping**, **structural steel** and **equipment** in Navisworks. Pick two bubbles to measure the distance in model units. It works alongside Autodesk Measure — it does not replace it.

**Author:** MD (Mayank Dholariya)  
**Version:** 1.0.1 (no-admin installer; plugin assembly 1.0.0)  
**License:** MIT

## Screenshots

_Add screenshots of the snap bubbles in the view, the dock pane distance readout, and typical piping/steel snaps in `docs/images/`._

## Supported Navisworks versions

Autodesk Navisworks **Manage** and **Simulate** **2017–2027** (64-bit). Navisworks Freedom is not supported.

## Install (no administrator rights)

Download the latest installer from [Releases](../../releases). Close Navisworks first.

- **Recommended (v1.0.1):** run `MD_Navisworks_VertexMeasure_NoAdmin_Setup_v1.0.1.exe`. It installs for your Windows user only, to `%APPDATA%\Autodesk\ApplicationPlugins\MD.MDVertexMeasure.bundle`. No admin rights, no Program Files.
- **Uninstall:** Windows Settings → Apps → "MD Vertex Measure".

v1.0.0 shipped an administrator setup (`MD_Navisworks_VertexMeasure_Setup_v1.0.0.exe`) that installed under Program Files. Use the v1.0.1 no-admin installer on new machines.

## Use

1. Open a model → **Tool add-ins → MD Vertex Measure**.
2. Select piping, steel or equipment items.
3. Keep **Center**, **Ends** and **End quadrants** ticked (elbow style: about four points per end plus center).
4. **Show snaps + activate pick tool** — coloured bubbles appear in the view.
5. Click a **From** bubble, then a **To** bubble. The distance (model units) is shown in the panel. You can also double-click rows in the snap list.
6. Optional: **Copy distance to clipboard**. **Clear snaps** removes the overlays.

Snaps are approximated from each item's axis-aligned bounding box (longest-axis ends, quadrant circles, corners, face centers). They are **not** guaranteed to match true E3D design P-points. Prefer the current selection; an entire-model extract is capped and can clutter the view. The log is `%APPDATA%\MDVertexMeasure\mdvertexmeasure.log`.

More detail is in [docs/USE.md](docs/USE.md) and `installer/README.txt`.

## Build from source

You need the .NET SDK (8 or later is fine for tests; plugin builds target .NET Framework 4.6–4.8).

**Do not commit Autodesk Navisworks DLLs.** They are not redistributable. Point `NwRefDir` at your own Navisworks install.

```bash
# Core unit tests (no Navisworks required)
dotnet test tests/VertexMeasure.Tests/VertexMeasure.Tests.csproj -c Release

# Plugin DLL for one year, against your Navisworks install
dotnet build src/MDVertexMeasure/MDVertexMeasure.csproj -c Release \
  -p:NwYear=2024 \
  -p:NwRefDir="C:/Program Files/Autodesk/Navisworks Manage 2024" \
  -p:NwTfm=net48
```

`NwTfm` is `net46` for 2017–2018, `net47` for 2019–2020, and `net48` for 2021–2027.

To build every year, set `NW_REFS` to a directory of per-year API folders (`$NW_REFS/2017`, …) and run `./build_all.sh`. That script always runs the Core tests first.

Compile the no-admin installer with [Inno Setup 6](https://jrsoftware.org/isinfo.php) from `installer/VertexMeasure_NoAdmin.iss` after staging the bundle to `stage/MD.MDVertexMeasure.bundle/`. The older administrator script is `installer/VertexMeasure.iss`.

## Tests

```bash
dotnet test tests/VertexMeasure.Tests/VertexMeasure.Tests.csproj -c Release
```

The Core tests cover snap math only and do not need Navisworks.

## License

MIT © 2026 MD (Mayank Dholariya). See [LICENSE](LICENSE).
