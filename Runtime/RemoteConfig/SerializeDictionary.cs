using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace THEBADDEST.RemoteConfigSystem
{
    [Serializable]
    public struct KeyValue<TKey, TValue>
    {
        public TKey key;
        public TValue value;

        public KeyValue(TKey key,TValue value)
        {
            this.key   = key;
            this.value = value;
        }
    }

    [Serializable]
    public class SerializeDictionary<TKey, TValue,TBindType> : IDictionary<TKey, TValue> where TBindType : TValue
    {
        [SerializeField] private List<KeyValue<TKey, TBindType>> elements;
        private IDictionary<TKey, TValue> cacheDictionary;
        private bool isDirty => elements.Count != cacheDictionary.Count;

        private IDictionary<TKey, TValue> dictionary
        {
            get
            {
                if (isDirty)
                {
                    cacheDictionary.Clear();
                    for (int i = 0; i < elements.Count; i++)
                    {
                        cacheDictionary.Add(elements[i].key, elements[i].value);
                    }
                }

                return cacheDictionary;
            }
        }

        public SerializeDictionary()
        {
            cacheDictionary = new Dictionary<TKey, TValue>();
        }
        public SerializeDictionary(string bindType)
        {
            cacheDictionary = new Dictionary<TKey, TValue>();
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dictionary).GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            elements.Clear();
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            elements.Remove(new KeyValue<TKey, TBindType>(item.Key, (TBindType)item.Value));
            return dictionary.Remove(item);
        }

        public int Count => dictionary.Count;

        public bool IsReadOnly => dictionary.IsReadOnly;

        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key,value);
            elements.Add(new KeyValue<TKey, TBindType>(key,(TBindType)value));
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            elements.Remove(new KeyValue<TKey, TBindType>(key, (TBindType)dictionary[key]));
            return dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        public ICollection<TKey> Keys => dictionary.Keys;

        public ICollection<TValue> Values => dictionary.Values;
    }
}