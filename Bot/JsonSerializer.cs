using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Bot
{
    public class JsonSerializer : IFormatter
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        public JsonSerializer()
        {
            _serializer = new Newtonsoft.Json.JsonSerializer();
        }

        public object Deserialize(Stream serializationStream)
        {
            return _serializer.Deserialize(new JsonTextReader(new StreamReader(serializationStream)));
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            using (var sw = new StreamWriter(serializationStream))
            {
                _serializer.Serialize(sw, graph);
            }
        }

        public ISurrogateSelector SurrogateSelector { get; set; }
        public SerializationBinder Binder { get; set; }
        public StreamingContext Context { get; set; }
    }
}