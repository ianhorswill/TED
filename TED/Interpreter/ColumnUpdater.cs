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

    internal class ColumnUpdater<TColumn, TKey1, TKey2> : ColumnUpdater
    {
        public ColumnAccessor<TColumn, (TKey1, TKey2)> Accessor;
        public TablePredicate<TKey1, TKey2, TColumn> UpdateList;

        public ColumnUpdater(ColumnAccessor<TColumn, (TKey1, TKey2)> accessor, TablePredicate<TKey1, TKey2, TColumn> updateList)
        {
            Accessor = accessor;
            UpdateList = updateList;
        }

        public override void DoUpdates()
        {
            foreach (var (key1, key2, value) in UpdateList)
                Accessor[(key1, key2)] = value;
        }
    }
}
