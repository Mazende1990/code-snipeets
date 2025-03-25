using System;
using System.Collections.Generic;

namespace DataStructures.BinarySearchTree
{
    /// <summary>
    /// An ordered tree with efficient insertion, removal, and lookup.
    /// </summary>
    /// <remarks>
    /// A Binary Search Tree (BST) is a tree that satisfies the following properties:
    /// <list type="bullet">
    /// <item>All nodes in the tree contain two children, usually called Left and Right.</item>
    /// <item>All nodes in the Left subtree contain keys that are less than the node's key.</item>
    /// <item>All nodes in the Right subtree contain keys that are greater than the node's key.</item>
    /// </list>
    /// A BST will have an average complexity of O(log n) for insertion, removal, and lookup operations.
    /// </remarks>
    /// <typeparam name="TKey">Type of key for the BST. Keys must implement IComparable.</typeparam>
    public class BinarySearchTree<TKey>
    {
        private readonly Comparer<TKey> _comparer;
        private BinarySearchTreeNode<TKey>? _root;

        public BinarySearchTree() : this(Comparer<TKey>.Default) { }

        public BinarySearchTree(Comparer<TKey> comparer)
        {
            _root = null;
            Count = 0;
            _comparer = comparer;
        }

        public int Count { get; private set; }

        public void Add(TKey key)
        {
            if (_root == null)
            {
                _root = new BinarySearchTreeNode<TKey>(key);
            }
            else
            {
                AddRecursive(_root, key);
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

        public BinarySearchTreeNode<TKey>? Search(TKey key) => SearchRecursive(_root, key);

        public bool Contains(TKey key) => Search(_root, key) != null;

        public bool Remove(TKey key)
        {
            if (_root == null)
            {
                return false;
            }

            var result = RemoveRecursive(_root, _root, key);
            if (result)
            {
                Count--;
            }
            return result;
        }

        public BinarySearchTreeNode<TKey>? GetMin() => _root == null ? null : GetMinRecursive(_root);

        public BinarySearchTreeNode<TKey>? GetMax() => _root == null ? null : GetMaxRecursive(_root);

        public ICollection<TKey> GetKeysInOrder() => GetKeysInOrderRecursive(_root);

        public ICollection<TKey> GetKeysPreOrder() => GetKeysPreOrderRecursive(_root);

        public ICollection<TKey> GetKeysPostOrder() => GetKeysPostOrderRecursive(_root);

        private void AddRecursive(BinarySearchTreeNode<TKey> node, TKey key)
        {
            var comparison = _comparer.Compare(node.Key, key);

            if (comparison > 0)
            {
                if (node.Left != null)
                {
                    AddRecursive(node.Left, key);
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
                    AddRecursive(node.Right, key);
                }
                else
                {
                    node.Right = new BinarySearchTreeNode<TKey>(key);
                }
            }
            else
            {
                throw new ArgumentException($"Key \"{key}\" already exists in tree!");
            }
        }

        private bool RemoveRecursive(BinarySearchTreeNode<TKey>? parent, BinarySearchTreeNode<TKey>? node, TKey key)
        {
            if (node == null || parent == null)
            {
                return false;
            }

            var comparison = _comparer.Compare(node.Key, key);

            if (comparison > 0)
            {
                return RemoveRecursive(node, node.Left, key);
            }

            if (comparison < 0)
            {
                return RemoveRecursive(node, node.Right, key);
            }

            BinarySearchTreeNode<TKey>? replacement;

            if (node.Left == null || node.Right == null)
            {
                replacement = node.Left ?? node.Right;
            }
            else
            {
                var predecessor = GetMaxRecursive(node.Left);
                RemoveRecursive(_root, _root, predecessor.Key);
                replacement = new BinarySearchTreeNode<TKey>(predecessor.Key)
                {
                    Left = node.Left,
                    Right = node.Right,
                };
            }

            if (node == _root)
            {
                _root = replacement;
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

        private BinarySearchTreeNode<TKey> GetMaxRecursive(BinarySearchTreeNode<TKey> node) =>
            node.Right == null ? node : GetMaxRecursive(node.Right);

        private BinarySearchTreeNode<TKey> GetMinRecursive(BinarySearchTreeNode<TKey> node) =>
            node.Left == null ? node : GetMinRecursive(node.Left);

        private IList<TKey> GetKeysInOrderRecursive(BinarySearchTreeNode<TKey>? node)
        {
            if (node == null)
            {
                return new List<TKey>();
            }

            var result = new List<TKey>();
            result.AddRange(GetKeysInOrderRecursive(node.Left));
            result.Add(node.Key);
            result.AddRange(GetKeysInOrderRecursive(node.Right));
            return result;
        }

        private IList<TKey> GetKeysPreOrderRecursive(BinarySearchTreeNode<TKey>? node)
        {
            if (node == null)
            {
                return new List<TKey>();
            }

            var result = new List<TKey>();
            result.Add(node.Key);
            result.AddRange(GetKeysPreOrderRecursive(node.Left));
            result.AddRange(GetKeysPreOrderRecursive(node.Right));
            return result;
        }

        private IList<TKey> GetKeysPostOrderRecursive(BinarySearchTreeNode<TKey>? node)
        {
            if (node == null)
            {
                return new List<TKey>();
            }

            var result = new List<TKey>();
            result.AddRange(GetKeysPostOrderRecursive(node.Left));
            result.AddRange(GetKeysPostOrderRecursive(node.Right));
            result.Add(node.Key);
            return result;
        }

        private BinarySearchTreeNode<TKey>? SearchRecursive(BinarySearchTreeNode<TKey>? node, TKey key)
        {
            if (node == null)
            {
                return null;
            }

            var comparison = _comparer.Compare(node.Key, key);

            if (comparison > 0)
            {
                return SearchRecursive(node.Left, key);
            }

            if (comparison < 0)
            {
                return SearchRecursive(node.Right, key);
            }

            return node;
        }
    }
}