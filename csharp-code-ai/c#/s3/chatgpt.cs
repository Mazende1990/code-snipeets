using System;
using System.Collections.Generic;

namespace DataStructures.BinarySearchTree
{
    public class BinarySearchTree<TKey>
    {
        private readonly Comparer<TKey> comparer;
        private BinarySearchTreeNode<TKey>? root;

        public BinarySearchTree()
            : this(Comparer<TKey>.Default) { }

        public BinarySearchTree(Comparer<TKey> customComparer)
        {
            root = null;
            Count = 0;
            comparer = customComparer;
        }

        public int Count { get; private set; }

        public void Add(TKey key)
        {
            if (root == null)
            {
                root = new BinarySearchTreeNode<TKey>(key);
            }
            else
            {
                InsertNode(root, key);
            }

            Count++;
        }

        public void AddRange(IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
                Add(key);
        }

        public bool Contains(TKey key) => FindNode(root, key) != null;

        public BinarySearchTreeNode<TKey>? Search(TKey key) => FindNode(root, key);

        public bool Remove(TKey key)
        {
            if (root == null)
                return false;

            bool removed = RemoveNode(null, root, key);

            if (removed)
                Count--;

            return removed;
        }

        public BinarySearchTreeNode<TKey>? GetMin() => root == null ? null : GetMinNode(root);

        public BinarySearchTreeNode<TKey>? GetMax() => root == null ? null : GetMaxNode(root);

        public ICollection<TKey> GetKeysInOrder() => TraverseInOrder(root);

        public ICollection<TKey> GetKeysPreOrder() => TraversePreOrder(root);

        public ICollection<TKey> GetKeysPostOrder() => TraversePostOrder(root);

        // --- Private Helper Methods ---

        private void InsertNode(BinarySearchTreeNode<TKey> current, TKey key)
        {
            int cmp = comparer.Compare(key, current.Key);

            if (cmp < 0)
            {
                if (current.Left != null)
                {
                    InsertNode(current.Left, key);
                }
                else
                {
                    current.Left = new BinarySearchTreeNode<TKey>(key);
                }
            }
            else if (cmp > 0)
            {
                if (current.Right != null)
                {
                    InsertNode(current.Right, key);
                }
                else
                {
                    current.Right = new BinarySearchTreeNode<TKey>(key);
                }
            }
            else
            {
                throw new ArgumentException($"Key \"{key}\" already exists in the tree.");
            }
        }

        private BinarySearchTreeNode<TKey>? FindNode(BinarySearchTreeNode<TKey>? current, TKey key)
        {
            if (current == null)
                return null;

            int cmp = comparer.Compare(key, current.Key);

            if (cmp < 0) return FindNode(current.Left, key);
            if (cmp > 0) return FindNode(current.Right, key);

            return current;
        }

        private bool RemoveNode(BinarySearchTreeNode<TKey>? parent, BinarySearchTreeNode<TKey>? current, TKey key)
        {
            if (current == null)
                return false;

            int cmp = comparer.Compare(key, current.Key);

            if (cmp < 0)
                return RemoveNode(current, current.Left, key);
            if (cmp > 0)
                return RemoveNode(current, current.Right, key);

            // Node with the matching key found
            BinarySearchTreeNode<TKey>? replacement;

            if (current.Left == null || current.Right == null)
            {
                replacement = current.Left ?? current.Right;
            }
            else
            {
                var predecessor = GetMaxNode(current.Left);
                RemoveNode(root, root, predecessor.Key);
                replacement = new BinarySearchTreeNode<TKey>(predecessor.Key)
                {
                    Left = current.Left,
                    Right = current.Right
                };
            }

            if (current == root)
            {
                root = replacement;
            }
            else if (parent!.Left == current)
            {
                parent.Left = replacement;
            }
            else
            {
                parent.Right = replacement;
            }

            return true;
        }

        private BinarySearchTreeNode<TKey> GetMinNode(BinarySearchTreeNode<TKey> node)
            => node.Left == null ? node : GetMinNode(node.Left);

        private BinarySearchTreeNode<TKey> GetMaxNode(BinarySearchTreeNode<TKey> node)
            => node.Right == null ? node : GetMaxNode(node.Right);

        private IList<TKey> TraverseInOrder(BinarySearchTreeNode<TKey>? node)
        {
            var keys = new List<TKey>();
            if (node != null)
            {
                keys.AddRange(TraverseInOrder(node.Left));
                keys.Add(node.Key);
                keys.AddRange(TraverseInOrder(node.Right));
            }
            return keys;
        }

        private IList<TKey> TraversePreOrder(BinarySearchTreeNode<TKey>? node)
        {
            var keys = new List<TKey>();
            if (node != null)
            {
                keys.Add(node.Key);
                keys.AddRange(TraversePreOrder(node.Left));
                keys.AddRange(TraversePreOrder(node.Right));
            }
            return keys;
        }

        private IList<TKey> TraversePostOrder(BinarySearchTreeNode<TKey>? node)
        {
            var keys = new List<TKey>();
            if (node != null)
            {
                keys.AddRange(TraversePostOrder(node.Left));
                keys.AddRange(TraversePostOrder(node.Right));
                keys.Add(node.Key);
            }
            return keys;
        }
    }
}
