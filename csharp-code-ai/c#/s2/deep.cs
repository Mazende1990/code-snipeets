using System;
using System.Collections.Generic;
using Utilities.Exceptions;

namespace DataStructures.LinkedList.DoublyLinkedList;

/// <summary>
/// A doubly linked list implementation where each node contains a reference to both the next and previous nodes.
/// This allows traversal in both directions.
/// </summary>
/// <typeparam name="T">The type of elements stored in the list.</typeparam>
public class DoublyLinkedList<T>
{
    private DoublyLinkedListNode<T>? _head;
    private DoublyLinkedListNode<T>? _tail;
    
    /// <summary>
    /// Gets the number of nodes in the list.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Initializes a new instance with a single element.
    /// </summary>
    /// <param name="data">The data for the initial node.</param>
    public DoublyLinkedList(T data)
    {
        _head = new DoublyLinkedListNode<T>(data);
        _tail = _head;
        Count = 1;
    }

    /// <summary>
    /// Initializes a new instance from a collection of elements.
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
    /// Adds a new node with the specified data at the beginning of the list.
    /// </summary>
    /// <param name="data">The data for the new node.</param>
    /// <returns>The newly created node.</returns>
    public DoublyLinkedListNode<T> AddHead(T data)
    {
        var newNode = new DoublyLinkedListNode<T>(data);

        if (IsEmpty())
        {
            InitializeListWithNewNode(newNode);
            return newNode;
        }

        InsertNodeBeforeHead(newNode);
        return newNode;
    }

    /// <summary>
    /// Adds a new node with the specified data at the end of the list.
    /// </summary>
    /// <param name="data">The data for the new node.</param>
    /// <returns>The newly created node.</returns>
    public DoublyLinkedListNode<T> Add(T data)
    {
        if (IsEmpty())
        {
            return AddHead(data);
        }

        var newNode = new DoublyLinkedListNode<T>(data);
        AppendNodeToTail(newNode);
        return newNode;
    }

    /// <summary>
    /// Adds a new node with the specified data after the specified existing node.
    /// </summary>
    /// <param name="data">The data for the new node.</param>
    /// <param name="existingNode">The node after which to insert the new node.</param>
    /// <returns>The newly created node.</returns>
    public DoublyLinkedListNode<T> AddAfter(T data, DoublyLinkedListNode<T> existingNode)
    {
        if (existingNode == _tail)
        {
            return Add(data);
        }

        var newNode = new DoublyLinkedListNode<T>(data);
        InsertNodeAfterExistingNode(newNode, existingNode);
        return newNode;
    }

    /// <summary>
    /// Returns an enumerable that iterates through the list from head to tail.
    /// </summary>
    public IEnumerable<T> GetData()
    {
        var current = _head;
        while (current != null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }

    /// <summary>
    /// Returns an enumerable that iterates through the list from tail to head.
    /// </summary>
    public IEnumerable<T> GetDataReversed()
    {
        var current = _tail;
        while (current != null)
        {
            yield return current.Data;
            current = current.Previous;
        }
    }

    /// <summary>
    /// Reverses the order of nodes in the list.
    /// </summary>
    public void Reverse()
    {
        var current = _head;
        DoublyLinkedListNode<T>? temp = null;

        while (current != null)
        {
            temp = current.Previous;
            current.Previous = current.Next;
            current.Next = temp;
            current = current.Previous;
        }

        _tail = _head;

        if (temp != null)
        {
            _head = temp.Previous;
        }
    }

    /// <summary>
    /// Finds the first node containing the specified data.
    /// </summary>
    /// <param name="data">The data to search for.</param>
    /// <returns>The node containing the data.</returns>
    /// <exception cref="ItemNotFoundException">Thrown when the data is not found.</exception>
    public DoublyLinkedListNode<T> Find(T data)
    {
        var current = _head;
        while (current != null)
        {
            if (DataEquals(current.Data, data))
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
    /// <param name="position">The zero-based index of the node to get.</param>
    /// <returns>The node at the specified position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when position is invalid.</exception>
    public DoublyLinkedListNode<T> GetAt(int position)
    {
        ValidatePosition(position);

        var current = _head;
        for (var i = 0; i < position; i++)
        {
            current = current!.Next;
        }

        return current ?? throw new ArgumentOutOfRangeException(nameof(position), "Position must be within list bounds");
    }

    /// <summary>
    /// Removes the first node from the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
    public void RemoveHead()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException("Cannot remove head from empty list");
        }

        _head = _head!.Next;
        
        if (_head == null)
        {
            ClearList();
        }
        else
        {
            _head.Previous = null;
            Count--;
        }
    }

    /// <summary>
    /// Removes the last node from the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
    public void Remove()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException("Cannot remove from empty list");
        }

