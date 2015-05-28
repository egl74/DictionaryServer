using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageNamespace;
using Newtonsoft.Json;

namespace DictionaryServer
{
    internal static class MessageHandler
    {
        private static Dictionary<string, string> dictionary = new Dictionary<string, string>();
        private static readonly string path = Environment.CurrentDirectory + "\\dictionary.json";

        internal static Message Enhance(Message message)
        {
            switch (message.Action)
            {
                case ActionType.Add:
                    return Add(message);
                case ActionType.Remove:
                    return Remove(message);
                case ActionType.Edit:
                    return Edit(message);
                case ActionType.SearchByWord:
                    return SearchByKey(message);
                case ActionType.SearchByLetter:
                    return SearchByLetter(message);
                case ActionType.GetAllDictionary:
                    return GetDictionary();
            }
            return null;
        }

        private static Message Add(Message message)
        {
            try
            {
                DictionaryFromJson();
                dictionary.Add(message.Key, message.Value);
                DictionaryToJson();
                Console.WriteLine(DateTime.Now + " - Successfully added new entry '{0}'", message.Key);
                return new Message { Result = "Successfully added new entry '" + message.Key + "'"};
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(DateTime.Now + " - Attempt of adding an entry that already exists '{0}'", message.Key);
                return new Message { Result = e.Message + " " + message.Key };
            }
        }

        private static Message Remove(Message message)
        {
            try
            {
                DictionaryFromJson();
                if (dictionary.ContainsKey(message.Key))
                {
                    dictionary.Remove(message.Key);
                    DictionaryToJson();
                    Console.WriteLine(DateTime.Now + " - Successfully removed entry '{0}'", message.Key);
                    return new Message { Result = "Successfully removed entry '" + message.Key + "'" };
                }
                Console.WriteLine(DateTime.Now + " - Attempt of removing an entry that doesn't exist '{0}'", message.Key);
                return new Message { Result = "Attempt of removing an entry that doesn't exist " + message.Key };
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(DateTime.Now + " - Error occurred when try to perform an entry remove '{0}'", message.Key);
                return new Message { Result = e.Message + " " + message.Key };
            }
        }

        private static Message Edit(Message message)
        {
            try
            {
                DictionaryFromJson();
                dictionary[message.Key] = message.Value;
                DictionaryToJson();
                Console.WriteLine(DateTime.Now + " - Successfully edited entry '{0}'", message.Key);
                return new Message {Result = "Successfully edited entry '" + message.Key + "'"};
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now + " - Entry was not edited '{0}'", message.Key);
                return new Message { Result = "Entry was not edited '" + message.Key + "'" };
            }
        }

        private static Message SearchByLetter(Message message)
        {
            DictionaryFromJson();
            var m = dictionary.Where(e => message.Key[0] == e.Key[0]).ToDictionary(d => d.Key, d => d.Value);
            if (m.Count > 0)
            {
                Console.WriteLine(DateTime.Now + " - Attempt to get entry by key {0} unsuccessful", message.Key);
                return new Message { Dictionary = m, Result = "Something found" };
            }
            Console.WriteLine(DateTime.Now + " - Attempt to get entry by key {0} successful", message.Key);
            return new Message { Result = "Nothing found" };
        }

        private static Message SearchByKey(Message message)
        {
            DictionaryFromJson();
            if (dictionary.Keys.Any(k => k.Contains(message.Key)))
            {
                Console.WriteLine(DateTime.Now + " - Attempt to get entry by key '{0}' successful", message.Key);
                return new Message {Key = message.Key, Value = dictionary.First(k => k.Key.Contains(message.Key)).Value, Result = "Something found"};
            }
            return new Message{Result = "Nothing found"};
        }

        private static Message GetDictionary()
        {
            DictionaryFromJson();
            return new Message{Dictionary = dictionary, Result = "Dictionary loaded"};
        }

        private static void DictionaryFromJson()
        {
            var json = File.ReadAllText(path);
            dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        private static void DictionaryToJson()
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(dictionary));
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
