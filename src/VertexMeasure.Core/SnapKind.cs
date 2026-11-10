// MD Vertex Measure - snap kinds
// Author: MD (Mayank Dholariya)
namespace VertexMeasure.Core
{
    public enum SnapKind
    {
        Center = 0,
        End = 1,
        Corner = 2,
        FaceCenter = 3,
        Quadrant = 4,
        BendCenter = 5
    }

    public enum ItemCategory
    {
        Unknown = 0,
        Piping = 1,
        Steel = 2,
        Equipment = 3
    }

    public sealed class SnapPoint
    {
        public Vec3 Position;
        public SnapKind Kind;
        public string Label;
        public string ItemName;
        public string ItemKey;
        public ItemCategory Category;

        public SnapPoint(Vec3 position, SnapKind kind, string label, string itemName, string itemKey, ItemCategory category)
        {
            Position = position;
            Kind = kind;
            Label = label ?? kind.ToString();
            ItemName = itemName ?? "";
            ItemKey = itemKey ?? "";
            Category = category;
        }
    }
}
