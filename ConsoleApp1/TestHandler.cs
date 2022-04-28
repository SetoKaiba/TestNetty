using System;
using System.IO;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace ConsoleApp1
{
    public class TestHandler : ChannelHandlerAdapter
    {
        public int count;
        
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Console.WriteLine("in:" + message);
            var byteBuffer = (IByteBuffer) message;
            var bytes = new byte[byteBuffer.ReadableBytes];
            byteBuffer.MarkReaderIndex();
            byteBuffer.ReadBytes(bytes);
            byteBuffer.ResetReaderIndex();
            count++;
            base.ChannelRead(context, message);
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            Console.WriteLine("out:" + message);
            var byteBuffer = (IByteBuffer) message;
            var bytes = new byte[byteBuffer.ReadableBytes];
            byteBuffer.MarkReaderIndex();
            byteBuffer.ReadBytes(bytes);
            byteBuffer.ResetReaderIndex();
            count++;
            return base.WriteAsync(context, message);
        }
    }
}