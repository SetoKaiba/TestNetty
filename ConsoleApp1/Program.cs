using System;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Codecs.Http.WebSockets.Extensions.Compression;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace ConsoleApp1
{
    class Program
    {
        public static async Task RunClientAsync(string url)
        {
            Console.WriteLine(url);
            var uri = new Uri(url);
            var ssl = url.StartsWith("wss");

            var workerGroup = new MultithreadEventLoopGroup();

            // Connect with V13 (RFC 6455 aka HyBi-17). You can change it to V08 or V00.
            // If you change it to V00, ping is not supported and remember to change
            // HttpResponseDecoder to WebSocketHttpResponseDecoder in the pipeline.
            var handler = new WebSocketClientHandler(
                WebSocketClientHandshakerFactory.NewHandshaker(
                    uri,
                    WebSocketVersion.V13,
                    null,
                    true,
                    new DefaultHttpHeaders()
                ));


            var bootstrap = new Bootstrap()
                .Group(workerGroup)
                .Option(ChannelOption.TcpNodelay, true)
                .Channel<TcpSocketChannel>()
                .Handler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new TestHandler());
                    if (ssl)
                    {
                        pipeline.AddLast("tls",
                            new TlsHandler(
                                stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true),
                                new ClientTlsSettings(uri.Host)));
                    }

                    pipeline.AddLast(new HttpClientCodec(),
                        new HttpObjectAggregator(8192),
                        WebSocketClientCompressionHandler.Instance,
                        handler);
                }));
            await bootstrap.ConnectAsync(new DnsEndPoint(uri.Host, uri.Port));
            await handler.HandshakeCompletion;
        }

        static void Main()
        {
            var url_ssl = "wss://live.kaiba.net:8443/live-bilibili/5619408/0130ab9c-a07b-4465-bc89-a6f59ba4c5de";
            RunClientAsync(url_ssl);
            Console.ReadLine();
        }
    }
}