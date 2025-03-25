using System;
using System.Collections.Generic;
using Utilities.Exceptions;

namespace DataStructures.LinkedList.DoublyLinkedList
{
    /// <summary>
    /// Represents a doubly linked list where each node has a reference to both the next and previous node.
    /// </summary>
    /// <typeparam name="T">Type of data stored in the list.</typeparam>
    public class DoublyLinkedList<T>
    {
        private DoublyLinkedListNode<T>? head;
        private DoublyLinkedListNode<T>? tail;

        public int Count { get; private set; }

        public DoublyLinkedList(T data)
        {
            var node = new DoublyLinkedListNode<T>(data);
            head = tail = node;
            Count = 1;
        }

        public DoublyLinkedList(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                Add(item);
        }

        public DoublyLinkedListNode<T> AddHead(T data)
        {
            var newNode = new DoublyLinkedListNode<T>(data);

            if (head == null)
            {
                head = tail = newNode;
            }
            else
            {
                newNode.Next = head;
                head.Previous = newNode;
                head = newNode;
            }

            Count++;
            return newNode;
        }

        public DoublyLinkedListNode<T> Add(T data)
        {
            if (head == null)
                return AddHead(data);

            var newNode = new DoublyLinkedListNode<T>(data);
            tail!.Next = newNode;
            newNode.Previous = tail;
            tail = newNode;

            Count++;
            return newNode;
        }

        public DoublyLinkedListNode<T> AddAfter(T data, DoublyLinkedListNode<T> existingNode)
        {
            if (existingNode == tail)
                return Add(data);

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

        public IEnumerable<T> GetData()
        {
            var current = head;
            while (current != null)
            {
                yield return current.Data;
                current = current.Next;
            }
        }

        public IEnumerable<T> GetDataReversed()
        {
            var current = tail;
            while (current != null)
            {
                yield return current.Data;
                current = current.Previous;
            }
        }

        public void Reverse()
        {
            var current = head;
            DoublyLinkedListNode<T>? temp = null;

            while (current != null)
            {
                temp = current.Previous;
                current.Previous = current.Next;
                current.Next = temp;
                current = current.Previous;
            }

            tail = head;
            if (temp != null)
                head = temp.Previous;
        }

        public DoublyLinkedListNode<T> Find(T data)
        {
            var current = head;

            while (current != null)
            {
                if (Equals(current.Data, data))
                    return current;

                current = current.Next;
            }

            throw new ItemNotFoundException();
        }

        public DoublyLinkedListNode<T> GetAt(int position)
        {
            if (position < 0 || position >= Count)
                throw new ArgumentOutOfRangeException(nameof(position), $"Max count is {Count}");

            var current = head;
            for (int i = 0; i < position; i++)
                current = current!.Next;

            return current!;
        }

        public void RemoveHead()
        {
            if (head == null)
                throw new InvalidOperationException();

            head = head.Next;

            if (head == null)
            {
                tail = null;
                Count = 0;
            }
            else
            {
                head.Previous = null;
                Count--;
            }
        }

        public void Remove()
        {
            if (tail == null)
                throw new InvalidOperationException("Cannot remove from an empty list");

            tail = tail.Previous;

            if (tail == null)
            {
                head = null;
                Count = 0;
            }
            else
            {
                tail.Next = null;
                Count--;
            }
        }

        public void RemoveNode(DoublyLinkedListNode<T> node)
        {
            if (node == head)
            {
                RemoveHead();
            }
            else if (node == tail)
            {
                Remove();
            }
            else
            {
                if (node.Previous == null || node.Next == null)
                    throw new ArgumentException("Node must be internally connected (not head or tail)");

                node.Previous.Next = node.Next;
                node.Next.Previous = node.Previous;
                Count--;
            }
        }

        public void Remove(T data)
        {
            var node = Find(data);
            RemoveNode(node);
        }

        public int IndexOf(T data)
        {
            var current = head;
            int index = 0;

            while (current != null)
            {
                if (Equals(current.Data, data))
                    return index;

                current = current.Next;
                index++;
            }

            return -1;
        }

        public bool Contains(T data) => IndexOf(data) != -1;
    }
}
