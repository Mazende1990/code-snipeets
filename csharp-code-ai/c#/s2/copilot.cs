using System;
using System.Collections.Generic;
using Utilities.Exceptions;

namespace DataStructures.LinkedList.DoublyLinkedList
{
    /// <summary>
    /// Represents a doubly linked list.
    /// </summary>
    /// <typeparam name="T">Generic type.</typeparam>
    public class DoublyLinkedList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoublyLinkedList{T}"/> class with a single element.
        /// </summary>
        /// <param name="data">Data of the original head of the list.</param>
        public DoublyLinkedList(T data)
        {
            Head = new DoublyLinkedListNode<T>(data);
            Tail = Head;
            Count = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoublyLinkedList{T}"/> class from name="data">Enumerable of data to be stored in the list.</param>
        public DoublyLinkedList(IEnumerable<T> data)
        {
            foreach (var item in data)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the number of nodes in the list.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets or sets the first node of the list.
        /// </summary>
        private DoublyLinkedListNode<T>? Head { get; set; }

        /// <summary>
        /// Gets or sets the last node of the list.
        /// </summary>
        private DoublyLinkedListNode<T>? Tail { get; set; }

        /// <summary>
        /// Adds a new value at the beginning of the list.
        /// </summary>
        /// <param name="data">Value for the new head of the list.</param>
        /// <returns>The new head node.</returns>
        public DoublyLinkedListNode<T> AddHead(T data)
        {
            var node = new DoublyLinkedListNode<T>(data);
            if (Head == null)
            {
                Head = node;
                Tail = node;
                Count = 1;
                return node;
            }

            Head.Previous = node;
            node.Next = Head;
            Head = node;
            Count++;
            return node;
        }

        /// <summary>
        /// Adds a new value at the end of the list.
        /// </summary>
        /// <param name="data">New value to be added to the list.</param>
        /// <returns>The new node created based on the new value.</returns>
        public DoublyLinkedListNode<T> Add(T data)
        {
            if (Head == null)
            {
                return AddHead(data);
            }

            var node = new DoublyLinkedListNode<T>(data);
            Tail!.Next = node;
            node.Previous = Tail;
            Tail = node;
            Count++;
            return node;
        }

        /// <summary>
        /// Adds a new value after an existing node.
        /// </summary>
        /// <param name="data">New value to be added to the list.</param>
        /// <param name="existingNode">An existing node in the list.</param>
        /// <returns>The new node created based on the new value.</returns>
        public DoublyLinkedListNode<T> AddAfter(T data, DoublyLinkedListNode<T> existingNode)
        {
            if (existingNode == Tail)
            {
                return Add(data);
            }

            var node = new DoublyLinkedListNode<T>(data)
            {
                Next = existingNode.Next,
                Previous = existingNode
            };
            existingNode.Next = node;
            node.Next?.Previous = node;
            Count++;
            return node;
        }

        /// <summary>
        /// Gets an enumerable of the data in the list.
        /// </summary>
        /// <returns>The data in the list as an IEnumerable.</returns>
        public IEnumerable<T> GetData()
        {
            var current = Head;
            while (current != null)
            {
                yield return current.Data;
                current = current.Next;
            }
        }

        /// <summary>
        /// Gets an enumerable of the data in the list in reverse order.
        /// </summary>
        /// <returns>The data in the list as an IEnumerable.</returns>
        public IEnumerable<T> GetDataReversed()
        {
            var current = Tail;
            while (current != null)
            {
                yield return current.Data;
                current = current.Previous;
            }
        }

        /// <summary>
        /// Reverses the list.
        /// </summary>
        public void Reverse()
        {
            var current = Head;
            DoublyLinkedListNode<T>? temp = null;
            while (current != null)
            {
                temp = current.Previous;
                current.Previous = current.Next;
                current.Next = temp;
                current = current.Previous;
            }

            Tail = Head;
            if (temp != null)
            {
                Head = temp.Previous;
            }
        }

        /// <summary>
        /// Finds a node in the list that contains the specified value.
        /// </summary>
        /// <param name="data">Value to be searched for in the list.</param>
        /// <returns>The node containing the specified value.</returns>
        /// <exception cref="ItemNotFoundException">Thrown when the item is not found.</exception>
        public DoublyLinkedListNode<T> Find(T data)
        {
            var current = Head;
            while (current != null)
            {
                if ((current.Data == null && data == null) || (current.Data != null && current.Data.Equals(data)))
                {
                    return current;
                }
                current = current.Next;
            }
            throw new ItemNotFoundException();
        }

        /// <summary>
        /// Gets the node at the specified position.
        /// </summary>
        /// <param name="position">Position in the list.</param>
        /// <returns>The node at the specified position.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the position is out of range.</exception>
        public DoublyLinkedListNode<T> GetAt(int position)
        {
            if (position < 0 || position >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(position), $"Max count is {Count}");
            }

            var current = Head;
            for (var i = 0; i < position; i++)
            {
                current = current!.Next;
            }
            return current ?? throw new ArgumentOutOfRangeException(nameof(position), "Position must be an index in the list");
        }

        /// <summary>
        /// Removes the head node.
        /// </summary>
        public void RemoveHead()
        {
            if (Head == null)
            {
                throw new InvalidOperationException();
            }

            Head = Head.Next;
            if (Head == null)
            {
                Tail = null;
                Count = 0;
                return;
            }

            Head.Previous = null;
            Count--;
        }

        /// <summary>
        /// Removes the tail node.
        /// </summary>
        public void Remove()
        {
            if (Tail == null)
            {
                throw new InvalidOperationException("Cannot prune empty list");
            }

            Tail = Tail.Previous;
            if (Tail == null)
            {
                Head = null;
                Count = 0;
                return;
            }

            Tail.Next = null;
            Count--;
        }

        /// <summary>
        /// Removes a specific node.
        /// </summary>
        /// <param name="node">Node to be removed.</param>
        public void RemoveNode(DoublyLinkedListNode<T> node)
        {
            if (node == Head)
            {
                RemoveHead();
                return;
            }

            node.Previous.Next = node.Next;
            node.Next.Previous = node.Previous;
            Count--;
        }

        /// <summary>
        /// Removes a node that contains the specified data.
        /// </summary>
        /// <param name="data">Data to be removed from the list.</param>
        public void Remove(T data)
        {
            var node = Find(data);
            RemoveNode(node);
        }

        /// <summary>
        /// Gets the index of the node with the specified data.
        /// </summary>
        /// <param name="data">Data to look for.</param>
        /// <returns>The index of the node if found, or -1 if not found.</returns>
        public int IndexOf(T data)
        {
            var current = Head;
            var index = 0;
            while (current != null)
            {
                if ((current.Data == null && data == null) || (current.Data != null && current.Data.Equals(data)))
                {
                    return index;
                }
                index++;
                current = current.Next;
            }
            return -1;
        }

        /// <summary>
        /// Checks if the list contains a node with the specified data.
        /// </summary>
        /// <param name="data">Data to check for.</param>
        /// <returns>True if the node is found, otherwise false.</returns>
        public bool Contains(T data) => IndexOf(data) != -1;
    }
}