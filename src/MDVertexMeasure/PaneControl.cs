// MD Vertex Measure - docked panel UI
// Author: MD (Mayank Dholariya)
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using VertexMeasure.Core;
using Nw = Autodesk.Navisworks.Api;

namespace MDVertexMeasure
{
    internal static class Theme
    {
        internal static readonly Color Surface = Color.FromArgb(243, 244, 246);
        internal static readonly Color Card = Color.White;
        internal static readonly Color CardBorder = Color.FromArgb(222, 225, 230);
        internal static readonly Color Text = Color.FromArgb(32, 33, 36);
        internal static readonly Color Subtle = Color.FromArgb(96, 100, 108);
        internal static readonly Color Accent = Color.FromArgb(0, 110, 130);
        internal static Font Ui(float size, FontStyle style = FontStyle.Regular) { return new Font("Segoe UI", size, style); }
        internal static Font Semibold(float size)
        {
            try
            {
                using (var probe = new Font("Segoe UI Semibold", size))
                    if (string.Equals(probe.Name, "Segoe UI Semibold", StringComparison.OrdinalIgnoreCase))
                        return new Font("Segoe UI Semibold", size);
            }
            catch { }
            return new Font("Segoe UI", size, FontStyle.Bold);
        }
    }

    internal sealed class Card : Panel
    {
        private readonly string _title;
        private readonly Font _titleFont = Theme.Semibold(9.75F);
        internal Card(string title, Control content)
        {
            _title = title;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Theme.Surface;
            Dock = DockStyle.Top;
            Margin = new Padding(0, 0, 0, 8);
            Padding = new Padding(12, 34, 12, 12);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            content.Dock = DockStyle.Top;
            content.BackColor = Theme.Card;
            Controls.Add(content);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Rounded(r, 6))
            using (var fill = new SolidBrush(Theme.Card))
            using (var pen = new Pen(Theme.CardBorder))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
            TextRenderer.DrawText(g, _title, _titleFont, new Point(12, 10), Theme.Text, TextFormatFlags.NoPadding);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _titleFont.Dispose();
            base.Dispose(disposing);
        }
        internal static GraphicsPath Rounded(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public sealed class MeasurePaneControl : UserControl
    {
        private readonly RadioButton _rbSelection = new RadioButton();
        private readonly RadioButton _rbEntire = new RadioButton();
        private readonly CheckBox _chkCenter = new CheckBox();
        private readonly CheckBox _chkEnds = new CheckBox();
        private readonly CheckBox _chkQuadrants = new CheckBox();
        private readonly CheckBox _chkCorners = new CheckBox();
        private readonly CheckBox _chkFaces = new CheckBox();
        private readonly NumericUpDown _numBubble = new NumericUpDown();
        private readonly CheckBox _chkAutoBubble = new CheckBox();
        private readonly ListView _list = new ListView();
        private readonly TextBox _txtStatus = new TextBox();
        private readonly Label _lblDistance = new Label();
        private readonly Label _lblSig = new Label();
        public MeasurePaneControl()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.Surface;
            AutoScroll = true;
            Padding = new Padding(10);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                BackColor = Theme.Surface
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var header = new Panel { Height = 52, Dock = DockStyle.Top, BackColor = Theme.Accent };
            header.Controls.Add(new Label
            {
                Text = "MD Vertex Measure",
                ForeColor = Color.White,
                Font = Theme.Semibold(12F),
                AutoSize = true,
                Location = new Point(12, 8)
            });
            header.Controls.Add(new Label
            {
                Text = "E3D-like snaps → pick bubbles → distance",
                ForeColor = Color.FromArgb(220, 240, 245),
                Font = Theme.Ui(8.25F),
                AutoSize = true,
                Location = new Point(12, 30)
            });
            root.Controls.Add(header);

            // Scope
            var scopeBody = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Dock = DockStyle.Top };
            _rbSelection.Text = "Current selection (recommended)";
            _rbSelection.Checked = true;
            _rbSelection.AutoSize = true;
            _rbEntire.Text = "Visible model (slow on huge files)";
            _rbEntire.AutoSize = true;
            scopeBody.Controls.Add(_rbSelection);
            scopeBody.Controls.Add(_rbEntire);
            root.Controls.Add(new Card("Scope", scopeBody));

            // Snap types
            var optBody = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Dock = DockStyle.Top };
            _chkCenter.Text = "Center / bend center";
            _chkCenter.Checked = true; _chkCenter.AutoSize = true;
            _chkEnds.Text = "End centers (pipe / steel)";
            _chkEnds.Checked = true; _chkEnds.AutoSize = true;
            _chkQuadrants.Text = "End quadrants (4 per end — elbow style)";
            _chkQuadrants.Checked = true; _chkQuadrants.AutoSize = true;
            _chkCorners.Text = "Corners (AABB / steel section)";
            _chkCorners.Checked = false; _chkCorners.AutoSize = true;
            _chkFaces.Text = "Face centers (equipment)";
            _chkFaces.Checked = false; _chkFaces.AutoSize = true;
            optBody.Controls.Add(_chkCenter);
            optBody.Controls.Add(_chkEnds);
            optBody.Controls.Add(_chkQuadrants);
            optBody.Controls.Add(_chkCorners);
            optBody.Controls.Add(_chkFaces);

