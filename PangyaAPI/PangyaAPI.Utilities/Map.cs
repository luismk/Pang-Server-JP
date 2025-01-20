using System.Collections.Generic;
using System.Linq;

namespace PangyaAPI.Utilities
{
    /// <summary>
    /// Extension to the normal Dictionary. This class can store more than one value for every key. It keeps a HashSet for every Key value.
    /// Calling Add with the same Key and multiple values will store each value under the same Key in the Dictionary. Obtaining the values
    /// for a Key will return the HashSet with the Values of the Key. 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class multimap<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="multimap&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        public multimap()
            : base()
        {
        }

        public bool ExistKey(TKey key)
        { return base.ContainsKey(key); }

        public void OrderBy()
        {
            var auto = this.OrderBy(c => c.Key).ToList();//retorna valores ordenados
            this.Clear();//limpo tudo primeiro
            foreach (var item in auto)//faco um laco para percorrer todos os valores adicionados
            {
                base.Add(item.Key, item.Value);//refaço o add com valores ja ordenados
            }
        }
        /// <summary>
        /// Adds the specified value under the specified key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value)
        {
            HashSet<TValue> container = null;
            if (ExistKey(key))
            {
                base[key].Add(value);
            }
            else
            {
                container = new HashSet<TValue>();
                this.Add(key, container);                
            }
            if (container!=null)//checa se esta null antes que eu possa add
                container.Add(value);
        }


        /// <summary>
        /// Adds the specified value under the specified key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>


        public void Insert(TKey key, TValue value)
        {
            GetValues(key).Add(value);
        }
        /// <summary>
        /// Determines whether this dictionary contains the specified value for the specified key 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the value is stored for the specified key in this dictionary, false otherwise</returns>
        public bool ContainsValue(TKey key, TValue value)
        {
            bool toReturn = false;
            HashSet<TValue> values = null;
            if (this.TryGetValue(key, out values))
            {
                toReturn = values.Contains(value);
            }
            return toReturn;
        }


        /// <summary>
        /// Removes the specified value for the specified key. It will leave the key in the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Remove(TKey key, TValue value)
        {
            HashSet<TValue> container = null;
            if (this.TryGetValue(key, out container))
            {
                container.Remove(value);
                if (container.Count <= 0)
                {
                    this.Remove(key);
                }
            }
        }


        /// <summary>
        /// Merges the specified multimap into this instance.
        /// </summary>
        /// <param name="toMergeWith">To merge with.</param>
        public void Merge(multimap<TKey, TValue> toMergeWith)
        {
            if (toMergeWith == null)
            {
                return;
            }

            foreach (KeyValuePair<TKey, HashSet<TValue>> pair in toMergeWith)
            {
                foreach (TValue value in pair.Value)
                {
                    this.Add(pair.Key, value);
                }
            }
        }


        /// <summary>
        /// Gets the values for the key specified. This method is useful if you want to avoid an exception for key value retrieval and you can't use TryGetValue
        /// (e.g. in lambdas)
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="returnEmptySet">if set to true and the key isn't found, an empty hashset is returned, otherwise, if the key isn't found, null is returned</param>
        /// <returns>
        /// This method will return null (or an empty set if returnEmptySet is true) if the key wasn't found, or
        /// the values if key was found.
        /// </returns>
        public HashSet<TValue> GetValues(TKey key, bool returnEmptySet = false)
        {
            HashSet<TValue> toReturn = null;
            if (!base.TryGetValue(key, out toReturn) && returnEmptySet)
            {
                toReturn = new HashSet<TValue>();
            }
            return toReturn;
        }
        public bool erase(TKey key) => this.Remove(key);
        public List<HashSet<TValue>> GetValues()
        {
            return Values.ToList();
        }



        public HashSet<TValue> Get(TKey key, bool returnEmptySet = false)
        {
            HashSet<TValue> toReturn = null;
            if (!base.TryGetValue(key, out toReturn) && returnEmptySet)
            {
                toReturn = new HashSet<TValue>();
            }
            return toReturn;
        }
        public HashSet<TValue> Find(TKey key, bool returnEmptySet = false)
        {
            HashSet<TValue> toReturn = null;
            if (!base.TryGetValue(key, out toReturn) && returnEmptySet)
            {
                toReturn = new HashSet<TValue>();
            }
            return toReturn;
        }

        public KeyValuePair<TKey, HashSet<TValue>> begin()
        {
            return this.First();
        }
        public TValue end()
        {
            return this.Last().Value.First();
        }
        public List<TValue> GetListValues()
        {
            var list = new List<TValue>(Count);

            foreach (var item in Values)
            {
                list.Add(item.First());
            }
            return list;
        }
        public List<TValue> SplitOne(int totalBySplit)
        {
            var splitList = GetListValues().Split(totalBySplit); //ChunkBy(this.ToList(), totalBySplit);
            var list = new List<TValue>(splitList.Count);
            foreach (var item in splitList)
            {
                list.Add(item.First());
            }
            return list.ToList();
        }

        public List<List<TValue>> SplitValues(int totalBySplit)
        {
            return GetListValues().Split(totalBySplit);
        }
    }
}

