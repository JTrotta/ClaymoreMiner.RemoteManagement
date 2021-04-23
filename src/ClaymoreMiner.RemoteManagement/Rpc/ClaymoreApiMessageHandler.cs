using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;

namespace ClaymoreMiner.RemoteManagement.Rpc
{
    internal class ClaymoreApiMessageHandler : StreamMessageHandler
    {
        private readonly string _password;

        public ClaymoreApiMessageHandler(string password, Stream stream) : base(stream, stream, new JsonMessageFormatter())
        {
            _password = password;
        }

        protected override async ValueTask<JsonRpcMessage> ReadCoreAsync(CancellationToken cancellationToken)
        {
            using (var data = new MemoryStream())
            {
                var buffer = new byte[32768];

                while (true)
                {
                    var read = await ReceivingStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    data.Write(buffer, 0, read);

                    var contentBytes = data.ToArray();
                    if (contentBytes.Length == 0)
                        return null;

                    //var content = Encoding.ASCII.GetString(contentBytes);
                    var ros = new ReadOnlySequence<byte>(contentBytes);
                    try
                    {
                        return this.Formatter.Deserialize(ros);
                    }
                    catch (JsonException)
                    {
                    }
                }
            }
        }


        protected override ValueTask WriteCoreAsync(JsonRpcMessage content, CancellationToken cancellationToken)
        {
            var contentBytes = Encoding.ASCII.GetBytes(TransformContent(content.ToString()));

            Task task = SendingStream.WriteAsync(contentBytes, 0, contentBytes.Length, cancellationToken);

            return new ValueTask(task);
        }

        private string TransformContent(string content)
        {
            var contentJson = JObject.Parse(content);

            if (_password != null)
                contentJson.Add("pwd", _password);

            var transformedContent = contentJson.ToString(Formatting.None) + '\n';

            return transformedContent;
        }
    }
}