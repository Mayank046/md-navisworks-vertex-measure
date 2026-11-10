// MD Vertex Measure - E3D-like key-point generation from axis-aligned bbox
// Author: MD (Mayank Dholariya)
using System;
using System.Collections.Generic;

namespace VertexMeasure.Core
{
    /// <summary>
    /// Pure geometry helpers. Navisworks rarely exposes true E3D design points
    /// (P1/P2/leave), so we approximate from the item bounding box:
    /// center, end-face centers along the longest axis, quadrant points on end
    /// circles, AABB corners, and face centers.
    /// </summary>
    public static class SnapMath
    {
        public static Vec3 BboxCenter(Vec3 min, Vec3 max)
        {
            return new Vec3((min.X + max.X) * 0.5, (min.Y + max.Y) * 0.5, (min.Z + max.Z) * 0.5);
        }

        public static Vec3 BboxSize(Vec3 min, Vec3 max)
        {
            return new Vec3(Math.Abs(max.X - min.X), Math.Abs(max.Y - min.Y), Math.Abs(max.Z - min.Z));
        }

        public static double BboxDiagonal(Vec3 min, Vec3 max)
        {
            return BboxSize(min, max).Length;
        }

        /// <summary>Index of longest AABB axis: 0=X, 1=Y, 2=Z.</summary>
        public static int LongestAxis(Vec3 size)
        {
            if (size.X >= size.Y && size.X >= size.Z) return 0;
            if (size.Y >= size.X && size.Y >= size.Z) return 1;
            return 2;
        }

        public static IEnumerable<Vec3> Corners(Vec3 min, Vec3 max)
        {
            yield return new Vec3(min.X, min.Y, min.Z);
            yield return new Vec3(max.X, min.Y, min.Z);
            yield return new Vec3(min.X, max.Y, min.Z);
            yield return new Vec3(max.X, max.Y, min.Z);
            yield return new Vec3(min.X, min.Y, max.Z);
            yield return new Vec3(max.X, min.Y, max.Z);
            yield return new Vec3(min.X, max.Y, max.Z);
            yield return new Vec3(max.X, max.Y, max.Z);
        }

        public static IEnumerable<Vec3> FaceCenters(Vec3 min, Vec3 max)
        {
            Vec3 c = BboxCenter(min, max);
            yield return new Vec3(min.X, c.Y, c.Z);
            yield return new Vec3(max.X, c.Y, c.Z);
            yield return new Vec3(c.X, min.Y, c.Z);
            yield return new Vec3(c.X, max.Y, c.Z);
            yield return new Vec3(c.X, c.Y, min.Z);
            yield return new Vec3(c.X, c.Y, max.Z);
        }

        /// <summary>
        /// End-face centers of a pipe-like AABB (faces perpendicular to longest axis).
        /// </summary>
        public static void EndCenters(Vec3 min, Vec3 max, out Vec3 endA, out Vec3 endB)
        {
            Vec3 c = BboxCenter(min, max);
            Vec3 size = BboxSize(min, max);
            int axis = LongestAxis(size);
            endA = c;
            endB = c;
            if (axis == 0) { endA = new Vec3(min.X, c.Y, c.Z); endB = new Vec3(max.X, c.Y, c.Z); }
            else if (axis == 1) { endA = new Vec3(c.X, min.Y, c.Z); endB = new Vec3(c.X, max.Y, c.Z); }
            else { endA = new Vec3(c.X, c.Y, min.Z); endB = new Vec3(c.X, c.Y, max.Z); }
        }

        /// <summary>
        /// Approximate OD radius for end circles = half the mean of the two shorter axes.
        /// </summary>
        public static double ApproxRadius(Vec3 size, int longAxis)
        {
            double a, b;
            if (longAxis == 0) { a = size.Y; b = size.Z; }
            else if (longAxis == 1) { a = size.X; b = size.Z; }
            else { a = size.X; b = size.Y; }
            return Math.Max(1e-9, 0.5 * 0.5 * (a + b)); // half of mean width
        }

        /// <summary>
        /// Four quadrant points on a circle around endCenter, in the plane normal to axis.
        /// </summary>
        public static IEnumerable<Vec3> EndQuadrants(Vec3 endCenter, int longAxis, double radius)
        {
            Vec3 u, v;
            if (longAxis == 0) { u = new Vec3(0, 1, 0); v = new Vec3(0, 0, 1); }
            else if (longAxis == 1) { u = new Vec3(1, 0, 0); v = new Vec3(0, 0, 1); }
            else { u = new Vec3(1, 0, 0); v = new Vec3(0, 1, 0); }
            yield return endCenter + u * radius;
            yield return endCenter - u * radius;
            yield return endCenter + v * radius;
            yield return endCenter - v * radius;
        }

        /// <summary>
        /// Section corner approximations at each end (steel): ±u ±v around end center.
        /// </summary>
        public static IEnumerable<Vec3> EndSectionCorners(Vec3 endCenter, int longAxis, Vec3 size)
        {
            double hu, hv;
            Vec3 u, v;
            if (longAxis == 0) { hu = size.Y * 0.5; hv = size.Z * 0.5; u = new Vec3(0, 1, 0); v = new Vec3(0, 0, 1); }
            else if (longAxis == 1) { hu = size.X * 0.5; hv = size.Z * 0.5; u = new Vec3(1, 0, 0); v = new Vec3(0, 0, 1); }
            else { hu = size.X * 0.5; hv = size.Y * 0.5; u = new Vec3(1, 0, 0); v = new Vec3(0, 1, 0); }
            yield return endCenter + u * hu + v * hv;
            yield return endCenter + u * hu - v * hv;
            yield return endCenter - u * hu + v * hv;
            yield return endCenter - u * hu - v * hv;
        }

