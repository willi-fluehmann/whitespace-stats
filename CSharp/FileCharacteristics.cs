using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhitespaceStats
{
    public class FileCharacteristics
    {
        public bool IsText { get; private set; }
        public Encoding TextEncoding { get; private set; } // null if binary or unknown encoding
        public TextStatistics TextStatistics { get; private set; } // null if binary

        public string EncodingName
        {
            get
            {
                if (this.IsText)
                {
                    if (this.TextEncoding != null)
                    {
                        return this.TextEncoding.EncodingName;
                    }
                    else
                    {
                        return "Any 8 bit text";
                    }
                }
                else
                {
                    return "Binary";
                }
            }
        }

        private FileCharacteristics()
        {
        }

        public static FileCharacteristics Analyze(FileStream fileStream)
        {
            var fileCharacteristics = new FileCharacteristics();

            Encoding identifiedEncoding = EncodingUtils.Identify(fileStream);
            Encoding assumedEncoding = identifiedEncoding ?? new UTF8Encoding(); // default: UTF-8

            var textStatistics = TextStatistics.Analyze(new StreamReader(new BufferedStream(fileStream), assumedEncoding));
            if (textStatistics.NonPrintableCharacters < 1)
            {
                fileCharacteristics.IsText = true;
                fileCharacteristics.TextEncoding = identifiedEncoding;
                fileCharacteristics.TextStatistics = textStatistics;
            }
            else
            {
                fileCharacteristics.IsText = false;
            }

            return fileCharacteristics;
        }
    }
}
