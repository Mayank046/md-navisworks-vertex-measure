// MD Vertex Measure - extract key snaps from selected ModelItems
// Author: MD (Mayank Dholariya)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using VertexMeasure.Core;
using Nw = Autodesk.Navisworks.Api;

namespace MDVertexMeasure
{
    internal static class SnapExtractor
    {
        public const int HugeWarnThreshold = 500;

        public sealed class Result
        {
            public List<SnapPoint> Snaps = new List<SnapPoint>();
            public int ItemsConsidered;
            public int ItemsWithGeometry;
            public int ItemsSkipped;
            public Vec3 SelectionMin;
            public Vec3 SelectionMax;
            public bool HasSelectionBox;
            public string Note = "";
        }

        public static Result Extract(Nw.Document doc, bool selectionOnly, SnapOptions options)
        {
            var result = new Result();
            if (doc == null) { result.Note = "No active document."; return result; }
            if (options == null) options = new SnapOptions();

            var items = new List<Nw.ModelItem>();
            foreach (Nw.ModelItem it in EnumerateScope(doc, selectionOnly))
            {
                result.ItemsConsidered++;
                if (it == null || !SafeHasGeometry(it)) continue;
                items.Add(it);
            }
            result.ItemsWithGeometry = items.Count;

            if (items.Count == 0)
            {
                result.Note = selectionOnly
                    ? "Nothing with geometry in the current selection."
                    : "No geometry items found.";
                return result;
            }

            if (items.Count > options.MaxItems)
            {
                result.Note = string.Format(CultureInfo.InvariantCulture,
                    "Limited to first {0} of {1} geometry items (raise Max or select fewer).",
                    options.MaxItems, items.Count);
                items = items.GetRange(0, options.MaxItems);
                result.ItemsSkipped = result.ItemsWithGeometry - options.MaxItems;
            }

            bool firstBox = true;
            Vec3 gmin = new Vec3(), gmax = new Vec3();
            foreach (Nw.ModelItem item in items)
            {
                Nw.BoundingBox3D box;
                try { box = item.BoundingBox(); }
                catch { result.ItemsSkipped++; continue; }
                if (box == null || box.IsEmpty) { result.ItemsSkipped++; continue; }

                Vec3 min = ToVec(box.Min);
                Vec3 max = ToVec(box.Max);
                if (firstBox) { gmin = min; gmax = max; firstBox = false; }
                else
                {
                    gmin = new Vec3(Math.Min(gmin.X, min.X), Math.Min(gmin.Y, min.Y), Math.Min(gmin.Z, min.Z));
                    gmax = new Vec3(Math.Max(gmax.X, max.X), Math.Max(gmax.Y, max.Y), Math.Max(gmax.Z, max.Z));
                }

                string display = SafeDisplay(item);
                string className = SafeClass(item);
                ItemCategory cat = CategoryHints.Classify(display, className);
                // Apply category-aware option defaults when user left corners/faces off:
                // equipment: prefer corners+faces; steel: ends+corners; piping: ends+quadrants
                SnapOptions local = options.Clone();
                if (cat == ItemCategory.Equipment || cat == ItemCategory.Unknown)
                {
                    if (!options.IncludeCorners && !options.IncludeFaceCenters)
                    {
                        // keep user toggles; Unknown gets center+ends+quadrants already
                    }
                    // For equipment always add corners if IncludeCorners is on; if both corner/face off,
                    // still give face centers? No — respect toggles. Equipment with only center is weak,
                    // so when category is equipment and corners/faces both off, force face centers.
                    if (cat == ItemCategory.Equipment && !local.IncludeCorners && !local.IncludeFaceCenters)
                        local.IncludeFaceCenters = true;
                }
                if (cat == ItemCategory.Steel && !local.IncludeCorners)
                    local.IncludeCorners = true; // section corners are the useful steel snaps

                bool elbow = LooksLikeElbow(display, className);
                string key = SafeKey(item);
                List<SnapPoint> built = SnapMath.BuildForItem(min, max, cat, display, key, local, elbow);
                result.Snaps.AddRange(built);
            }

            result.Snaps = SnapMath.Dedup(result.Snaps, options.DedupEpsilon);
            if (!firstBox)
            {
                result.SelectionMin = gmin;
                result.SelectionMax = gmax;
                result.HasSelectionBox = true;
            }
            Diagnostics.Log(string.Format(CultureInfo.InvariantCulture,
                "Extract considered={0} geom={1} snaps={2} skipped={3}",
                result.ItemsConsidered, result.ItemsWithGeometry, result.Snaps.Count, result.ItemsSkipped));
            return result;
        }

        private static IEnumerable<Nw.ModelItem> EnumerateScope(Nw.Document doc, bool selectionOnly)
        {
            if (selectionOnly)
            {
                foreach (Nw.ModelItem it in doc.CurrentSelection.SelectedItems)
                    if (it != null) yield return it;
                yield break;
            }
            foreach (Nw.Model model in doc.Models)
            {
                if (model == null) continue;
                Nw.ModelItem root = null;
                try { root = model.RootItem; } catch { }
                if (root == null) continue;
                foreach (Nw.ModelItem it in root.DescendantsAndSelf)
                    if (it != null) yield return it;
            }
        }

        private static bool SafeHasGeometry(Nw.ModelItem item)
        {
            try { return item.HasGeometry; } catch { return false; }
        }

        private static string SafeDisplay(Nw.ModelItem item)
        {
            try { return item.DisplayName ?? ""; } catch { return ""; }
        }

        private static string SafeClass(Nw.ModelItem item)
        {
            try
            {
                if (item.ClassDisplayName != null) return item.ClassDisplayName;
            }
            catch { }
            try { return item.ClassName ?? ""; } catch { return ""; }
        }

        private static string SafeKey(Nw.ModelItem item)
        {
            try
            {
                Guid g = item.InstanceGuid;
                if (g != Guid.Empty) return g.ToString("D");
            }
            catch { }
            try { return item.DisplayName ?? "item"; } catch { return "item"; }
        }

        private static bool LooksLikeElbow(string display, string className)
        {
            string t = ((display ?? "") + " " + (className ?? "")).ToUpperInvariant();
            return t.Contains("ELBOW") || t.Contains("BEND");
        }

        private static Vec3 ToVec(Nw.Point3D p) { return new Vec3(p.X, p.Y, p.Z); }
    }
}
