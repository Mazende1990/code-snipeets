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
            if (root is null)
            {
                root = new BinarySearchTreeNode<TKey>(key);
            }
            else
            {
                Add(root, key);
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

        public BinarySearchTreeNode<TKey>? Search(TKey key) => Search(root, key);

        public bool Contains(TKey key) => Search(root, key) is not null;

        public bool Remove(TKey key)
        {
            if (root is null) return false;

            var result = Remove(root, root, key);
            if (result) Count--;

            return result;
        }

        public BinarySearchTreeNode<TKey>? GetMin() => root is null ? default : GetMin(root);

        public BinarySearchTreeNode<TKey>? GetMax() => root is null ? default : GetMax(root);

        public ICollection<TKey> GetKeysInOrder() => GetKeysInOrder(root);

        public ICollection<TKey> GetKeysPreOrder() => GetKeysPreOrder(root);

        public ICollection<TKey> GetKeysPostOrder() => GetKeysPostOrder(root);

        private void Add(BinarySearchTreeNode<TKey> node, TKey key)
        {
            var compareResult = comparer.Compare(node.Key, key);
            if (compareResult > 0)
            {
                if (node.Left is not null)
                {
                    Add(node.Left, key);
                }
                else
                {
                    node.Left = new BinarySearchTreeNode<TKey>(key);
                }
            }
            else if (compareResult < 0)
            {
                if (node.Right is not null)
                {
                    Add(node.Right, key);
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

        private bool Remove(BinarySearchTreeNode<TKey>? parent, BinarySearchTreeNode<TKey>? node, TKey key)
        {
            if (node is null || parent is null) return false;

            var compareResult = comparer.Compare(node.Key, key);
            if (compareResult > 0) return Remove(node, node.Left, key);
            if (compareResult < 0) return Remove(node, node.Right, key);

            BinarySearchTreeNode<TKey>? replacementNode;
            if (node.Left is null || node.Right is null)
            {
                replacementNode = node.Left ?? node.Right;
            }
            else
            {
                var predecessorNode = GetMax(node.Left);
                Remove(root, root, predecessorNode.Key);
                replacementNode = new BinarySearchTreeNode<TKey>(predecessorNode.Key)
                {
                    Left = node.Left,
                    Right = node.Right,
                };
            }

            if (node == root)
            {
                root = replacementNode;
            }
            else if (parent.Left == node)
            {
                parent.Left = replacementNode;
            }
            else
            {
                parent.Right = replacementNode;
            }

            return true;
        }

        private BinarySearchTreeNode<TKey> GetMax(BinarySearchTreeNode<TKey> node)
        {
            return node.Right is null ? node : GetMax(node.Right);
        }

        private BinarySearchTreeNode<TKey> GetMin(BinarySearchTreeNode<TKey> node)
        {
            return node.Left is null ? node : GetMin(node.Left);
        }

        private IList<TKey> GetKeysInOrder(BinarySearchTreeNode<TKey>? node)
        {
            if (node is null) return new List<TKey>();

            var result = new List<TKey>();
            result.AddRange(GetKeysInOrder(node.Left));
            result.Add(node.Key);
            result.AddRange(GetKeysInOrder(node.Right));
            return result;
        }

        private IList<TKey> GetKeysPreOrder(BinarySearchTreeNode<TKey>? node)
        {
            if (node is null) return new List<TKey>();

            var result = new List<TKey>();
            result.Add(node.Key);
            result.AddRange(GetKeysPreOrder(node.Left));
            result.AddRange(GetKeysPreOrder(node.Right));
            return result;
        }

        private IList<TKey> GetKeysPostOrder(BinarySearchTreeNode<TKey>? node)
        {
            if (node is null) return new List<TKey>();

            var result = new List<TKey>();
            result.AddRange(GetKeysPostOrder(node.Left));
            result.AddRange(GetKeysPostOrder(node.Right));
            result.Add(node.Key);
            return result;
        }

        private BinarySearchTreeNode<TKey>? Search(BinarySearchTreeNode<TKey>? node, TKey key)
        {
            if (node is null) return default;

            var compareResult = comparer.Compare(node.Key, key);
            if (compareResult > 0) return Search(node.Left, key);
            if (compareResult < 0) return Search(node.Right, key);

            return node;
        }
    }
}