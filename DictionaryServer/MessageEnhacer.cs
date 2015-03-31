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
    internal static class MessageEnhacer
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
                return new Message { Result = "Successfully added new entry" + message.Key };
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
                dictionary.Remove(message.Key);
                DictionaryToJson();
                Console.WriteLine(DateTime.Now + " - Successfully removed entry '{0}'", message.Key);
                return new Message {Result = "Successfully removed entry" + message.Key};
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(DateTime.Now + " - Attempt of removing an entry that doesn't exist '{0}'", message.Key);
                return new Message { Result = e.Message + " " + message.Key };
            }
        }

        private static Message Edit(Message message)
        {
            DictionaryFromJson();
            dictionary[message.Key] = message.Value;
            DictionaryToJson();
            Console.WriteLine(DateTime.Now + " - Successfully edited entry '{0}'", message.Key);
            return new Message {Result = "Successfully edited entry" + message.Key};
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