        public static List<SnapPoint> BuildForItem(
            Vec3 min, Vec3 max,
            ItemCategory category,
            string itemName, string itemKey,
            SnapOptions options,
            bool looksLikeElbow)
        {
            if (options == null) options = new SnapOptions();
            var snaps = new List<SnapPoint>();
            Vec3 size = BboxSize(min, max);
            Vec3 center = BboxCenter(min, max);
            int axis = LongestAxis(size);

            if (options.IncludeCenter)
            {
                if (looksLikeElbow)
                    snaps.Add(new SnapPoint(center, SnapKind.BendCenter, "BendCtr", itemName, itemKey, category));
                else
                    snaps.Add(new SnapPoint(center, SnapKind.Center, "Center", itemName, itemKey, category));
            }

            Vec3 endA, endB;
            EndCenters(min, max, out endA, out endB);

            if (options.IncludeEnds)
            {
                snaps.Add(new SnapPoint(endA, SnapKind.End, "EndA", itemName, itemKey, category));
                snaps.Add(new SnapPoint(endB, SnapKind.End, "EndB", itemName, itemKey, category));
            }

            if (options.IncludeQuadrants && (category == ItemCategory.Piping || category == ItemCategory.Unknown || looksLikeElbow))
            {
                double r = ApproxRadius(size, axis);
                int qi = 0;
                foreach (Vec3 q in EndQuadrants(endA, axis, r))
                    snaps.Add(new SnapPoint(q, SnapKind.Quadrant, "QA" + (++qi), itemName, itemKey, category));
                qi = 0;
                foreach (Vec3 q in EndQuadrants(endB, axis, r))
                    snaps.Add(new SnapPoint(q, SnapKind.Quadrant, "QB" + (++qi), itemName, itemKey, category));
            }

            if (options.IncludeCorners)
            {
                if (category == ItemCategory.Steel)
                {
                    int ci = 0;
                    foreach (Vec3 p in EndSectionCorners(endA, axis, size))
                        snaps.Add(new SnapPoint(p, SnapKind.Corner, "SA" + (++ci), itemName, itemKey, category));
                    ci = 0;
                    foreach (Vec3 p in EndSectionCorners(endB, axis, size))
                        snaps.Add(new SnapPoint(p, SnapKind.Corner, "SB" + (++ci), itemName, itemKey, category));
                }
                else
                {
                    int ci = 0;
                    foreach (Vec3 p in Corners(min, max))
                        snaps.Add(new SnapPoint(p, SnapKind.Corner, "C" + (++ci), itemName, itemKey, category));
                }
            }

            if (options.IncludeFaceCenters)
            {
                int fi = 0;
                foreach (Vec3 p in FaceCenters(min, max))
                    snaps.Add(new SnapPoint(p, SnapKind.FaceCenter, "F" + (++fi), itemName, itemKey, category));
            }

            return Dedup(snaps, options.DedupEpsilon);
        }

        public static List<SnapPoint> Dedup(List<SnapPoint> snaps, double epsilon)
        {
            if (snaps == null || snaps.Count == 0) return snaps ?? new List<SnapPoint>();
            double eps2 = epsilon * epsilon;
            var kept = new List<SnapPoint>(snaps.Count);
            foreach (SnapPoint s in snaps)
            {
                bool dup = false;
                for (int i = 0; i < kept.Count; i++)
                {
                    if ((kept[i].Position - s.Position).LengthSquared <= eps2)
                    {
                        // Prefer Center / End over Quadrant when coinciding
                        if (Priority(s.Kind) < Priority(kept[i].Kind))
                            kept[i] = s;
                        dup = true;
                        break;
                    }
                }
                if (!dup) kept.Add(s);
            }
            return kept;
        }

        private static int Priority(SnapKind k)
        {
            switch (k)
            {
                case SnapKind.Center: return 0;
                case SnapKind.BendCenter: return 1;
                case SnapKind.End: return 2;
                case SnapKind.FaceCenter: return 3;
                case SnapKind.Corner: return 4;
                default: return 5;
            }
        }

        public static double SuggestBubbleRadius(Vec3 min, Vec3 max)
        {
            double d = BboxDiagonal(min, max);
            if (d < 1e-9) return 0.05;
            return Math.Max(0.01, Math.Min(0.5, d * 0.03));
        }

        /// <summary>Screen-space nearest snap: returns index or -1.</summary>
        public static int NearestInScreen(
            IList<SnapPoint> snaps,
            Func<Vec3, Vec3> projectToScreen,
            double mouseX, double mouseY,
            double maxPixelDist)
        {
            if (snaps == null || snaps.Count == 0 || projectToScreen == null) return -1;
            double best = maxPixelDist * maxPixelDist;
            int idx = -1;
            for (int i = 0; i < snaps.Count; i++)
            {
                Vec3 s = projectToScreen(snaps[i].Position);
                // Z<=0 typically behind camera — skip if caller encodes that as Z
                double dx = s.X - mouseX;
                double dy = s.Y - mouseY;
                double d2 = dx * dx + dy * dy;
                if (d2 <= best) { best = d2; idx = i; }
            }
            return idx;
        }
    }
}
