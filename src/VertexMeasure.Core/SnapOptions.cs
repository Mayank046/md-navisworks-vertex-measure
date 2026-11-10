// MD Vertex Measure - snap option toggles
// Author: MD (Mayank Dholariya)
namespace VertexMeasure.Core
{
    public sealed class SnapOptions
    {
        public bool IncludeCenter = true;
        public bool IncludeEnds = true;
        public bool IncludeCorners = false;
        public bool IncludeFaceCenters = false;
        public bool IncludeQuadrants = true;
        public double BubbleRadius = 0.05; // model units; UI may auto-scale
        public int MaxItems = 400;
        public double DedupEpsilon = 1e-4;

        public SnapOptions Clone()
        {
            return new SnapOptions
            {
                IncludeCenter = IncludeCenter,
                IncludeEnds = IncludeEnds,
                IncludeCorners = IncludeCorners,
                IncludeFaceCenters = IncludeFaceCenters,
                IncludeQuadrants = IncludeQuadrants,
                BubbleRadius = BubbleRadius,
                MaxItems = MaxItems,
                DedupEpsilon = DedupEpsilon
            };
        }
    }
}
