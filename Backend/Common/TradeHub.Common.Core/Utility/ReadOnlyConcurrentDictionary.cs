/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TradeHub.Common.Core.Utility
{
    
    public class ReadOnlyConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        // Instance of the actual Dictionary
        private ConcurrentDictionary<TKey, TValue> _concurrentDictionary;

        /// <summary>
        /// Default Constructor - To create a new ReadOnly ConcurrentDictionary
        /// </summary>
        public ReadOnlyConcurrentDictionary()
        {
            _concurrentDictionary = new ConcurrentDictionary<TKey, TValue>();
        }

        /// <summary>
        /// Argument Constructor - To create a ReadOnly ConcurrentDictionary from an existing ConcurrentDictionary
        /// </summary>
        public ReadOnlyConcurrentDictionary(ConcurrentDictionary<TKey, TValue> concurrentDictionary)
        {
            _concurrentDictionary = concurrentDictionary;
        }

        #region IDictionary<TKey,TValue> Members

        /// <summary>
        /// Overrides the "Add" method and doesn't alow to Add New values
        /// </summary>
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw ReadOnlyException();
        }

        /// <summary>
        /// Checks existance of the requested KEY
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return _concurrentDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Returns All the existing KEYS
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return _concurrentDictionary.Keys; }
        }

        /// <summary>
        /// Overrides "Remove" method and doesn't allow to Remove values
        /// </summary>
        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw ReadOnlyException();
        }

        /// <summary>
        /// Returns the VALUE for the requested KEY
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _concurrentDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns all the existing VALUES
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return _concurrentDictionary.Values; }
        }

        /// <summary>
        /// Returns the VALUE for the specified KEY
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                return _concurrentDictionary[key];
            }
        }

        /// <summary>
        /// Returns the VALUE for the specified KEY, but doesn't allows to change its VALUE
        /// </summary>
        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key];
            }
            set
            {
                throw ReadOnlyException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Doesn't allow to "ADD" new KeyValuePair
        /// </summary>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw ReadOnlyException();
        }

        /// <summary>
        /// Doesn't allow to "Clear" the dictionary values
        /// </summary>
        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw ReadOnlyException();
        }

        /// <summary>
        /// Checks whether the specified item exists or not
        /// </summary>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _concurrentDictionary.Contains(item);
        }

        /// <summary>
        /// Copies the local values to the provided array
        /// </summary>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            // Cast local Concurrent Dictionary to IDictionary to get the "CopyTo" method
            IDictionary<TKey, TValue> tempDictionary = new Dictionary<TKey, TValue>(_concurrentDictionary);
            tempDictionary.CopyTo(array,arrayIndex);
        }

        /// <summary>
        /// Number of existing values
        /// </summary>
        public int Count
        {
            get { return _concurrentDictionary.Count; }
        }

        /// <summary>
        /// Tells if the dictionary is Readonly
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Overrides the "Remove" and doesn't allow to Remove values
        /// </summary>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw ReadOnlyException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Returns the IEnumerator
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _concurrentDictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns the IEnumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private static Exception ReadOnlyException()
        {
            return new NotSupportedException("This dictionary is read-only");
        }
    }
}
