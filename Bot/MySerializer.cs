using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Bot
{
    class MySerializer :  IFormatter
    {
        private readonly BinaryFormatter _serializer;

        public MySerializer()
        {
            _serializer = new BinaryFormatter();

            _serializer.Binder = new SearchAssembliesBinder(Assembly.GetExecutingAssembly(), true);
        }

        public object Deserialize(Stream serializationStream)
        {
            return _serializer.Deserialize(serializationStream);
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            _serializer.Serialize(serializationStream, graph);
        }

        public ISurrogateSelector SurrogateSelector { get; set; }
        public SerializationBinder Binder { get; set; }
        public StreamingContext Context { get; set; }
    }
}