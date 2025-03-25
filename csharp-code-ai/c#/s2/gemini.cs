using System;
using System.Collections.Generic;
using Utilities.Exceptions;

namespace DataStructures.LinkedList.DoublyLinkedList;

/// <summary>
/// A doubly linked list where each node references both the next and previous nodes.
/// Enables efficient forward and backward traversal.
/// </summary>
/// <typeparam name="T">The type of data stored in the list.</typeparam>
public class DoublyLinkedList<T>
{
    /// <summary>
    /// Gets the number of nodes in the list.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets or sets the first node in the list.
    /// </summary>
    private DoublyLinkedListNode<T>? Head { get; set; }

    /// <summary>
    /// Gets or sets the last node in the list.
    /// </summary>
    private DoublyLinkedListNode<T>? Tail { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DoublyLinkedList{T}"/> class with a single data element.
    /// </summary>
    /// <param name="data">The data for the initial head node.</param>
    public DoublyLinkedList(T data)
    {
        Head = new DoublyLinkedListNode<T>(data);
        Tail = Head;
        Count = 1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DoublyLinkedList{T}"/> class from an enumerable collection.
    /// </summary>
    /// <param name="data">The enumerable collection of data to add to the list.</param>
    public DoublyLinkedList(IEnumerable<T> data)
    {
        foreach (var item in data)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Adds a new node with the specified data to the beginning of the list.
    /// </summary>
    /// <param name="data">The data for the new head node.</param>
    /// <returns>The newly created head node.</returns>
    public DoublyLinkedListNode<T> AddHead(T data)
    {
        var newNode = new DoublyLinkedListNode<T>(data);

        if (Head is null)
        {
            Head = newNode;
            Tail = newNode;
        }
        else
        {
            newNode.Next = Head;
            Head.Previous = newNode;
            Head = newNode;
        }

        Count++;
        return newNode;
    }

    /// <summary>
    /// Adds a new node with the specified data to the end of the list.
    /// </summary>
    /// <param name="data">The data for the new node.</param>
    /// <returns>The newly created tail node.</returns>
    public DoublyLinkedListNode<T> Add(T data)
    {
        if (Head is null)
        {
            return AddHead(data);
        }

        var newNode = new DoublyLinkedListNode<T>(data);
        Tail!.Next = newNode;
        newNode.Previous = Tail;
        Tail = newNode;
        Count++;
        return newNode;
    }

    /// <summary>
    /// Adds a new node with the specified data after an existing node.
    /// </summary>
    /// <param name="data">The data for the new node.</param>
    /// <param name="existingNode">The existing node to insert after.</param>
    /// <returns>The newly created node.</returns>
    public DoublyLinkedListNode<T> AddAfter(T data, DoublyLinkedListNode<T> existingNode)
    {
        if (existingNode == Tail)
        {
            return Add(data);
        }

        var newNode = new DoublyLinkedListNode<T>(data);
        newNode.Next = existingNode.Next;
        newNode.Previous = existingNode;
        existingNode.Next = newNode;
        if (newNode.Next is not null)
        {
            newNode.Next.Previous = newNode;
        }
        Count++;
        return newNode;
    }

    /// <summary>
    /// Returns an enumerable collection of the data in the list, in forward order.
    /// </summary>
    /// <returns>An enumerable collection of the list's data.</returns>
    public IEnumerable<T> GetData()
    {
        var current = Head;
        while (current is not null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }

    /// <summary>
    /// Returns an enumerable collection of the data in the list, in reverse order.
    /// </summary>
    /// <returns>An enumerable collection of the list's data in reverse order.</returns>
    public IEnumerable<T> GetDataReversed()
    {
        var current = Tail;
        while (current is not null)
        {
            yield return current.Data;
            current = current.Previous;
        }
    }

    /// <summary>
    /// Reverses the order of the nodes in the list.
    /// </summary>
    public void Reverse()
    {
        var current = Head;
        DoublyLinkedListNode<T>? temp = null;

        while (current is not null)
        {
            temp = current.Previous;
            current.Previous = current.Next;
            current.Next = temp;
            current = current.Previous;
        }

        Tail = Head;
        if (temp is not null)
        {
            Head = temp.Previous;
        }
    }

    /// <summary>
    /// Finds the first node with the specified data.
    /// </summary>
    /// <param name="data">The data to find.</param>
    /// <returns>The found node, or throws ItemNotFoundException if not found.</returns>
    /// <exception cref="ItemNotFoundException">Thrown when the item is not found.</exception>
    public DoublyLinkedListNode<T> Find(T data)
    {
        var current = Head;
        while (current is not null)
        {
            if (current.Data is null && data is null || current.Data is not null && current.Data.Equals(data))
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
    /// <param name="position">The index of the node to retrieve.</param>
    /// <returns>The node at the specified position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the position is invalid.</exception>
    public DoublyLinkedListNode<T> GetAt(int position)
    {
        if (position < 0 || position >= Count)
        {
            throw new ArgumentOutOfRangeException(<span class="math-inline">"Max count is \{Count\}"\);
\}
var current \= Head;
for \(var i \= 0; i < position; i\+\+\)
\{
current \= current\!\.Next;
\}
return current ?? throw new ArgumentOutOfRangeException\(</span>"{nameof(position)} must be an index in the list");
    }

    /// <summary>
    /// Removes the head node from the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
    public void RemoveHead()
    {
        if (Head is null)
        {
            throw new InvalidOperationException("Cannot remove from an empty list.");
        }

        Head = Head.Next;
        if (Head is null)
        {
            Tail = null;
        }
        else
        {
            Head.Previous = null;
        }

        Count--;
    }

    /// <summary>
    /// Removes the tail node from the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
    public void Remove()
    {
        if (Tail is null)
        {
            throw new InvalidOperationException("Cannot remove from an empty list.");
        }

        Tail = Tail.Previous;
        if (Tail is null)
        {
            Head = null;
        }
        else
        {
            Tail.Next = null;
        }

        Count--;
    }

    /// <summary>
    /// Removes the specified node from the list.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <exception cref="ArgumentException">Thrown when the node is invalid.</exception>
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
            return
          }

        if (node.Previous is null || node.Next is null)
        {
            throw new ArgumentException($"{nameof(node)} cannot have Previous or Next null if it's an internal node.");
        }

        node.Previous.Next = node.Next;
        node.Next.Previous = node.Previous;
        Count--;
    }

    /// <summary>
    /// Removes the first node with the specified data.
    /// </summary>
    /// <param name="data">The data to remove.</param>
    public void Remove(T data)
    {
        var node = Find(data);
        RemoveNode(node);
    }

    /// <summary>
    /// Finds the index of the first node with the specified data.
    /// </summary>
    /// <param name="data">The data to find.</param>
    /// <returns>The index of the found node, or -1 if not found.</returns>
    public int IndexOf(T data)
    {
        var current = Head;
        var index = 0;

        while (current is not null)
        {
            if (current.Data is null && data is null || current.Data is not null && current.Data.Equals(data))
            {
                return index;
            }

            index++;
            current = current.Next;
        }

        return -1;
    }

    /// <summary>
    /// Determines whether the list contains a node with the specified data.
    /// </summary>
    /// <param name="data">The data to search for.</param>
    /// <returns>True if the list contains a node with the specified data; otherwise, false.</returns>
    public bool Contains(T data) => IndexOf(data) != -1;
}