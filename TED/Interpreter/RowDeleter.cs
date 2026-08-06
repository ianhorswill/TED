using TED.Tables;

namespace TED.Interpreter
{
    internal abstract class RowDeleter
    {
        public abstract void DoUpdates();
    }

    internal class RowDeleter<TKey> : RowDeleter
    {
        public readonly IKeyIndex<TKey> Index;
        public readonly TablePredicate<TKey> UpdateList;

        public RowDeleter(IKeyIndex<TKey> index, TablePredicate<TKey> updateList)
        {
            Index = index;
            UpdateList = updateList;
        }

        public override void DoUpdates()
        {
            var indexTableUntyped = Index.TableUntyped;
            foreach (var key in UpdateList) 
                indexTableUntyped.DeleteRow(Index.RowWithKey(key));
        }
    }

}
