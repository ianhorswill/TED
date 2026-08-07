using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TED.Tables
{
    /// <summary>
    /// An order-preserving B+ tree whose stored elements are <see cref="uint"/> keys (typically row numbers),
    /// but whose *ordering* is defined over the <typeparamref name="T"/> value that each key maps to.
    ///
    /// The tree never stores <typeparamref name="T"/> values; instead the constructor is given:
    ///   * a <see cref="Func{UInt32,T}"/> that looks up the <typeparamref name="T"/> value for a key, and
    ///   * an <see cref="IComparer{T}"/> that orders those values.
    /// This makes it suitable for use as an ordered index over the rows of a <see cref="Table"/>: the keys are
    /// row numbers and the <typeparamref name="T"/> value is the contents of the column being ordered on.
    ///
    /// All data lives in the leaves, which are chained left-to-right so an in-order traversal is a simple linked-list
    /// walk.  Internal nodes hold only separator keys used for routing.
    ///
    /// Ties (two different keys whose <typeparamref name="T"/> values compare equal) are broken by the numeric value
    /// of the key itself, so the ordering is a strict total order and every key occupies a distinct position.  This
    /// lets the tree hold multiple rows that share the same column value, which an ordered index needs.
    ///
    /// STORAGE: nodes are not objects.  They live in a set of parallel flat arrays (a struct-of-arrays "arena") and a
    /// node is referred to by an <see cref="int"/> handle — an index shared across all the arrays.  A node's keys are
    /// a contiguous run of <c>order</c> entries in <see cref="keys"/> (at the default order, exactly one cache line of
    /// 4-byte keys), and its child handles are a contiguous run in <see cref="children"/>.  Because keys are
    /// <see cref="uint"/> and child links are <see cref="int"/> handles, the arena contains no managed references at
    /// all, so the GC never scans it — important for a large, long-lived index.  This also sidesteps the per-node
    /// array-object header the object-per-node layout would pay.
    ///
    /// Nodes are pooled: freed nodes (from splits/merges/clears) go on an integer free list and are handed back out by
    /// later insertions, so steady-state operation performs no allocation.  The arena grows by doubling.
    ///
    /// NOTE: <see cref="Remove"/> must be called while the removed key's <typeparamref name="T"/> value is still
    /// available from the lookup function — i.e. remove a row from the index *before* invalidating its table entry —
    /// because deletion navigates using comparisons.  Likewise the <typeparamref name="T"/> value associated with a
    /// key must not change while that key is in the tree.
    /// </summary>
    /// <typeparam name="T">Type of the value the keys are ordered by</typeparam>
    public sealed class BPlusTree<T> : IEnumerable<uint>
    {
        /// <summary>Size of a CPU cache line in bytes.  A node's key run is sized to fill whole cache lines.</summary>
        private const int CacheLineBytes = 64;

        /// <summary>Handle value meaning "no node" (end of a leaf chain, empty child slot, empty free list).</summary>
        private const int NoNode = -1;

        /// <summary>Initial number of node slots allocated the first time a node is needed.</summary>
        private const int InitialCapacity = 8;

        /// <summary>Maps a key to the value it is ordered by.</summary>
        private readonly Func<uint, T> valueOf;

        /// <summary>Orders the values keys map to.</summary>
        private readonly IComparer<T> comparer;

        /// <summary>Branching factor: the maximum number of children an internal node may have.</summary>
        private readonly int order;

        /// <summary>Stride between successive nodes' child runs in <see cref="children"/> (== order + 1).</summary>
        private readonly int childStride;

        /// <summary>Maximum number of keys in a node in the stable (non-overflowing) state.</summary>
        private readonly int maxKeys;

        /// <summary>Minimum number of keys a non-root node may hold before it must borrow or merge.</summary>
        private readonly int minKeys;

        //
        // The arena.  All five arrays are indexed by node handle; keys/children are strided runs (see the accessors).
        //

        /// <summary>Per node: true if it is a leaf (holds data keys), false if it is an internal routing node.</summary>
        private bool[] isLeaf;

        /// <summary>Per node: number of keys it currently holds.  Never large, so a byte suffices.</summary>
        private byte[] keyCount;

        /// <summary>
        /// Per node: for a leaf, the handle of the next leaf to the right (for in-order scans);
        /// also reused as the free-list link while the node is free.  <see cref="NoNode"/> when there is none.
        /// </summary>
        private int[] next;

        /// <summary>Keys, <c>order</c> per node.  Node n's keys are <c>keys[n*order .. n*order+order]</c>.</summary>
        private uint[] keys;

        /// <summary>Child handles, <c>order+1</c> per node.  Node n's children are <c>children[n*childStride ..]</c>.</summary>
        private int[] children;

        /// <summary>Number of node slots the arena arrays currently have room for.</summary>
        private int capacity;

        /// <summary>High-water mark: number of node slots ever handed out (free list reuses slots below this).</summary>
        private int allocated;

        /// <summary>Head of the free list of pooled node handles, or <see cref="NoNode"/>.</summary>
        private int freeList;

        /// <summary>Handle of the root node.  Always valid; an empty tree is a single empty leaf.</summary>
        private int root;

        /// <summary>Number of keys currently in the tree.</summary>
        private int count;

        /// <summary>Number of keys currently stored in the tree.</summary>
        public int Count => count;

        /// <summary>
        /// Make a new, empty B+ tree.
        /// </summary>
        /// <param name="valueOf">Looks up the value a key is ordered by</param>
        /// <param name="comparer">Orders the values keys map to</param>
        /// <param name="order">
        /// Branching factor (maximum children per internal node).  Must be between 4 and 255 — a node's key count is
        /// stored in a single byte.  Defaults to one cache line's worth of 4-byte keys so that a leaf's key run
        /// fills exactly one cache line.
        /// </param>
        public BPlusTree(Func<uint, T> valueOf, IComparer<T> comparer, int order = CacheLineBytes / sizeof(uint))
        {
            if (order < 4 || order > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(order), order,
                    $"B+ tree order must be between 4 and {byte.MaxValue}");
            this.valueOf = valueOf ?? throw new ArgumentNullException(nameof(valueOf));
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            this.order = order;
            childStride = order + 1;
            maxKeys = order - 1;
            minKeys = (order - 1) / 2;
            isLeaf = Array.Empty<bool>();
            keyCount = Array.Empty<byte>();
            next = Array.Empty<int>();
            keys = Array.Empty<uint>();
            children = Array.Empty<int>();
            freeList = NoNode;
            root = Rent(true);
        }

        #region Arena accessors
        /// <summary>The i'th key of a node.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint Key(int node, int i) => keys[node * order + i];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetKey(int node, int i, uint value) => keys[node * order + i] = value;

        /// <summary>The i'th child handle of a node.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Child(int node, int i) => children[node * childStride + i];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetChild(int node, int i, int child) => children[node * childStride + i] = child;
        #endregion

        #region Comparison
        /// <summary>
        /// Total order over keys: compare the values they map to, breaking ties by the numeric key so that distinct
        /// keys never compare equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CompareKeys(uint a, uint b)
        {
            if (a == b) return 0;
            var c = comparer.Compare(valueOf(a), valueOf(b));
            return c != 0 ? c : a.CompareTo(b);
        }
        #endregion

        #region Node pool
        /// <summary>
        /// Obtain a node handle, reusing one from the free list if available.  The returned node is reset to empty.
        /// </summary>
        private int Rent(bool leaf)
        {
            int id;
            if (freeList != NoNode)
            {
                id = freeList;
                freeList = next[id];
            }
            else
            {
                if (allocated == capacity) Grow();
                id = allocated++;
            }
            isLeaf[id] = leaf;
            keyCount[id] = 0;
            next[id] = NoNode;
            return id;
        }

        /// <summary>
        /// Return a node to the free list.  Its key/child slots are left as-is (they are plain integers, never read
        /// beyond the node's key count, and hold no references), so nothing needs clearing.
        /// </summary>
        private void Return(int id)
        {
            keyCount[id] = 0;
            next[id] = freeList;
            freeList = id;
        }

        /// <summary>Grow the arena arrays.  Existing node data keeps its indices, so live handles stay valid.</summary>
        private void Grow()
        {
            var newCapacity = capacity == 0 ? InitialCapacity : capacity * 2;
            Array.Resize(ref isLeaf, newCapacity);
            Array.Resize(ref keyCount, newCapacity);
            Array.Resize(ref next, newCapacity);
            Array.Resize(ref keys, newCapacity * order);
            Array.Resize(ref children, newCapacity * childStride);
            capacity = newCapacity;
        }
        #endregion

        #region Insertion
        /// <summary>
        /// Add a key to the tree.  If the key is already present the tree is unchanged.
        /// </summary>
        public void Insert(uint key)
        {
            if (InsertInto(root, key, out var splitKey, out var splitNode, out var added) && splitNode != NoNode)
            {
                // The root split; grow a new root one level up.
                var newRoot = Rent(false);
                SetChild(newRoot, 0, root);
                SetKey(newRoot, 0, splitKey);
                SetChild(newRoot, 1, splitNode);
                keyCount[newRoot] = 1;
                root = newRoot;
            }
            if (added) count++;
        }

        /// <summary>
        /// Insert <paramref name="key"/> into the subtree rooted at <paramref name="node"/>.
        /// Returns true if the node split, in which case <paramref name="splitKey"/> is the key to promote into the
        /// parent and <paramref name="splitNode"/> is the new right sibling (else <see cref="NoNode"/>).
        /// </summary>
        private bool InsertInto(int node, uint key, out uint splitKey, out int splitNode, out bool added)
        {
            splitKey = 0;
            splitNode = NoNode;

            if (isLeaf[node])
            {
                var idx = SearchLeaf(node, key);
                if (idx >= 0)
                {
                    // Already present.
                    added = false;
                    return false;
                }
                var pos = ~idx;
                for (int i = keyCount[node]; i > pos; i--) SetKey(node, i, Key(node, i - 1));
                SetKey(node, pos, key);
                keyCount[node]++;
                added = true;

                if (keyCount[node] <= maxKeys) return false;

                // Split the leaf: right half moves to a new leaf, whose smallest key is copied up as the separator.
                var mid = keyCount[node] / 2;
                var rightCount = keyCount[node] - mid;
                var right = Rent(true);
                for (var i = 0; i < rightCount; i++) SetKey(right, i, Key(node, mid + i));
                keyCount[right] = (byte)rightCount;
                keyCount[node] = (byte)mid;
                next[right] = next[node];
                next[node] = right;
                splitKey = Key(right, 0);
                splitNode = right;
                return true;
            }

            // Internal node: descend, then absorb any split coming back up.
            var ci = FindChildIndex(node, key);
            if (!InsertInto(Child(node, ci), key, out var childKey, out var childNode, out added))
                return false;

            // Insert the promoted separator childKey at position ci and its new node at ci+1.
            for (int i = keyCount[node]; i > ci; i--) SetKey(node, i, Key(node, i - 1));
            for (int i = keyCount[node] + 1; i > ci + 1; i--) SetChild(node, i, Child(node, i - 1));
            SetKey(node, ci, childKey);
            SetChild(node, ci + 1, childNode);
            keyCount[node]++;

            if (keyCount[node] <= maxKeys) return false;

            // Split the internal node: the middle key moves up (it is not copied), the rest go to a new right node.
            var m = keyCount[node] / 2;
            var rk = keyCount[node] - m - 1;
            var rightNode = Rent(false);
            for (var i = 0; i < rk; i++) SetKey(rightNode, i, Key(node, m + 1 + i));
            for (var i = 0; i <= rk; i++) SetChild(rightNode, i, Child(node, m + 1 + i));
            // The left node's now-surplus child slots (beyond m) are simply never read again — no need to clear them.
            keyCount[rightNode] = (byte)rk;
            splitKey = Key(node, m);
            keyCount[node] = (byte)m;
            splitNode = rightNode;
            return true;
        }
        #endregion

        #region Deletion
        /// <summary>
        /// Remove a key from the tree.  Returns true if the key was present.
        /// Must be called while the key's ordering value is still available from the lookup function.
        /// </summary>
        public bool Remove(uint key)
        {
            if (!DeleteFrom(root, key)) return false;
            count--;
            // If the root is an internal node that has collapsed to a single child, that child becomes the new root.
            if (!isLeaf[root] && keyCount[root] == 0)
            {
                var old = root;
                root = Child(root, 0);
                Return(old);
            }
            return true;
        }

        private bool DeleteFrom(int node, uint key)
        {
            if (isLeaf[node])
            {
                var idx = SearchLeaf(node, key);
                if (idx < 0) return false;
                for (int i = idx; i < keyCount[node] - 1; i++) SetKey(node, i, Key(node, i + 1));
                keyCount[node]--;
                return true;
            }

            var ci = FindChildIndex(node, key);
            var child = Child(node, ci);
            if (!DeleteFrom(child, key)) return false;

            // If the removed key was serving as the separator that routes into this child, replace it with the
            // child's new minimum so no reference to the removed key lingers in the tree.  (Leaf-with-nothing-left is
            // handled below by rebalancing, which either refills the child or removes the separator by merging.)
            if (ci > 0 && Key(node, ci - 1) == key && (!isLeaf[child] || keyCount[child] > 0))
                SetKey(node, ci - 1, MinKey(child));

            if (keyCount[child] < minKeys)
                Rebalance(node, ci);
            return true;
        }

        /// <summary>
        /// Restore the minimum-occupancy invariant for the under-full child at index <paramref name="ci"/> of
        /// <paramref name="node"/>, by borrowing a key from a sibling that has one to spare, or else merging with a sibling.
        /// </summary>
        private void Rebalance(int node, int ci)
        {
            if (ci > 0 && keyCount[Child(node, ci - 1)] > minKeys)
            {
                BorrowFromLeft(node, ci);
                return;
            }
            if (ci < keyCount[node] && keyCount[Child(node, ci + 1)] > minKeys)
            {
                BorrowFromRight(node, ci);
                return;
            }
            if (ci > 0) Merge(node, ci - 1);
            else Merge(node, ci);
        }

        /// <summary>Move one element from the left sibling into the child at <paramref name="ci"/>.</summary>
        private void BorrowFromLeft(int node, int ci)
        {
            var child = Child(node, ci);
            var left = Child(node, ci - 1);
            if (isLeaf[child])
            {
                for (int i = keyCount[child]; i > 0; i--) SetKey(child, i, Key(child, i - 1));
                SetKey(child, 0, Key(left, keyCount[left] - 1));
                keyCount[child]++;
                keyCount[left]--;
                SetKey(node, ci - 1, Key(child, 0));
            }
            else
            {
                for (int i = keyCount[child]; i > 0; i--) SetKey(child, i, Key(child, i - 1));
                for (int i = keyCount[child] + 1; i > 0; i--) SetChild(child, i, Child(child, i - 1));
                SetKey(child, 0, Key(node, ci - 1));                 // rotate the separator down
                SetChild(child, 0, Child(left, keyCount[left]));     // take the left sibling's last child
                SetKey(node, ci - 1, Key(left, keyCount[left] - 1)); // rotate the left sibling's last key up
                keyCount[child]++;
                keyCount[left]--;
            }
        }

        /// <summary>Move one element from the right sibling into the child at <paramref name="ci"/>.</summary>
        private void BorrowFromRight(int node, int ci)
        {
            var child = Child(node, ci);
            var right = Child(node, ci + 1);
            if (isLeaf[child])
            {
                SetKey(child, keyCount[child], Key(right, 0));
                keyCount[child]++;
                for (int i = 0; i < keyCount[right] - 1; i++) SetKey(right, i, Key(right, i + 1));
                keyCount[right]--;
                SetKey(node, ci, Key(right, 0));
                // Guards the case where the child was empty and thus its minimum just changed.
                if (ci > 0) SetKey(node, ci - 1, Key(child, 0));
            }
            else
            {
                SetKey(child, keyCount[child], Key(node, ci));                  // rotate the separator down
                SetChild(child, keyCount[child] + 1, Child(right, 0));          // take the right sibling's first child
                keyCount[child]++;
                SetKey(node, ci, Key(right, 0));                               // rotate the right sibling's first key up
                for (int i = 0; i < keyCount[right] - 1; i++) SetKey(right, i, Key(right, i + 1));
                for (int i = 0; i < keyCount[right]; i++) SetChild(right, i, Child(right, i + 1));
                keyCount[right]--;
            }
        }

        /// <summary>
        /// Merge the child at <paramref name="i"/> with its right sibling into a single node, dropping the separator
        /// Keys[i], and return the emptied right sibling to the pool.
        /// </summary>
        private void Merge(int node, int i)
        {
            var left = Child(node, i);
            var right = Child(node, i + 1);
            if (isLeaf[left])
            {
                for (int j = 0; j < keyCount[right]; j++) SetKey(left, keyCount[left] + j, Key(right, j));
                keyCount[left] += keyCount[right];
                next[left] = next[right];
            }
            else
            {
                int lc = keyCount[left];
                SetKey(left, lc, Key(node, i));                      // pull the separator down as a key
                SetChild(left, lc + 1, Child(right, 0));
                for (int j = 0; j < keyCount[right]; j++)
                {
                    SetKey(left, lc + 1 + j, Key(right, j));
                    SetChild(left, lc + 2 + j, Child(right, j + 1));
                }
                keyCount[left] = (byte)(lc + keyCount[right] + 1);
            }
            // Remove Keys[i] and Children[i+1] from the parent.
            for (int j = i; j < keyCount[node] - 1; j++) SetKey(node, j, Key(node, j + 1));
            for (int j = i + 1; j < keyCount[node]; j++) SetChild(node, j, Child(node, j + 1));
            keyCount[node]--;
            Return(right);
        }
        #endregion

        #region Lookup and traversal
        /// <summary>
        /// Test whether the tree contains a key.
        /// </summary>
        public bool Contains(uint key)
        {
            var n = root;
            while (!isLeaf[n]) n = Child(n, FindChildIndex(n, key));
            return SearchLeaf(n, key) >= 0;
        }

        /// <summary>
        /// Get the smallest key in the tree (the one whose value is least).  Returns false if the tree is empty.
        /// </summary>
        public bool TryGetMinimum(out uint key)
        {
            if (count == 0) { key = 0; return false; }
            key = MinKey(root);
            return true;
        }

        /// <summary>
        /// Get the largest key in the tree (the one whose value is greatest).  Returns false if the tree is empty.
        /// </summary>
        public bool TryGetMaximum(out uint key)
        {
            if (count == 0) { key = 0; return false; }
            var n = root;
            while (!isLeaf[n]) n = Child(n, keyCount[n]);
            key = Key(n, keyCount[n] - 1);
            return true;
        }

        /// <summary>Smallest key in the subtree rooted at <paramref name="node"/>.</summary>
        private uint MinKey(int node)
        {
            while (!isLeaf[node]) node = Child(node, 0);
            return Key(node, 0);
        }

        /// <summary>
        /// Enumerate every key in ascending order of its value.
        /// </summary>
        public IEnumerator<uint> GetEnumerator()
        {
            var leaf = LeftmostLeaf();
            while (leaf != NoNode)
            {
                int c = keyCount[leaf];
                for (int i = 0; i < c; i++) yield return Key(leaf, i);
                leaf = next[leaf];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Enumerate, in ascending order, every key whose value is strictly greater than <paramref name="value"/>.
        /// The bound is a <typeparamref name="T"/> value and need not be the value of any key actually in the tree.
        /// </summary>
        public IEnumerable<uint> GreaterThan(T value) => EnumerateFrom(LowerBound(value, inclusive: false));

        /// <summary>
        /// Enumerate, in ascending order, every key whose value is greater than or equal to <paramref name="value"/>.
        /// </summary>
        public IEnumerable<uint> GreaterThanOrEqual(T value) => EnumerateFrom(LowerBound(value, inclusive: true));

        /// <summary>
        /// Enumerate, in ascending order, every key whose value is strictly less than <paramref name="value"/>.
        /// </summary>
        public IEnumerable<uint> LessThan(T value)
        {
            var leaf = LeftmostLeaf();
            while (leaf != NoNode)
            {
                int c = keyCount[leaf];
                for (int i = 0; i < c; i++)
                {
                    var k = Key(leaf, i);
                    if (comparer.Compare(valueOf(k), value) >= 0) yield break;
                    yield return k;
                }
                leaf = next[leaf];
            }
        }

        /// <summary>
        /// Enumerate, in ascending order, every key whose value is less than or equal to <paramref name="value"/>.
        /// </summary>
        public IEnumerable<uint> LessThanOrEqual(T value)
        {
            var leaf = LeftmostLeaf();
            while (leaf != NoNode)
            {
                int c = keyCount[leaf];
                for (int i = 0; i < c; i++)
                {
                    var k = Key(leaf, i);
                    if (comparer.Compare(valueOf(k), value) > 0) yield break;
                    yield return k;
                }
                leaf = next[leaf];
            }
        }

        /// <summary>
        /// Locate the first position (leaf handle and index within it) holding a key whose value is at the lower bound
        /// for <paramref name="value"/>: the first key with value &gt;= <paramref name="value"/> when
        /// <paramref name="inclusive"/> is true, or the first key with value &gt; <paramref name="value"/> otherwise.
        /// If no such key exists the position is the end of the rightmost leaf.
        /// </summary>
        private (int leaf, int index) LowerBound(T value, bool inclusive)
        {
            var n = root;
            while (!isLeaf[n])
            {
                int lo = 0, hi = keyCount[n];
                while (lo < hi)
                {
                    var mid = (lo + hi) >> 1;
                    var c = comparer.Compare(valueOf(Key(n, mid)), value);
                    // Descend right of separator mid while it still sits strictly below the bound.
                    if (inclusive ? c < 0 : c <= 0) lo = mid + 1;
                    else hi = mid;
                }
                n = Child(n, lo);
            }
            {
                int lo = 0, hi = keyCount[n];
                while (lo < hi)
                {
                    var mid = (lo + hi) >> 1;
                    var c = comparer.Compare(valueOf(Key(n, mid)), value);
                    if (inclusive ? c < 0 : c <= 0) lo = mid + 1;
                    else hi = mid;
                }
                return (n, lo);
            }
        }

        /// <summary>
        /// Enumerate every key from the given position rightward to the end of the leaf chain, in ascending order.
        /// </summary>
        private IEnumerable<uint> EnumerateFrom((int leaf, int index) start)
        {
            var leaf = start.leaf;
            var i = start.index;
            while (leaf != NoNode)
            {
                int c = keyCount[leaf];
                for (; i < c; i++) yield return Key(leaf, i);
                leaf = next[leaf];
                i = 0;
            }
        }

        private int LeftmostLeaf()
        {
            var n = root;
            while (!isLeaf[n]) n = Child(n, 0);
            return n;
        }
        #endregion

        #region Maintenance
        /// <summary>
        /// Remove every key.  The whole arena is reused, so no allocation happens and the backing storage is retained.
        /// </summary>
        public void Clear()
        {
            allocated = 0;
            freeList = NoNode;
            count = 0;
            root = Rent(true);
        }
        #endregion

        #region Search primitives
        /// <summary>
        /// Binary search a leaf for <paramref name="key"/>.  Returns the index if found, otherwise the bitwise
        /// complement of the index at which it should be inserted.
        /// </summary>
        private int SearchLeaf(int node, uint key)
        {
            int lo = 0, hi = keyCount[node] - 1;
            while (lo <= hi)
            {
                var mid = (lo + hi) >> 1;
                var c = CompareKeys(key, Key(node, mid));
                if (c == 0) return mid;
                if (c < 0) hi = mid - 1;
                else lo = mid + 1;
            }
            return ~lo;
        }

        /// <summary>
        /// Return the index of the child of an internal node into which <paramref name="key"/> should descend.
        /// </summary>
        private int FindChildIndex(int node, uint key)
        {
            int lo = 0, hi = keyCount[node];
            while (lo < hi)
            {
                var mid = (lo + hi) >> 1;
                if (CompareKeys(key, Key(node, mid)) >= 0) lo = mid + 1;
                else hi = mid;
            }
            return lo;
        }
        #endregion
    }
}
