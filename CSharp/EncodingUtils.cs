using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhitespaceStats
{
    public static class EncodingUtils
    {
        private static Encoding _utf8 = new UTF8Encoding();
        private static Encoding _utf16 = new UnicodeEncoding();
        private static Encoding _utf32 = new UTF32Encoding();

        private static Tuple<byte[], Encoding>[] _preambles = new[]
        {
            new Tuple<byte[], Encoding>(new byte[] { 0xEF, 0xBB, 0xBF }, _utf8), // _utf8.GetPreample() doesn't provide meaningful result
            new Tuple<byte[], Encoding>(_utf16.GetPreamble(), _utf16),
            new Tuple<byte[], Encoding>(_utf16.GetPreamble().Reverse().ToArray(), _utf16),
            new Tuple<byte[], Encoding>(_utf32.GetPreamble(), _utf32),
            new Tuple<byte[], Encoding>(_utf32.GetPreamble().Reverse().ToArray(), _utf32),
        };

        public static Encoding Identify(FileStream fileStream)
        {
            long originalPosition = fileStream.Position;

            Encoding encodingFound = null;
            foreach (var preamble in _preambles)
            {
                int encodingPreambleLength = preamble.Item1.Length;
                if (fileStream.Length >= encodingPreambleLength)
                {
                    fileStream.Position = 0;
                    var actualPreamble = new byte[encodingPreambleLength];
                    fileStream.Read(actualPreamble, 0, actualPreamble.Length);

                    if (actualPreamble.SequenceEqual(preamble.Item1))
                    {
                        encodingFound = preamble.Item2;
                        break;
                    }
                }
            }

            fileStream.Position = originalPosition;

            return encodingFound;
        }
    }
}
