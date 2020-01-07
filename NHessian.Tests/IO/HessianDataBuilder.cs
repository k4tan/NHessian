using NHessian.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NHessian.Tests.IO
{
    internal class HessianDataBuilder
    {
        private readonly List<byte> _byteList = new List<byte>();

        public byte[] ToArray() => _byteList.ToArray();

        public HessianStreamReader ToReader() => new HessianStreamReader(new MemoryStream(ToArray()));

        public HessianDataBuilder WriteBytes(params byte[] bytes)
        {
            _byteList.AddRange(bytes);
            return this;
        }

        public HessianDataBuilder WriteChar(params char[] chars) => WriteBytes(chars.Select(it => (byte)it).ToArray());

        public HessianDataBuilder WriteUtf8(string str)
        {
            WriteBytes(Encoding.UTF8.GetBytes(str));
            return this;
        }
    }
}