        _tail = _tail!.Previous;
        
        if (_tail == null)
        {
            ClearList();
        }
        else
        {
            _tail.Next = null;
            Count--;
        }
    }

    /// <summary>
    /// Removes the specified node from the list.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    public void RemoveNode(DoublyLinkedListNode<T> node)
    {
        if (node == _head)
        {
            RemoveHead();
            return;
        }

        if (node == _tail)
        {
            Remove();
            return;
        }

        ValidateInternalNode(node);
        RemoveInternalNode(node);
    }

    /// <summary>
    /// Removes the first node containing the specified data.
    /// </summary>
    /// <param name="data">The data of the node to remove.</param>
    public void Remove(T data)
    {
        var node = Find(data);
        RemoveNode(node);
    }

    /// <summary>
    /// Returns the zero-based index of the first occurrence of the specified data.
    /// </summary>
    /// <param name="data">The data to locate in the list.</param>
    /// <returns>The index of the data, or -1 if not found.</returns>
    public int IndexOf(T data)
    {
        var current = _head;
        var index = 0;
        
        while (current != null)
        {
            if (DataEquals(current.Data, data))
            {
                return index;
            }
            
            index++;
            current = current.Next;
        }
        
        return -1;
    }

    /// <summary>
    /// Determines whether the list contains the specified data.
    /// </summary>
    /// <param name="data">The data to locate in the list.</param>
    public bool Contains(T data) => IndexOf(data) != -1;

    #region Helper Methods

    private bool IsEmpty() => _head == null;

    private static bool DataEquals(T? a, T? b) => 
        (a == null && b == null) || (a != null && a.Equals(b));

    private void InitializeListWithNewNode(DoublyLinkedListNode<T> node)
    {
        _head = node;
        _tail = node;
        Count = 1;
    }

    private void InsertNodeBeforeHead(DoublyLinkedListNode<T> node)
    {
        node.Next = _head;
        _head!.Previous = node;
        _head = node;
        Count++;
    }

    private void AppendNodeToTail(DoublyLinkedListNode<T> node)
    {
        _tail!.Next = node;
        node.Previous = _tail;
        _tail = node;
        Count++;
    }

    private void InsertNodeAfterExistingNode(DoublyLinkedListNode<T> newNode, DoublyLinkedListNode<T> existingNode)
    {
        newNode.Next = existingNode.Next;
        newNode.Previous = existingNode;
        existingNode.Next = newNode;

        if (newNode.Next != null)
        {
            newNode.Next.Previous = newNode;
        }

        Count++;
    }

    private void ValidatePosition(int position)
    {
        if (position < 0 || position >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(position), $"Position must be between 0 and {Count - 1}");
        }
    }

    private void ClearList()
    {
        _head = null;
        _tail = null;
        Count = 0;
    }

    private static void ValidateInternalNode(DoublyLinkedListNode<T> node)
    {
        if (node.Previous == null || node.Next == null)
        {
            throw new ArgumentException("Internal node must have non-null Previous and Next references", nameof(node));
        }
    }

    private void RemoveInternalNode(DoublyLinkedListNode<T> node)
    {
        node.Previous!.Next = node.Next;
        node.Next!.Previous = node.Previous;
        Count--;
    }

    #endregion
}