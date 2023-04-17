using TED.Tables;

namespace TED.Interpreter
{
    internal abstract class ColumnUpdater
    {
        public abstract void DoUpdates();
    }

    internal class ColumnUpdater<TColumn, TKey> : ColumnUpdater
    {
        public ColumnAccessor<TColumn, TKey> Accessor;
        public TablePredicate<TKey, TColumn> UpdateList;

        public ColumnUpdater(ColumnAccessor<TColumn, TKey> accessor, TablePredicate<TKey, TColumn> updateList)
        {
            Accessor = accessor;
            UpdateList = updateList;
        }

        public override void DoUpdates()
        {
            foreach (var (key, value) in UpdateList)
                Accessor[key] = value;
        }
    }
}
