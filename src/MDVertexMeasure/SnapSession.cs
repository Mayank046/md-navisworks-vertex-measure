// MD Vertex Measure - shared snap / measure state between pane and tool
// Author: MD (Mayank Dholariya)
using System;
using System.Collections.Generic;
using VertexMeasure.Core;

namespace MDVertexMeasure
{
    internal sealed class SnapSession
    {
        public static readonly SnapSession Instance = new SnapSession();

        private readonly object _gate = new object();
        private List<SnapPoint> _snaps = new List<SnapPoint>();
        private SnapOptions _options = new SnapOptions();
        private int _fromIndex = -1;
        private int _toIndex = -1;
        private bool _toolActive;
        private string _status = "Select items, then Show snaps.";
        private double _bubbleRadius = 0.05;

        public event Action Changed;

        public IReadOnlyList<SnapPoint> Snaps
        {
            get { lock (_gate) return _snaps.ToArray(); }
        }

        public SnapOptions Options
        {
            get { lock (_gate) return _options.Clone(); }
            set { lock (_gate) { _options = (value ?? new SnapOptions()).Clone(); } Raise(); }
        }

        public int FromIndex
        {
            get { lock (_gate) return _fromIndex; }
            set { lock (_gate) { _fromIndex = value; } Raise(); }
        }

        public int ToIndex
        {
            get { lock (_gate) return _toIndex; }
            set { lock (_gate) { _toIndex = value; } Raise(); }
        }

        public bool ToolActive
        {
            get { lock (_gate) return _toolActive; }
            set { lock (_gate) { _toolActive = value; } Raise(); }
        }

        public string Status
        {
            get { lock (_gate) return _status; }
            set { lock (_gate) { _status = value ?? ""; } Raise(); }
        }

        public double BubbleRadius
        {
            get { lock (_gate) return _bubbleRadius; }
            set { lock (_gate) { _bubbleRadius = Math.Max(0.001, value); } Raise(); }
        }

        public void SetSnaps(List<SnapPoint> snaps, double bubbleRadius, string status)
        {
            lock (_gate)
            {
                _snaps = snaps ?? new List<SnapPoint>();
                _fromIndex = -1;
                _toIndex = -1;
                if (bubbleRadius > 0) _bubbleRadius = bubbleRadius;
                _status = status ?? "";
            }
            Raise();
        }

        public void Clear()
        {
            lock (_gate)
            {
                _snaps = new List<SnapPoint>();
                _fromIndex = -1;
                _toIndex = -1;
                _status = "Snaps cleared.";
            }
            Raise();
        }

        public bool TryGetFrom(out SnapPoint snap)
        {
            lock (_gate)
            {
                if (_fromIndex >= 0 && _fromIndex < _snaps.Count) { snap = _snaps[_fromIndex]; return true; }
            }
            snap = null;
            return false;
        }

        public bool TryGetTo(out SnapPoint snap)
        {
            lock (_gate)
            {
                if (_toIndex >= 0 && _toIndex < _snaps.Count) { snap = _snaps[_toIndex]; return true; }
            }
            snap = null;
            return false;
        }

        public double? Distance()
        {
            SnapPoint a, b;
            if (!TryGetFrom(out a) || !TryGetTo(out b)) return null;
            return Vec3.Distance(a.Position, b.Position);
        }

        public void PickSnap(int index)
        {
            if (index < 0) return;
            lock (_gate)
            {
                if (index >= _snaps.Count) return;
                if (_fromIndex < 0)
                {
                    _fromIndex = index;
                    _toIndex = -1;
                    _status = "From: " + Label(_snaps[index]) + " — click To bubble.";
                }
                else if (_toIndex < 0)
                {
                    _toIndex = index;
                    double d = Vec3.Distance(_snaps[_fromIndex].Position, _snaps[_toIndex].Position);
                    _status = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "Distance = {0:G6}  ({1} → {2})", d, Label(_snaps[_fromIndex]), Label(_snaps[_toIndex]));
                }
                else
                {
                    // start new measure
                    _fromIndex = index;
                    _toIndex = -1;
                    _status = "From: " + Label(_snaps[index]) + " — click To bubble.";
                }
            }
            Raise();
        }

        private static string Label(SnapPoint s)
        {
            string name = string.IsNullOrEmpty(s.ItemName) ? s.ItemKey : s.ItemName;
            if (name.Length > 28) name = name.Substring(0, 28) + "…";
            return s.Label + "@" + name;
        }

        private void Raise()
        {
            try { var h = Changed; if (h != null) h(); } catch { }
        }
    }
}
