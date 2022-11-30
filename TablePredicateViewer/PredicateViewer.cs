using System.ComponentModel;
using TED;

namespace TablePredicateViewer
{
    public partial class PredicateViewer : Form
    {
        static readonly Dictionary<TablePredicate, PredicateViewer> viewers = new Dictionary<TablePredicate, PredicateViewer>();

        private bool isClosing;
        public bool SuppressUpdates;

        public static void ShowPredicates(params TablePredicate[] predicates)
        {
            foreach (var p in predicates)
                // Make sure it's in the table
                Of(p);
        }

        public static PredicateViewer Of(TablePredicate p)
        {
            if (!viewers.TryGetValue(p, out var viewer))
            {
                viewer = new PredicateViewer(p);
                viewer.Show();
                viewers[p] = viewer;
            }

            return viewer;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            isClosing = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            dataGridView.Size = ClientSize;
        }

        public static void UpdateAll()
        {
            foreach (var pair in viewers)
            {
                pair.Value.UpdateRows();
            }
        }

        public PredicateViewer(TablePredicate p)
        {
            Predicate = p;
            InitializeComponent();
            dataGridView.Columns.AddRange(Predicate.ColumnHeadings.Select(MakeColumn).ToArray());
            UpdateRows();
            Text = p.Name;
            // Only the last row appears if we don't do this
            dataGridView.Size = ClientSize;
        }

        public TablePredicate Predicate;

        public void UpdateRows()
        {
            if (isClosing || SuppressUpdates) return;

            var predicateRows = Predicate.Length;
            var viewRows = dataGridView.Rows.Count;
            var iterations = Math.Min(100, Math.Max(predicateRows, viewRows));
            for (var whyDoIHaveToDoThis = 0; whyDoIHaveToDoThis < 2; whyDoIHaveToDoThis++)
            for (var i = 0; i < iterations; i++)
            {
                if (i >= viewRows) AddRow();

                var r = dataGridView.Rows[i];

                if (i >= predicateRows)
                    // we have blank rows at the end
                    ClearRow(r);
                else
                    UpdateRow(i, r);
            }

            Text = $"{Predicate.Name} ({Predicate.Length} rows)";
        }

        private void UpdateRow(int i, DataGridViewRow r)
        {
            Predicate.RowToStrings((uint)i, RowBuffer);
            for (var j = 0; j < r.Cells.Count; j++)
                r.Cells[j].Value = RowBuffer[j];
        }

        private static void ClearRow(DataGridViewRow r)
        {
            foreach (DataGridViewTextBoxCell c in r.Cells)
                c.Value = "";
        }

        private void AddRow()
        {
            var newRow = new DataGridViewRow();
            for (var j = 0; j < Predicate.ColumnHeadings.Length; j++)
            {
                var c = new DataGridViewTextBoxCell();
                newRow.Cells.Add(c);
            }

            dataGridView.Rows.Add(newRow);
        }

        private static DataGridViewColumn MakeColumn(string name)
        {
            var col = new DataGridViewColumn();
            var cell = new DataGridViewTextBoxCell();
            col.CellTemplate = cell;
            var head = new DataGridViewColumnHeaderCell();
            col.HeaderCell = head;
            col.HeaderText = name;
            return col;
        }

        private static readonly string[] RowBuffer = new string[6];
    }
}