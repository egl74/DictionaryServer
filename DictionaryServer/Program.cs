using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using MessageNamespace;

namespace DictionaryServer
{
    class Program
    {
        static void Main(string[] args)
        {
            { 
                TcpListener server = null;
                try
                {
                    // Set the TcpListener on port 13000.
                    Int32 port = 13000;

                    // TcpListener server = new TcpListener(port);
                    server = new TcpListener(IPAddress.Any, port);

                    // Start listening for client requests.
                    server.Start();
                    
                    // Enter the listening loop. 
                    while (true)
                    {
                        Console.Write("Waiting for a connection... ");

                        // Perform a blocking call to accept requests. 
                        TcpClient client = server.AcceptTcpClient();
                        Console.WriteLine("Connected!");
                        
                        // Get a stream object for reading and writing
                        NetworkStream stream = client.GetStream();
                        
                        //receive length of query
                        byte[] length = new byte[4];
                        stream.Read(length, 0, sizeof (int));

                        Byte[] bytes = new Byte[BitConverter.ToInt32(length, 0)];

                        // Loop to receive all the data sent by the client. 
                        while ((stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            Console.WriteLine("Data received");

                            // Process the data sent by the client.

                            var m = (Message)DeserializeFromStream(new MemoryStream(bytes));
                            m = MessageHandler.Enhance(m);
                            byte[] msg = SerializeToStream(m).ToArray();

                            //send back length of a response
                            stream.Write(BitConverter.GetBytes(msg.Length), 0, sizeof(int));

                            // Send back a response.
                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine("Response sent");
                            //Console.WriteLine("Sent: {0}", data);
                        }

                        // Shutdown and end connection
                        client.Close();
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
                finally
                {
                    // Stop listening for new clients.
                    server.Stop();
                }


                Console.WriteLine("\nHit enter to continue...");
                Console.Read();
            }  
        }

        /// <summary>
        /// serializes the given object into memory stream
        /// </summary>
        /// <param name="objectType">the object to be serialized</param>
        /// <returns>The serialized object as memory stream</returns>
        public static MemoryStream SerializeToStream(object objectType)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, objectType);
            return stream;
        }

        /// <summary>
        /// deserializes as an object
        /// </summary>
        /// <param name="stream">the stream to deserialize</param>
        /// <returns>the deserialized object</returns>
        public static object DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object objectType = formatter.Deserialize(stream);
            return objectType;
        }
    }
}
