using System;
using System.Collections.Generic;
using System.Linq;
using VertexMeasure.Core;
using Xunit;

namespace VertexMeasure.Tests
{
    public class CoreTests
    {
        [Fact]
        public void Distance_IsCorrect()
        {
            var a = new Vec3(0, 0, 0);
            var b = new Vec3(3, 4, 0);
            Assert.Equal(5.0, Vec3.Distance(a, b), 9);
        }

        [Fact]
        public void BboxCenter_AndCorners()
        {
            var min = new Vec3(0, 0, 0);
            var max = new Vec3(2, 4, 6);
            Vec3 c = SnapMath.BboxCenter(min, max);
            Assert.Equal(1.0, c.X, 9);
            Assert.Equal(2.0, c.Y, 9);
            Assert.Equal(3.0, c.Z, 9);
            Assert.Equal(8, SnapMath.Corners(min, max).Count());
            Assert.Equal(6, SnapMath.FaceCenters(min, max).Count());
        }

        [Fact]
        public void EndCenters_AlongLongestAxis()
        {
            // Long in X
            var min = new Vec3(0, 0, 0);
            var max = new Vec3(10, 2, 2);
            Vec3 a, b;
            SnapMath.EndCenters(min, max, out a, out b);
            Assert.Equal(0.0, a.X, 9);
            Assert.Equal(10.0, b.X, 9);
            Assert.Equal(1.0, a.Y, 9);
            Assert.Equal(1.0, a.Z, 9);
        }

        [Fact]
        public void EndQuadrants_FourPoints()
        {
            var center = new Vec3(5, 1, 1);
            var pts = SnapMath.EndQuadrants(center, 0, 0.5).ToList();
            Assert.Equal(4, pts.Count);
            Assert.All(pts, p => Assert.Equal(5.0, p.X, 9));
            Assert.Contains(pts, p => Math.Abs(p.Y - 1.5) < 1e-9);
            Assert.Contains(pts, p => Math.Abs(p.Y - 0.5) < 1e-9);
        }

        [Fact]
        public void OrientedBboxCorners_SteelSection()
        {
            var end = new Vec3(0, 0, 0);
            var size = new Vec3(10, 2, 4); // long X
            var corners = SnapMath.EndSectionCorners(end, 0, size).ToList();
            Assert.Equal(4, corners.Count);
            Assert.Contains(corners, p => Math.Abs(p.Y - 1) < 1e-9 && Math.Abs(p.Z - 2) < 1e-9);
        }

        [Fact]
        public void BuildForElbow_HasCenterEndsAndQuadrants()
        {
            var min = new Vec3(0, 0, 0);
            var max = new Vec3(4, 2, 2);
            var opts = new SnapOptions { IncludeCenter = true, IncludeEnds = true, IncludeQuadrants = true, IncludeCorners = false, IncludeFaceCenters = false };
            List<SnapPoint> snaps = SnapMath.BuildForItem(min, max, ItemCategory.Piping, "ELBOW-90", "g1", opts, true);
            Assert.Contains(snaps, s => s.Kind == SnapKind.BendCenter);
            Assert.DoesNotContain(snaps, s => s.Kind == SnapKind.Center);
            Assert.Equal(2, snaps.Count(s => s.Kind == SnapKind.End));
            Assert.Equal(8, snaps.Count(s => s.Kind == SnapKind.Quadrant));
        }

        [Fact]
        public void CategoryHints_PipingSteelEquipment()
        {
            Assert.Equal(ItemCategory.Piping, CategoryHints.Classify("ELBOW 90 LR", "Fitting"));
            Assert.Equal(ItemCategory.Steel, CategoryHints.Classify("UB 305x165", "Beam"));
            Assert.Equal(ItemCategory.Equipment, CategoryHints.Classify("V-101", "Vessel"));
        }

        [Fact]
        public void Dedup_KeepsPreferredKind()
        {
            var list = new List<SnapPoint>
            {
                new SnapPoint(new Vec3(1,1,1), SnapKind.Quadrant, "Q", "i", "k", ItemCategory.Piping),
                new SnapPoint(new Vec3(1,1,1), SnapKind.Center, "C", "i", "k", ItemCategory.Piping),
            };
            var d = SnapMath.Dedup(list, 1e-4);
            Assert.Single(d);
            Assert.Equal(SnapKind.Center, d[0].Kind);
        }

        [Fact]
        public void SuggestBubbleRadius_Clamped()
        {
            double r = SnapMath.SuggestBubbleRadius(new Vec3(0,0,0), new Vec3(100,0,0));
            Assert.InRange(r, 0.01, 0.5);
        }
    }
}
