using System;
using System.Collections.Generic;

namespace DataStructures.BinarySearchTree
{
    /// <summary>
    /// Represents a Binary Search Tree (BST) data structure with efficient insertion, removal, and lookup operations.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the tree, which must be comparable.</typeparam>
    public class BinarySearchTree<TKey>
    {
        /// <summary>
        /// The root node of the Binary Search Tree.
        /// </summary>
        private BinarySearchTreeNode<TKey>? _root;

        /// <summary>
        /// The comparer used for key comparisons.
        /// </summary>
        private readonly IComparer<TKey> _comparer;

        /// <summary>
        /// Initializes a new instance of the Binary Search Tree with default comparison.
        /// </summary>
        public BinarySearchTree() : this(Comparer<TKey>.Default) { }

        /// <summary>
        /// Initializes a new instance of the Binary Search Tree with a custom comparer.
        /// </summary>
        /// <param name="comparer">The custom comparer to use for key comparisons.</param>
        public BinarySearchTree(IComparer<TKey> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            Count = 0;
        }

        /// <summary>
        /// Gets the number of nodes in the Binary Search Tree.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Adds a single key to the Binary Search Tree.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <exception cref="ArgumentException">Thrown if the key already exists in the tree.</exception>
        public void Add(TKey key)
        {
            _root = AddRecursive(_root, key);
            Count++;
        }

        /// <summary>
        /// Adds multiple keys to the Binary Search Tree.
        /// </summary>
        /// <param name="keys">The sequence of keys to add.</param>
        public void AddRange(IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
            {
                Add(key);
            }
        }

        /// <summary>
        /// Searches for a node with the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>The node with the specified key, or null if not found.</returns>
        public BinarySearchTreeNode<TKey>? Search(TKey key) => SearchRecursive(_root, key);

        /// <summary>
        /// Checks if the tree contains a specific key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key exists, false otherwise.</returns>
        public bool Contains(TKey key) => Search(key) is not null;

        /// <summary>
        /// Removes a key from the Binary Search Tree.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the key was successfully removed, false otherwise.</returns>
        public bool Remove(TKey key)
        {
            var originalCount = Count;
            _root = RemoveRecursive(_root, key);
            return Count < originalCount;
        }

        /// <summary>
        /// Retrieves the node with the minimum key value.
        /// </summary>
        /// <returns>The node with the minimum key, or null if the tree is empty.</returns>
        public BinarySearchTreeNode<TKey>? GetMin() => _root is null ? null : FindMinNode(_root);

        /// <summary>
        /// Retrieves the node with the maximum key value.
        /// </summary>
        /// <returns>The node with the maximum key, or null if the tree is empty.</returns>
        public BinarySearchTreeNode<TKey>? GetMax() => _root is null ? null : FindMaxNode(_root);

        /// <summary>
        /// Retrieves all keys in the tree using in-order traversal.
        /// </summary>
        /// <returns>A collection of keys in sorted order.</returns>
        public ICollection<TKey> GetKeysInOrder() => TraverseInOrder(_root);

        /// <summary>
        /// Retrieves all keys in the tree using pre-order traversal.
        /// </summary>
        /// <returns>A collection of keys in pre-order.</returns>
        public ICollection<TKey> GetKeysPreOrder() => TraversePreOrder(_root);

        /// <summary>
        /// Retrieves all keys in the tree using post-order traversal.
        /// </summary>
        /// <returns>A collection of keys in post-order.</returns>
        public ICollection<TKey> GetKeysPostOrder() => TraversePostOrder(_root);

        /// <summary>
        /// Recursively adds a key to the tree.
        /// </summary>
        private BinarySearchTreeNode<TKey> AddRecursive(BinarySearchTreeNode<TKey>? node, TKey key)
        {
            if (node is null)
            {
                return new BinarySearchTreeNode<TKey>(key);
            }

            int comparisonResult = _comparer.Compare(key, node.Key);

            if (comparisonResult < 0)
            {
                node.Left = AddRecursive(node.Left, key);
            }
            else if (comparisonResult > 0)
            {
                node.Right = AddRecursive(node.Right, key);
            }
            else
            {
                throw new ArgumentException($"Key '{key}' already exists in the tree.");
            }

            return node;
        }

        /// <summary>
        /// Recursively removes a key from the tree.
        /// </summary>
        private BinarySearchTreeNode<TKey>? RemoveRecursive(BinarySearchTreeNode<TKey>? node, TKey key)
        {
            if (node is null)
            {
                return null;
            }

            int comparisonResult = _comparer.Compare(key, node.Key);

            if (comparisonResult < 0)
            {
                node.Left = RemoveRecursive(node.Left, key);
            }
            else if (comparisonResult > 0)
            {
                node.Right = RemoveRecursive(node.Right, key);
            }
            else
            {
                // Handle removal based on number of children
                if (node.Left is null)
                {
                    Count--;
                    return node.Right;
                }
                if (node.Right is null)
                {
                    Count--;
                    return node.Left;
                }

                // Two children: replace with in-order predecessor
                var predecessorNode = FindMaxNode(node.Left);
                node.Key = predecessorNode.Key;
                node.Left = RemoveRecursive(node.Left, predecessorNode.Key);
            }

            return node;
        }

        /// <summary>
        /// Finds the node with the maximum key in a given subtree.
        /// </summary>
        private BinarySearchTreeNode<TKey> FindMaxNode(BinarySearchTreeNode<TKey> node)
        {
            return node.Right is null ? node : FindMaxNode(node.Right);
        }

        /// <summary>
        /// Finds the node with the minimum key in a given subtree.
        /// </summary>
        private BinarySearchTreeNode<TKey> FindMinNode(BinarySearchTreeNode<TKey> node)
        {
            return node.Left is null ? node : FindMinNode(node.Left);
        }

        /// <summary>
        /// Recursively searches for a node with a specific key.
        /// </summary>
        private BinarySearchTreeNode<TKey>? SearchRecursive(BinarySearchTreeNode<TKey>? node, TKey key)
        {
            if (node is null)
            {
                return null;
            }

            int comparisonResult = _comparer.Compare(key, node.Key);

            return comparisonResult switch
            {
                < 0 => SearchRecursive(node.Left, key),
                > 0 => SearchRecursive(node.Right, key),
                _ => node
            };
        }

        /// <summary>
        /// Performs an in-order traversal of the tree.
        /// </summary>
        private IList<TKey> TraverseInOrder(BinarySearchTreeNode<TKey>? node)
        {
            if (node is null)
            {
                return new List<TKey>();
            }

            var result = new List<TKey>();
            result.AddRange(TraverseInOrder(node.Left));
            result.Add(node.Key);
            result.AddRange(TraverseInOrder(node.Right));
            return result;
        }

        /// <summary>
        /// Performs a pre-order traversal of the tree.
        /// </summary>
        private IList<TKey> TraversePreOrder(BinarySearchTreeNode<TKey>? node)
        {
            if (node is null)
            {
                return new List<TKey>();
            }

            var result = new List<TKey>();
            result.Add(node.Key);
            result.AddRange(TraversePreOrder(node.Left));
            result.AddRange(TraversePreOrder(node.Right));
            return result;
        }

        /// <summary>
        /// Performs a post-order traversal of the tree.
        /// </summary>
        private IList<TKey> TraversePostOrder(BinarySearchTreeNode<TKey>? node)
        {
            if (node is null)
            {
                return new List<TKey>();
            }

            var result = new List<TKey>();
            result.AddRange(TraversePostOrder(node.Left));
            result.AddRange(TraversePostOrder(node.Right));
            result.Add(node.Key);
            return result;
        }
    }
}