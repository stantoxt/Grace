﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grace.Data.Immutable
{

    /// <summary>
    /// Immutable HashTree implementation http://en.wikipedia.org/wiki/AVL_tree
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class ImmutableHashTree<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Empty hashtree, used as the starting point 
        /// </summary>
        public static readonly ImmutableHashTree<TKey, TValue> Empty = new ImmutableHashTree<TKey, TValue>();

        /// <summary>
        /// Hash value for this node
        /// </summary>
        public readonly int Hash;

        /// <summary>
        /// Height of hashtree node
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// Key value for this hash node
        /// </summary>
        public readonly TKey Key;

        /// <summary>
        /// Value for this hash node
        /// </summary>
        public readonly TValue Value;

        /// <summary>
        /// Keys with the same hashcode
        /// </summary>
        public readonly ImmutableArray<KeyValuePair<TKey, TValue>> Conflicts;

        /// <summary>
        /// Left node of the hash tree
        /// </summary>
        public readonly ImmutableHashTree<TKey, TValue> Left;

        /// <summary>
        /// Right node of the hash tree
        /// </summary>
        public readonly ImmutableHashTree<TKey, TValue> Right;

        /// <summary>
        /// Update delegate defines behavior when key already exists
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public delegate TValue UpdateDelegate(TValue currentValue, TValue newValue);

        /// <summary>
        /// Provide an action that will be called for each node in the hash tree
        /// </summary>
        /// <param name="iterateAction"></param>
        public void IterateInOrder(Action<TKey, TValue> iterateAction)
        {
            if (Left.Height > 0)
            {
                Left.IterateInOrder(iterateAction);
            }

            iterateAction(Key, Value);

            if (Right.Height > 0)
            {
                Right.IterateInOrder(iterateAction);
            }
        }

        /// <summary>
        /// Return an enumerable of KVP
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TKey, TValue>> IterateInOrder()
        {
            ImmutableHashTree<TKey, TValue>[] nodes = new ImmutableHashTree<TKey, TValue>[Height];
            int nodeCount = 0;
            ImmutableHashTree<TKey, TValue> currentNode = this;

            while (!currentNode.IsEmpty || nodeCount != 0)
            {
                if (!currentNode.IsEmpty)
                {
                    nodes[nodeCount++] = currentNode;

                    currentNode = currentNode.Left;
                }
                else
                {
                    currentNode = nodes[--nodeCount];

                    yield return new KeyValuePair<TKey, TValue>(currentNode.Key, currentNode.Value);

                    if (currentNode.Conflicts.Count > 0)
                    {
                        for (int i = 0; i < currentNode.Conflicts.Count; i++)
                        {
                            yield return currentNode.Conflicts[i];
                        }
                    }

                    currentNode = currentNode.Right;
                }
            }
        }

        /// <summary>
        /// Adds a new entry to the hashtree
        /// </summary>
        /// <param name="key">key to add</param>
        /// <param name="value">value to add</param>
        /// <param name="updateDelegate">update delegate, by default will throw key already exits exception</param>
        /// <returns></returns>
        public ImmutableHashTree<TKey, TValue> Add(TKey key, TValue value, UpdateDelegate updateDelegate = null)
        {
            if (updateDelegate == null)
            {
                updateDelegate = KeyAlreadyExists;
            }

            return InternalAdd(key.GetHashCode(), key, value, updateDelegate);
        }

        /// <summary>
        /// Checks to see if a key is contained in the hashtable
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            TValue value;

            return TryGetValue(key, out value);
        }

        /// <summary>
        /// Try get value from hashtree
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (Height == 0)
            {
                value = default (TValue);

                return false;
            }

            ImmutableHashTree<TKey, TValue> currenNode = this;
            int keyHash = key.GetHashCode();

            while (currenNode.Hash != keyHash && currenNode.Height != 0)
            {
                currenNode = keyHash < currenNode.Hash ? currenNode.Left : currenNode.Right;
            }

            if (currenNode.Height != 0)
            {
                if (key.Equals(currenNode.Key))
                {
                    value = currenNode.Value;

                    return true;
                }

                for (int i = 0; i < currenNode.Conflicts.Count; i++)
                {
                    KeyValuePair<TKey, TValue> kvp = currenNode.Conflicts[i];

                    if (key.Equals(currenNode.Conflicts[i].Key))
                    {
                        value = kvp.Value;

                        return true;
                    }
                }

            }

            value = default(TValue);

            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;

                if (!TryGetValue(key, out value))
                {
                    throw new KeyNotFoundException(string.Format("Key {0} was not found",key));
                }

                return value;
            }
        }

        /// <summary>
        /// Returns all the keys in the hashtree
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get { return this.Select(x => x.Key); }
        }

        /// <summary>
        /// returns all the values in the hashtree
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get { return this.Select(x => x.Value); }
        }

        /// <summary>
        /// Is the hash tree empty
        /// </summary>
        public bool IsEmpty
        {
            get { return Height == 0; }
        }

        private ImmutableHashTree()
        {
            Conflicts = ImmutableArray<KeyValuePair<TKey, TValue>>.Empty;
        }

        private ImmutableHashTree(int hash,
                                  TKey key,
                                  TValue value,
                                  ImmutableArray<KeyValuePair<TKey, TValue>> conflicts,
                                  ImmutableHashTree<TKey, TValue> left,
                                  ImmutableHashTree<TKey, TValue> right)
        {
            Hash = hash;
            Key = key;
            Value = value;

            Conflicts = conflicts;

            Left = left;
            Right = right;

            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private ImmutableHashTree<TKey, TValue> InternalAdd(int hashCode, TKey key, TValue value, UpdateDelegate updateDelegate)
        {
            if (Height == 0)
            {
                return new ImmutableHashTree<TKey, TValue>(hashCode,
                                                           key,
                                                           value,
                                                           ImmutableArray<KeyValuePair<TKey, TValue>>.Empty,
                                                           Empty,
                                                           Empty);
            }

            if (hashCode == Hash)
            {
                return ResolveConflicts(key, value, updateDelegate);
            }

            return hashCode < Hash ?
                   New(Left.InternalAdd(hashCode, key, value, updateDelegate), Right).EnsureBalanced() :
                   New(Left, Right.InternalAdd(hashCode, key, value, updateDelegate)).EnsureBalanced();
        }

        private ImmutableHashTree<TKey, TValue> EnsureBalanced()
        {
            int heightDeleta = Left.Height - Right.Height;

            if (heightDeleta > 2)
            {
                ImmutableHashTree<TKey, TValue> newLeft = Left;

                if (Left.Right.Height - Left.Left.Height == 1)
                {
                    newLeft = Left.RotateLeft();
                }

                return New(newLeft, Right).RotateRight();
            }

            if (heightDeleta < -2)
            {
                ImmutableHashTree<TKey, TValue> newRight = Right;

                if (Right.Left.Height - Right.Right.Height == 1)
                {
                    newRight = Right.RotateRight();
                }

                return New(Left, newRight).RotateLeft();
            }

            return this;
        }

        private ImmutableHashTree<TKey, TValue> RotateRight()
        {
            return Left.New(Left.Left, New(Left.Right, Right));
        }

        private ImmutableHashTree<TKey, TValue> RotateLeft()
        {
            return Right.New(New(Left, Right.Left), Right.Right);
        }

        private ImmutableHashTree<TKey, TValue> ResolveConflicts(TKey key, TValue value, UpdateDelegate updateDelegate)
        {
            if (ReferenceEquals(Key, key) || Key.Equals(key))
            {
                TValue newValue = updateDelegate(Value, value);

                return new ImmutableHashTree<TKey, TValue>(Hash, key, newValue, Conflicts, Left, Right);
            }

            return new ImmutableHashTree<TKey, TValue>(Hash, Key, Value, Conflicts.Add(new KeyValuePair<TKey, TValue>(key, value)), Left, Right);
        }

        private ImmutableHashTree<TKey, TValue> New(ImmutableHashTree<TKey, TValue> left,
                                                    ImmutableHashTree<TKey, TValue> right)
        {
            return new ImmutableHashTree<TKey, TValue>(Hash, Key, Value, Conflicts, left, right);
        }

        private static TValue KeyAlreadyExists(TValue currentValue, TValue newValue)
        {
            throw new KeyExistsException<TKey>();
        }

        /// <summary>
        /// Gets an enumerator for the immutable hash
        /// </summary>
        /// <returns>enumerator</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return IterateInOrder().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the count of the immutable hashtree. Note its faster to do a lookup than to do a count
        /// If you want to test for emptyness use the IsEmpty property
        /// </summary>
        public int Count
        {
            get { return Height == 0 ? 0 : this.Count(); }
        }
    }
}
