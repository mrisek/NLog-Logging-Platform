using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml.Serialization;

namespace NLogPoC
{
    public class Serialization
    {
        #region Member variables

        // stores list of serializable test objects
        private Dictionary<int, TestSimpleObject> m_listConversations = new Dictionary<int, TestSimpleObject>();

        #endregion

        #region Collection serialization

        // A test objects serialized by using SOAP format
        [Serializable()]
        public class TestSimpleObject
        {
            public int member1;
            public string member2;
            public string member3;
            public double member4;

            // A field that is not serialized.
            [NonSerialized()]
            public string member5;

            public TestSimpleObject()
            {
                member1 = 11;
                member2 = "hello";
                member3 = "hello";
                member4 = 3.14159265;
                member5 = "hello world!";
            }

            /// <summary>
            /// Method for printing all elements of TestSimpleObject instance
            /// </summary>
            public void Print()
            {
                Console.WriteLine("member1 = '{0}'", member1);
                Console.WriteLine("member2 = '{0}'", member2);
                Console.WriteLine("member3 = '{0}'", member3);
                Console.WriteLine("member4 = '{0}'", member4);
                Console.WriteLine("member5 = '{0}'", member5);
            }
        }

        /// <summary>
        /// Method for creating serialized collection by using SOAP format
        /// </summary>
        /// <returns></returns>
        static Stream CreateSerializedCollection()
        {
            //Creates a new TestSimpleObject object.
            TestSimpleObject obj = new TestSimpleObject();

            Console.WriteLine("Before serialization the object contains: ");
            obj.Print();

            //Opens a file and serializes the object into it in binary format.
            Stream stream = File.Open("data.xml", FileMode.Create);
            SoapFormatter formatter = new SoapFormatter();

            //BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);
            stream.Close();

            return stream;
        }

        /// <summary>
        /// Method for deserializing collections in SOAP format
        /// </summary>
        /// <param name="stream">serialized object in binary format that we want to deserialize</param>
        static void DeserializeCollection(Stream stream)
        {
            TestSimpleObject obj = new TestSimpleObject();
            //Empties obj.
            obj = null;

            //Opens file "data.xml" and deserializes the object from it.
            stream = File.Open("data.xml", FileMode.Open);

            SoapFormatter formatter = new SoapFormatter();
            formatter = new SoapFormatter();

            //formatter = new BinaryFormatter();

            obj = (TestSimpleObject)formatter.Deserialize(stream);
            stream.Close();

            Console.WriteLine("");
            Console.WriteLine("After deserialization the object contains: ");
            obj.Print();
        }

        /// <summary>
        /// Method for converting stream to byte array
        /// </summary>
        /// <param name="input">input stream that we want to convert</param>
        /// <returns></returns>
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                try
                {
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                }
                catch (ObjectDisposedException e)
                {
                    Console.WriteLine("Conversion failed" + e.Message);
                }

                return ms.ToArray();
            }
        }


        #endregion

        #region XML deserialization

        /// <summary>
        /// Sample collection of objects deserialized by 
        /// </summary>
        public class CollectionTestObject
        {
            public object Key;
            public object Value;

            /// <summary>
            /// Default constructor for CollectionTestObject
            /// </summary>
            public CollectionTestObject()
            {
            }

            /// <summary>
            /// Constructor for CollectionTestObject
            /// </summary>
            /// <param name="key">sample key</param>
            /// <param name="value">sample value</param>
            public CollectionTestObject(object key, object value)
            {
                Key = key;
                Value = value;
            }
        }

        [XmlRoot("StepList")]
        public class StepList
        {
            [XmlElement("Step")]
            public List<Step> Steps { get; set; }

            /// <summary>
            /// Method for printing out all objects from collection
            /// </summary>
            public void Print()
            {
                int count = 1;
                Steps.ForEach(
                    item => Console.Write(
                        count++ +
                        ". Value: " +
                        item.Name +
                        ", Key: " +
                        item.Desc +
                        "\n"
                    )
                );
            }
        }

        public class Step
        {
            [XmlElement("Name")]
            public string Name { get; set; }
            [XmlElement("Desc")]
            public string Desc { get; set; }
        }

        #endregion
    }
}
