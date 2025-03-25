using System;
using System.Collections.Generic;

namespace DataStructures.BinarySearchTree
{
    /// <summary>
    /// An ordered tree with efficient insertion, removal, and lookup.
    /// </summary>
    /// <remarks>
    /// A Binary Search Tree (BST) is a tree that satisfies:
    /// 1. All nodes have two children (Left and Right)
    /// 2. Left subtree contains only keys less than node's key
    /// 3. Right subtree contains only keys greater than node's key
    /// Average complexity for operations is O(log n).
    /// </remarks>
    /// <typeparam name="TKey">Type of key which must implement IComparable</typeparam>
    public class BinarySearchTree<TKey>
    {
        private readonly Comparer<TKey> comparer;
        private BinarySearchTreeNode<TKey>? root;

        public BinarySearchTree() : this(Comparer<TKey>.Default) { }

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
                AddNode(root, key);
            }
            Count++;
        }

        public void AddRange(IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
            {
                Add(key);
            }
        }

        public BinarySearchTreeNode<TKey>? Search(TKey key) => SearchNode(root, key);

        public bool Contains(TKey key) => SearchNode(root, key) != null;

        public bool Remove(TKey key)
        {
            if (root == null) return false;

            bool removed = RemoveNode(root, root, key);
            if (removed) Count--;
            return removed;
        }

        public BinarySearchTreeNode<TKey>? GetMin() => root == null ? null : GetMinNode(root);

        public BinarySearchTreeNode<TKey>? GetMax() => root == null ? null : GetMaxNode(root);

        public ICollection<TKey> GetKeysInOrder() => TraverseInOrder(root);
        public ICollection<TKey> GetKeysPreOrder() => TraversePreOrder(root);
        public ICollection<TKey> GetKeysPostOrder() => TraversePostOrder(root);

        private void AddNode(BinarySearchTreeNode<TKey> node, TKey key)
        {
            int comparison = comparer.Compare(node.Key, key);
            
            if (comparison > 0)
            {
                if (node.Left != null)
                {
                    AddNode(node.Left, key);
                }
                else
                {
                    node.Left = new BinarySearchTreeNode<TKey>(key);
                }
            }
            else if (comparison < 0)
            {
                if (node.Right != null)
                {
                    AddNode(node.Right, key);
                }
                else
                {
                    node.Right = new BinarySearchTreeNode<TKey>(key);
                }
            }
            else
            {
                throw new ArgumentException($"Key \"{key}\" already exists in tree");
            }
        }

        private bool RemoveNode(BinarySearchTreeNode<TKey>? parent, BinarySearchTreeNode<TKey>? node, TKey key)
        {
            if (node == null || parent == null) return false;

            int comparison = comparer.Compare(node.Key, key);

            if (comparison > 0) return RemoveNode(node, node.Left, key);
            if (comparison < 0) return RemoveNode(node, node.Right, key);

            // Found node to remove
            BinarySearchTreeNode<TKey>? replacement = GetReplacementNode(node);

            // Update references
            if (node == root)
            {
                root = replacement;
            }
            else if (parent.Left == node)
            {
                parent.Left = replacement;
            }
            else
            {
                parent.Right = replacement;
            }

            return true;
        }

        private BinarySearchTreeNode<TKey>? GetReplacementNode(BinarySearchTreeNode<TKey> node)
        {
            // Case 0/1: No children or one child
            if (node.Left == null || node.Right == null)
            {
                return node.Left ?? node.Right;
            }

            // Case 2: Two children - use in-order predecessor
            var predecessor = GetMaxNode(node.Left);
            RemoveNode(root!, root!, predecessor.Key);
            return new BinarySearchTreeNode<TKey>(predecessor.Key)
            {
                Left = node.Left,
                Right = node.Right
            };
        }

        private BinarySearchTreeNode<TKey> GetMaxNode(BinarySearchTreeNode<TKey> node)
        {
            return node.Right == null ? node : GetMaxNode(node.Right);
        }

        private BinarySearchTreeNode<TKey> GetMinNode(BinarySearchTreeNode<TKey> node)
        {
            return node.Left == null ? node : GetMinNode(node.Left);
        }

        private IList<TKey> TraverseInOrder(BinarySearchTreeNode<TKey>? node)
        {
            if (node == null) return new List<TKey>();

            var result = new List<TKey>();
            result.AddRange(TraverseInOrder(node.Left));
            result.Add(node.Key);
            result.AddRange(TraverseInOrder(node.Right));
            return result;
        }

        private IList<TKey> TraversePreOrder(BinarySearchTreeNode<TKey>? node)
        {
            if (node == null) return new List<TKey>();

            var result = new List<TKey> { node.Key };
            result.AddRange(TraversePreOrder(node.Left));
            result.AddRange(TraversePreOrder(node.Right));
            return result;
        }

        private IList<TKey> TraversePostOrder(BinarySearchTreeNode<TKey>? node)
        {
            if (node == null) return new List<TKey>();

            var result = new List<TKey>();
            result.AddRange(TraversePostOrder(node.Left));
            result.AddRange(TraversePostOrder(node.Right));
            result.Add(node.Key);
            return result;
        }

        private BinarySearchTreeNode<TKey>? SearchNode(BinarySearchTreeNode<TKey>? node, TKey key)
        {
            if (node == null) return null;

            int comparison = comparer.Compare(node.Key, key);
            
            if (comparison > 0) return SearchNode(node.Left, key);
            if (comparison < 0) return SearchNode(node.Right, key);
            
            return node;
        }
    }
}