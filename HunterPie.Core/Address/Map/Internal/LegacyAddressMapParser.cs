﻿using System;
using System.Collections.Generic;
using System.IO;
using static HunterPie.Core.Address.Map.Internal.AddressMapKeyWords;

namespace HunterPie.Core.Address.Map.Internal
{
    internal class LegacyAddressMapParser : IAddressMapParser
    {

        private readonly Dictionary<Type, Dictionary<string, object>> items = new Dictionary<Type, Dictionary<string, object>>();

        public IReadOnlyDictionary<Type, Dictionary<string, object>> Items => items;

        public LegacyAddressMapParser(StreamReader stream)
        {
            Parse(stream);
        }

        public void Add<T>(string key, T value)
        {
            if (!items.ContainsKey(typeof(T)))
                items.Add(typeof(T), new Dictionary<string, object>());

            if (items[typeof(T)].ContainsKey(key))
                throw new Exception($"The key '{key}' already exists in '{typeof(T)}'");

            items[typeof(T)].Add(key, value);
        }

        public T Get<T>(string key)
        {
            if (!items.ContainsKey(typeof(T)))
                throw new KeyNotFoundException($"Map container '{typeof(T)}' not found.");

            if (!items[typeof(T)].ContainsKey(key))
                throw new KeyNotFoundException($"Map entry '{key}' not found in {typeof(T)}");

            if (items[typeof(T)][key] is not T)
                throw new InvalidCastException($"Map entry '{key}' is not of the type of {typeof(T)}");

            return (T)items[typeof(T)][key];
        }

        private void Parse(StreamReader stream)
        {
            
            while (!stream.EndOfStream)
            {
                int nextChar = stream.Peek();

                switch ((AddressMapTokens)nextChar)
                {
                    // # are used for comments
                    case AddressMapTokens.Comment:
                        AddressMapTokenizer.ConsumeUntilChar(stream, '\n');
                        AddressMapTokenizer.ConsumeTokens(stream, new char[] { '\n' });
                        break;

                    // Actual strings
                    default:
                        string value = AddressMapTokenizer.ConsumeUntilChars(stream, new char[] { '\n', ' ', '=' });

                        if (AddressMapKeyWords.IsKeyWord(value))
                        {
                            AddressMapType valueType = AddressMapKeyWords.GetType(value);
                            AddressMapTokenizer.ConsumeTokens(stream, new char[] { ' ', '=' });

                            string keyName = AddressMapTokenizer.ConsumeUntilChars(stream, new char[] { '\n', ' ', '=' });

                            AddressMapTokenizer.ConsumeTokens(stream, new char[] { ' ', '=' });

                            string keyValue = AddressMapTokenizer.ConsumeUntilChars(stream, new char[] { '#', '\n' });

                            this.AddValueByType(valueType, keyName, keyValue);

                            AddressMapTokenizer.ConsumeTokens(stream, new char[] { '\n', ' ' });
                        }

                        break;
                }

            }
        }
    }
}
