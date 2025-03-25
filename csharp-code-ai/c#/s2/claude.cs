using System;
using System.Collections.Generic;
using Utilities.Exceptions;

namespace DataStructures.LinkedList.DoublyLinkedList;

/// <summary>
/// A generic doubly linked list implementation that allows bidirectional traversal and manipulation.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public class DoublyLinkedList<T>
{
    /// <summary>
    /// Gets the number of elements in the list.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets the first node in the list.
    /// </summary>
    private DoublyLinkedListNode<T>? Head { get; set; }

    /// <summary>
    /// Gets the last node in the list.
    /// </summary>
    private DoublyLinkedListNode<T>? Tail { get; set; }

    /// <summary>
    /// Initializes a new empty doubly linked list.
    /// </summary>
    public DoublyLinkedList() { }

    /// <summary>
    /// Initializes a new doubly linked list with an initial element.
    /// </summary>
    /// <param name="data">The initial data to add to the list.</param>
    public DoublyLinkedList(T data)
    {
        InitializeFirstNode(data);
    }

    /// <summary>
    /// Initializes a new doubly linked list from an enumerable collection.
    /// </summary>
    /// <param name="data">The collection of elements to add to the list.</param>
    public DoublyLinkedList(IEnumerable<T> data)
    {
        foreach (var item in data)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Creates the first node in an empty list.
    /// </summary>
    /// <param name="data">The data for the first node.</param>
    private void InitializeFirstNode(T data)
    {
        Head = new DoublyLinkedListNode<T>(data);
        Tail = Head;
        Count = 1;
    }

    /// <summary>
    /// Adds a new element to the beginning of the list.
    /// </summary>
    /// <param name="data">The data to add.</param>
    /// <returns>The newly created node.</returns>
    public DoublyLinkedListNode<T> AddHead(T data)
    {
        if (Head is null)
        {
            return InitializeFirstNode(data);
        }

        var newNode = new DoublyLinkedListNode<T>(data)
        {
            Next = Head
        };
        Head.Previous = newNode;
        Head = newNode;
        Count++;
        return newNode;
    }

    /// <summary>
    /// Adds a new element to the end of the list.
    /// </summary>
    /// <param name="data">The data to add.</param>
    /// <returns>The newly created node.</returns>
    public DoublyLinkedListNode<T> Add(T data)
    {
        if (Head is null)
        {
            return InitializeFirstNode(data);
        }

        var newNode = new DoublyLinkedListNode<T>(data)
        {
            Previous = Tail
        };
        Tail!.Next = newNode;
        Tail = newNode;
        Count++;
        return newNode;
    }

    /// <summary>
    /// Adds a new element after a specified existing node.
    /// </summary>
    /// <param name="data">The data to add.</param>
    /// <param name="existingNode">The node after which to insert the new element.</param>
    /// <returns>The newly created node.</returns>
    public DoublyLinkedListNode<T> AddAfter(T data, DoublyLinkedListNode<T> existingNode)
    {
        if (existingNode == Tail)
        {
            return Add(data);
        }

        var newNode = new DoublyLinkedListNode<T>(data)
        {
            Next = existingNode.Next,
            Previous = existingNode
        };

        existingNode.Next!.Previous = newNode;
        existingNode.Next = newNode;
        Count++;
        return newNode;
    }

    /// <summary>
    /// Retrieves all elements in the list in forward order.
    /// </summary>
    /// <returns>An enumerable of list elements.</returns>
    public IEnumerable<T> GetData()
    {
        for (var current = Head; current is not null; current = current.Next)
        {
            yield return current.Data;
        }
    }

    /// <summary>
    /// Retrieves all elements in the list in reverse order.
    /// </summary>
    /// <returns>An enumerable of list elements in reverse.</returns>
    public IEnumerable<T> GetDataReversed()
    {
        for (var current = Tail; current is not null; current = current.Previous)
        {
            yield return current.Data;
        }
    }

    /// <summary>
    /// Reverses the order of elements in the list.
    /// </summary>
    public void Reverse()
    {
        if (Head is null) return;

        var current = Head;
        Tail = current;

        while (current is not null)
        {
            var temp = current.Previous;
            current.Previous = current.Next;
            current.Next = temp;
            current = current.Previous;
        }

        Head = current?.Previous ?? Tail;
    }

    /// <summary>
    /// Finds the first node containing the specified data.
    /// </summary>
    /// <param name="data">The data to search for.</param>
    /// <returns>The first node containing the data.</returns>
    /// <exception cref="ItemNotFoundException">Thrown when the item is not found.</exception>
    public DoublyLinkedListNode<T> Find(T data)
    {
        for (var current = Head; current is not null; current = current.Next)
        {
            if (IsDataEqual(current.Data, data))
            {
                return current;
            }
        }

        throw new ItemNotFoundException();
    }

    /// <summary>
    /// Retrieves the node at the specified position.
    /// </summary>
    /// <param name="position">The zero-based index of the node.</param>
    /// <returns>The node at the specified position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the position is invalid.</exception>
    public DoublyLinkedListNode<T> GetAt(int position)
    {
        if (position < 0 || position >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(position), $"Position must be between 0 and {Count - 1}");
        }

        var current = Head;
        for (int i = 0; i < position; i++)
        {
            current = current!.Next;
        }

        return current!;
    }

    /// <summary>
    /// Removes the first node from the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
    public void RemoveHead()
    {
        if (Head is null)
        {
            throw new InvalidOperationException("Cannot remove from an empty list");
        }

        Head = Head.Next;
        if (Head is null)
        {
            Tail = null;
            Count = 0;
            return;
        }

        Head.Previous = null;
        Count--;
    }

    /// <summary>
    /// Removes the last node from the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
    public void Remove()
    {
        if (Tail is null)
        {
            throw new InvalidOperationException("Cannot remove from an empty list");
        }

        Tail = Tail.Previous;
        if (Tail is null)
        {
            Head = null;
            Count = 0;
            return;
        }

        Tail.Next = null;
        Count--;
    }

    /// <summary>
    /// Removes a specific node from the list.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    public void RemoveNode(DoublyLinkedListNode<T> node)
    {
        if (node == Head)
        {
            RemoveHead();
            return;
        }

        if (node == Tail)
        {
            Remove();
            return;
        }

        node.Previous!.Next = node.Next;
        node.Next!.Previous = node.Previous;
        Count--;
    }

    /// <summary>
    /// Removes the first node containing the specified data.
    /// </summary>
    /// <param name="data">The data to remove.</param>
    public void Remove(T data)
    {
        var node = Find(data);
        RemoveNode(node);
    }

    /// <summary>
    /// Finds the index of the first node containing the specified data.
    /// </summary>
    /// <param name="data">The data to search for.</param>
    /// <returns>The zero-based index of the first occurrence, or -1 if not found.</returns>
    public int IndexOf(T data)
    {
        int index = 0;
        for (var current = Head; current is not null; current = current.Next)
        {
            if (IsDataEqual(current.Data, data))
            {
                return index;
            }
            index++;
        }

        return -1;
    }

    /// <summary>
    /// Checks if the list contains a node with the specified data.
    /// </summary>
    /// <param name="data">The data to search for.</param>
    /// <returns>True if the data is found, otherwise false.</returns>
    public bool Contains(T data) => IndexOf(data) != -1;

    /// <summary>
    /// Compares two values for equality, handling null cases.
    /// </summary>
    /// <param name="a">First value to compare.</param>
    /// <param name="b">Second value to compare.</param>
    /// <returns>True if values are equal, considering null cases.</returns>
    private bool IsDataEqual(T? a, T? b)
    {
        return a is null && b is null || a is not null && a.Equals(b);
    }

    /// <summary>
    /// Initializes a new node with the given data and returns it.
    /// </summary>
    /// <param name="data">The data to use for the node.</param>
    /// <returns>The newly created node.</returns>
    private DoublyLinkedListNode<T> InitializeFirstNode(T data)
    {
        Head = new DoublyLinkedListNode<T>(data);
        Tail = Head;
        Count = 1;
        return Head;
    }
}