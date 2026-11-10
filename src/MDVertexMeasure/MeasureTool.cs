// MD Vertex Measure - ToolPlugin: draw bubbles + pick From/To
// Author: MD (Mayank Dholariya)
using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using VertexMeasure.Core;
using NwColor = Autodesk.Navisworks.Api.Color;

namespace MDVertexMeasure
{
    [Plugin(Ids.ToolName, Ids.Developer, DisplayName = "MD Vertex Measure Tool",
        ToolTip = "Pick snap bubbles to measure. Author: MD (Mayank Dholariya)")]
    public sealed class MeasureToolPlugin : ToolPlugin
    {
        public override void OverlayRender(View view, Graphics graphics)
        {
            if (view == null || graphics == null) return;
            IReadOnlyList<SnapPoint> snaps = SnapSession.Instance.Snaps;
            if (snaps == null || snaps.Count == 0) return;

            double r = SnapSession.Instance.BubbleRadius;
            int from = SnapSession.Instance.FromIndex;
            int to = SnapSession.Instance.ToIndex;

            try
            {
                graphics.DepthTest(false);
                graphics.DepthMask(false);

                for (int i = 0; i < snaps.Count; i++)
                {
                    SnapPoint s = snaps[i];
                    Point3D p = new Point3D(s.Position.X, s.Position.Y, s.Position.Z);
                    NwColor color = ColorFor(s.Kind, i == from, i == to);
                    double radius = r;
                    if (i == from || i == to) radius *= 1.35;
                    try
                    {
                        graphics.Color(color, 0.92);
                        graphics.Sphere(p, radius);
                    }
                    catch
                    {
                        try
                        {
                            graphics.Color(color, 0.92);
                            graphics.Cuboid(p,
                                new Vector3D(radius, 0, 0),
                                new Vector3D(0, radius, 0),
                                new Vector3D(0, 0, radius), true);
                        }
                        catch { }
                    }
                }

                if (from >= 0 && to >= 0 && from < snaps.Count && to < snaps.Count)
                {
                    Point3D a = new Point3D(snaps[from].Position.X, snaps[from].Position.Y, snaps[from].Position.Z);
                    Point3D b = new Point3D(snaps[to].Position.X, snaps[to].Position.Y, snaps[to].Position.Z);
                    graphics.Color(NwColor.FromByteRGB(255, 220, 60), 1.0);
                    graphics.LineWidth(3.0);
                    graphics.Line(a, b);
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log("OverlayRender: " + ex.Message);
            }
        }

        public override bool MouseDown(View view, KeyModifiers modifiers, ushort button, int x, int y, double timeOffset)
        {
            if (button != 1) return false;
            IReadOnlyList<SnapPoint> snaps = SnapSession.Instance.Snaps;
            if (snaps == null || snaps.Count == 0 || view == null) return false;

            int hit = FindNearestSnap(view, snaps, x, y, 16.0);
            if (hit < 0) return false;
            SnapSession.Instance.PickSnap(hit);
            try { view.RequestDelayedRedraw(ViewRedrawRequests.All); } catch { }
            return true;
        }

        public override Cursor GetCursor(View view, KeyModifiers modifiers)
        {
            return Cursor.MeasureVertex;
        }

        internal static int FindNearestSnap(View view, IReadOnlyList<SnapPoint> snaps, int mouseX, int mouseY, double maxPx)
        {
            double best = maxPx * maxPx;
            int idx = -1;
            for (int i = 0; i < snaps.Count; i++)
            {
                Point3D world = new Point3D(snaps[i].Position.X, snaps[i].Position.Y, snaps[i].Position.Z);
                ProjectionResult screen;
                try { screen = view.ProjectPoint(world, false, false); }
                catch { continue; }
                if (screen == null) continue;
                double dx = screen.X - mouseX;
                double dy = screen.Y - mouseY;
                double d2 = dx * dx + dy * dy;
                if (d2 <= best) { best = d2; idx = i; }
            }
            return idx;
        }

        private static NwColor ColorFor(SnapKind kind, bool isFrom, bool isTo)
        {
            if (isFrom) return NwColor.FromByteRGB(255, 160, 20);
            if (isTo) return NwColor.FromByteRGB(40, 210, 120);
            switch (kind)
            {
                case SnapKind.Center:
                case SnapKind.BendCenter:
                    return NwColor.FromByteRGB(255, 255, 255);
                case SnapKind.End:
                    return NwColor.FromByteRGB(70, 170, 255);
                case SnapKind.Quadrant:
                    return NwColor.FromByteRGB(255, 210, 70);
                case SnapKind.Corner:
                    return NwColor.FromByteRGB(200, 120, 255);
                case SnapKind.FaceCenter:
                    return NwColor.FromByteRGB(120, 220, 220);
                default:
                    return NwColor.FromByteRGB(200, 200, 200);
            }
        }

        /// <summary>Load tool plugin and set it as the document custom tool.</summary>
        public static bool Activate()
        {
            try
            {
                var doc = Autodesk.Navisworks.Api.Application.ActiveDocument;
                if (doc == null) return false;
                PluginRecord record = Autodesk.Navisworks.Api.Application.Plugins.FindPlugin(Ids.ToolId);
                if (record == null)
                {
                    Diagnostics.Log("Tool plugin record not found: " + Ids.ToolId);
                    return false;
                }
                if (record.LoadedPlugin == null) record.LoadPlugin();
                var tool = record.LoadedPlugin as ToolPlugin;
                if (tool == null) return false;
                doc.Tool.SetCustomToolPlugin(tool);
                SnapSession.Instance.ToolActive = true;
                try
                {
                    View view = doc.ActiveView;
                    if (view != null) view.RequestDelayedRedraw(ViewRedrawRequests.All);
                }
                catch { }
                return true;
            }
            catch (Exception ex)
            {
                Diagnostics.Log("Activate tool: " + ex);
                return false;
            }
        }

        public static void Deactivate()
        {
            try
            {
                var doc = Autodesk.Navisworks.Api.Application.ActiveDocument;
                if (doc != null) doc.Tool.Value = Tool.Select;
            }
            catch { }
            SnapSession.Instance.ToolActive = false;
        }
    }
}