            var bubbleRow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false };
            bubbleRow.Controls.Add(new Label { Text = "Bubble size:", AutoSize = true, Padding = new Padding(0, 4, 0, 0) });
            _numBubble.DecimalPlaces = 3;
            _numBubble.Minimum = 0.001M;
            _numBubble.Maximum = 50M;
            _numBubble.Increment = 0.01M;
            _numBubble.Value = 0.05M;
            _numBubble.Width = 80;
            bubbleRow.Controls.Add(_numBubble);
            _chkAutoBubble.Text = "Auto from selection";
            _chkAutoBubble.Checked = true;
            _chkAutoBubble.AutoSize = true;
            bubbleRow.Controls.Add(_chkAutoBubble);
            optBody.Controls.Add(bubbleRow);
            root.Controls.Add(new Card("Snap types", optBody));

            // Actions
            var actBody = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Dock = DockStyle.Top };
            actBody.Controls.Add(MakeButton("Show snaps + activate pick tool", AccentBtn, OnShowSnaps));
            actBody.Controls.Add(MakeButton("Clear snaps", SecondaryBtn, OnClear));
            actBody.Controls.Add(MakeButton("Copy distance to clipboard", SecondaryBtn, OnCopy));
            root.Controls.Add(new Card("Actions", actBody));

            // Measure readout
            var measBody = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Dock = DockStyle.Top };
            _lblDistance.Text = "Distance: —";
            _lblDistance.Font = Theme.Semibold(11F);
            _lblDistance.ForeColor = Theme.Accent;
            _lblDistance.AutoSize = true;
            measBody.Controls.Add(_lblDistance);
            _txtStatus.Multiline = true;
            _txtStatus.ReadOnly = true;
            _txtStatus.BorderStyle = BorderStyle.FixedSingle;
            _txtStatus.Height = 48;
            _txtStatus.Width = 300;
            _txtStatus.Dock = DockStyle.Top;
            _txtStatus.BackColor = Color.White;
            _txtStatus.Text = "Select piping / steel / equipment, then Show snaps.";
            measBody.Controls.Add(_txtStatus);
            root.Controls.Add(new Card("Measure", measBody));

            // Snap list
            var listBody = new Panel { Height = 180, Dock = DockStyle.Top };
            _list.Dock = DockStyle.Fill;
            _list.View = System.Windows.Forms.View.Details;
            _list.FullRowSelect = true;
            _list.HideSelection = false;
            _list.Columns.Add("Label", 70);
            _list.Columns.Add("Kind", 70);
            _list.Columns.Add("Item", 120);
            _list.Columns.Add("X", 60);
            _list.Columns.Add("Y", 60);
            _list.Columns.Add("Z", 60);
            _list.DoubleClick += OnListDoubleClick;
            listBody.Controls.Add(_list);
            root.Controls.Add(new Card("Snaps (double-click = pick)", listBody));

            _lblSig.Text = Names.Author + "  ·  v" + Names.Version;
            _lblSig.ForeColor = Theme.Subtle;
            _lblSig.Font = Theme.Ui(7.5F);
            _lblSig.AutoSize = true;
            _lblSig.Dock = DockStyle.Top;
            _lblSig.Padding = new Padding(4, 8, 0, 4);
            root.Controls.Add(_lblSig);

            Controls.Add(root);

            SnapSession.Instance.Changed += OnSessionChanged;
            Disposed += (s, e) => { try { SnapSession.Instance.Changed -= OnSessionChanged; } catch { } };
        }

        private Button MakeButton(string text, Action<Button> style, EventHandler click)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = false,
                Width = 300,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.Ui(9F),
                Margin = new Padding(0, 0, 0, 6),
                Cursor = Cursors.Hand
            };
            style(b);
            b.Click += click;
            return b;
        }

        private static void AccentBtn(Button b)
        {
            b.BackColor = Theme.Accent;
            b.ForeColor = Color.White;
            b.FlatAppearance.BorderSize = 0;
        }

        private static void SecondaryBtn(Button b)
        {
            b.BackColor = Color.White;
            b.ForeColor = Theme.Text;
            b.FlatAppearance.BorderColor = Theme.CardBorder;
            b.FlatAppearance.BorderSize = 1;
        }

        private SnapOptions ReadOptions()
        {
            var o = new SnapOptions
            {
                IncludeCenter = _chkCenter.Checked,
                IncludeEnds = _chkEnds.Checked,
                IncludeQuadrants = _chkQuadrants.Checked,
                IncludeCorners = _chkCorners.Checked,
                IncludeFaceCenters = _chkFaces.Checked,
                BubbleRadius = (double)_numBubble.Value,
                MaxItems = 400
            };
            return o;
        }

        private void OnShowSnaps(object sender, EventArgs e)
        {
            Nw.Document doc = Nw.Application.ActiveDocument;
            if (doc == null)
            {
                MessageBox.Show("Open a model first.", Names.Product, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            bool selectionOnly = _rbSelection.Checked;
            if (!selectionOnly)
            {
                var ask = MessageBox.Show(
                    "Entire-model snaps can be slow and cluttered on big files.\r\nPrefer current selection.\r\n\r\nContinue anyway?",
                    Names.Product, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (ask != DialogResult.Yes) return;
            }
            else if (doc.CurrentSelection == null || doc.CurrentSelection.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select one or more piping, steel or equipment items first.", Names.Product,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SnapOptions opts = ReadOptions();
            // soft warn on large selection
            int n = 0;
            try
            {
                foreach (Nw.ModelItem it in doc.CurrentSelection.SelectedItems)
                {
                    n++;
                    if (n > SnapExtractor.HugeWarnThreshold) break;
                }
            }
            catch { }
            if (selectionOnly && n > SnapExtractor.HugeWarnThreshold)
            {
                var ask = MessageBox.Show(
                    "Selection looks large (>" + SnapExtractor.HugeWarnThreshold + " items). Continue with a capped extract?",
                    Names.Product, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (ask != DialogResult.Yes) return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                SnapExtractor.Result result = SnapExtractor.Extract(doc, selectionOnly, opts);
                double bubble = opts.BubbleRadius;
                if (_chkAutoBubble.Checked && result.HasSelectionBox)
                    bubble = SnapMath.SuggestBubbleRadius(result.SelectionMin, result.SelectionMax);
                _numBubble.Value = ClampDecimal((decimal)bubble);

                string note = string.Format(CultureInfo.InvariantCulture,
                    "{0} snaps from {1} item(s).", result.Snaps.Count, result.ItemsWithGeometry);
                if (!string.IsNullOrEmpty(result.Note)) note += " " + result.Note;
                SnapSession.Instance.BubbleRadius = bubble;
                SnapSession.Instance.SetSnaps(result.Snaps, bubble, note + " Activate pick tool and click From, then To.");

                if (!MeasureToolPlugin.Activate())
                    SnapSession.Instance.Status = note + " (tool activate failed — use list double-click to pick)";

                RefreshList();
                RefreshMeasure();
            }
            catch (Exception ex)
            {
                Diagnostics.Log("ShowSnaps: " + ex);
                MessageBox.Show("Failed to build snaps.\r\n\r\n" + ex.Message, Names.Product,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { Cursor = Cursors.Default; }
        }

        private void OnClear(object sender, EventArgs e)
        {
            MeasureToolPlugin.Deactivate();
            SnapSession.Instance.Clear();
            RefreshList();
            RefreshMeasure();
            try
            {
                Nw.Document doc = Nw.Application.ActiveDocument;
                if (doc != null && doc.ActiveView != null)
                    doc.ActiveView.RequestDelayedRedraw(Nw.ViewRedrawRequests.All);
            }
            catch { }
        }

        private void OnCopy(object sender, EventArgs e)
        {
            double? d = SnapSession.Instance.Distance();
            if (d == null)
            {
                MessageBox.Show("Pick From and To first.", Names.Product, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                Clipboard.SetText(d.Value.ToString("G8", CultureInfo.InvariantCulture));
                SnapSession.Instance.Status = "Copied distance " + d.Value.ToString("G6", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Clipboard failed: " + ex.Message, Names.Product, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnListDoubleClick(object sender, EventArgs e)
        {
            if (_list.SelectedIndices.Count == 0) return;
            SnapSession.Instance.PickSnap(_list.SelectedIndices[0]);
            RefreshMeasure();
            try
            {
                Nw.Document doc = Nw.Application.ActiveDocument;
                if (doc != null && doc.ActiveView != null)
                    doc.ActiveView.RequestDelayedRedraw(Nw.ViewRedrawRequests.All);
            }
            catch { }
        }

        private void OnSessionChanged()
        {
            if (IsDisposed) return;
            try
            {
                if (InvokeRequired) { BeginInvoke(new Action(OnSessionChanged)); return; }
                RefreshMeasure();
                // keep list selection in sync lightly
            }
            catch { }
        }

        private void RefreshMeasure()
        {
            _txtStatus.Text = SnapSession.Instance.Status;
            double? d = SnapSession.Instance.Distance();
            string units = "";
            try
            {
                Nw.Document doc = Nw.Application.ActiveDocument;
                if (doc != null) units = " " + doc.Units;
            }
            catch { }
            if (d == null) _lblDistance.Text = "Distance: —";
            else _lblDistance.Text = string.Format(CultureInfo.InvariantCulture, "Distance: {0:G8}{1}", d.Value, units);
        }

        private void RefreshList()
        {
            _list.BeginUpdate();
            _list.Items.Clear();
            var snaps = SnapSession.Instance.Snaps;
            for (int i = 0; i < snaps.Count; i++)
            {
                SnapPoint s = snaps[i];
                var item = new ListViewItem(s.Label);
                item.SubItems.Add(s.Kind.ToString());
                item.SubItems.Add(s.ItemName);
                item.SubItems.Add(s.Position.X.ToString("G5", CultureInfo.InvariantCulture));
                item.SubItems.Add(s.Position.Y.ToString("G5", CultureInfo.InvariantCulture));
                item.SubItems.Add(s.Position.Z.ToString("G5", CultureInfo.InvariantCulture));
                _list.Items.Add(item);
            }
            _list.EndUpdate();
        }

        private static decimal ClampDecimal(decimal v)
        {
            if (v < 0.001M) return 0.001M;
            if (v > 50M) return 50M;
            return Math.Round(v, 3);
        }
    }
}
