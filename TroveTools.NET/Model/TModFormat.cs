using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.DataAccess;
using TroveTools.NET.Framework;

namespace TroveTools.NET.Model
{
    static class TModFormat
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const long AdditionalDataOffset = 7;
        private static readonly byte[] TroveSpecialBytePattern1 = new byte[] { 0x00, 0x00, 0x80, 0xFF, 0x7F };
        private static readonly byte[] TroveSpecialBytePattern2 = new byte[] { 0x00, 0x44, 0x13, 0xBB, 0xEC };

        public static void ReadTmodProperties(string file, Dictionary<string, string> properties)
        {
            using (var stream = File.OpenRead(file))
            {
                using (var reader = new BinaryReader(stream))
                {
                    // Start at beginning of the file, read headerSize (fixed64), tmodVersion (fixed16), and propertyCount (fixed16)
                    stream.Position = 0;
                    ulong headerSize = reader.ReadUInt64();
                    ushort tmodVersion = reader.ReadUInt16();
                    ushort propertyCount = reader.ReadUInt16();

                    // Read a number of properties based on the propertyCount value
                    for (int i = 0; i < propertyCount; i++)
                    {
                        string key = reader.ReadString();
                        string value = reader.ReadString();

                        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value)) properties[key] = value;
                    }
                }
            }
        }

        public static void ExtractTmod(string file, string folder, Action<double> updateProgress)
        {
            var buffer = new byte[1048576];
            var properties = new Dictionary<string, string>();
            var archiveEntries = new List<ArchiveIndexEntry>();
            using (var stream = File.OpenRead(file))
            {
                using (var reader = new MyBinaryReader(stream))
                {
                    // Start at beginning of the file, read headerSize (fixed64), tmodVersion (fixed16), and propertyCount (fixed16)
                    stream.Position = 0;
                    ulong headerSize = reader.ReadUInt64();
                    ushort tmodVersion = reader.ReadUInt16();
                    ushort propertyCount = reader.ReadUInt16();

                    // Read a number of properties based on the propertyCount value
                    for (int i = 0; i < propertyCount; i++)
                    {
                        string key = reader.ReadString();
                        string value = reader.ReadString();

                        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value)) properties[key] = value;
                    }

                    // Read archive index entries (remainder of header)
                    while ((ulong)stream.Position < headerSize)
                    {
                        var entry = new ArchiveIndexEntry();
                        entry.file = reader.ReadString();
                        entry.archiveIndex = reader.Read7BitEncodedInt();
                        entry.byteOffset = reader.Read7BitEncodedInt();
                        entry.size = reader.Read7BitEncodedInt();
                        entry.hash = reader.Read7BitEncodedInt();

                        archiveEntries.Add(entry);
                    }

                    // I observed 7 bytes between the header and start of the file data that isn't accounted for
                    // (in all the mods I checked these 7 bytes were exactly the same: 78 01 00 00 80 FF 7F)
                    double count = 0;
                    foreach (var entry in archiveEntries)
                    {
                        updateProgress(count / archiveEntries.Count * 100d);
                        log.InfoFormat("Extracting {0} from {1}", entry.file, file);
                        string extractPath = Path.Combine(folder, entry.file.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
                        SettingsDataProvider.ResolveFolder(Path.GetDirectoryName(extractPath));
                        SaveBytes(extractPath, stream, Convert.ToInt64(headerSize) + AdditionalDataOffset + Convert.ToInt64(entry.byteOffset), entry.size, buffer);
                    }

                    log.InfoFormat("Completed extracting files from {0}", file);
                }
            }
        }

        private static BoyerMooreByteSearch _search1 = null, _search2 = null;

        private static void SaveBytes(string extractPath, FileStream stream, long position, int size, byte[] buffer)
        {
            int bytesToRead = size, read = 0;

            // We are going to use the initial bytes in buffer to store the last bytes read to ensure the special pattern is not missed between between buffer switches
            int patternLength = TroveSpecialBytePattern1.Length;
            if (_search1 == null) _search1 = new BoyerMooreByteSearch(TroveSpecialBytePattern1);
            if (_search2 == null) _search2 = new BoyerMooreByteSearch(TroveSpecialBytePattern2);
            for (int i = 0; i < patternLength; i++) buffer[i] = 0; // initialize buffer bytes to 0

            stream.Position = position;
            using (FileStream output = File.Create(extractPath))
            {
                do
                {
                    read = stream.Read(buffer, patternLength, bytesToRead < buffer.Length - patternLength ? bytesToRead : buffer.Length - patternLength);

                    // Search for either special byte pattern
                    int match1 = _search1.Match(buffer, 0, read + patternLength);
                    int match2 = _search2.Match(buffer, 0, read + patternLength);
                    // Get the first of the two pattern match positions
                    int match = match1 != -1 && match2 != -1 ? Math.Min(match1, match2) : (match1 != -1 ? match1 : match2);

                    if (match == -1)
                    {
                        // Special pattern not found: write out data exactly as read
                        output.Write(buffer, patternLength, read);
                        bytesToRead -= read;
                    }
                    else
                    {
                        // Special pattern found: write to output file skipping the pattern bytes
                        int writeOffset = patternLength, writeCount;
                        do
                        {
                            writeCount = match - writeOffset;
                            output.Write(buffer, writeOffset, writeCount);
                            bytesToRead -= writeCount;
                            writeOffset = match + patternLength;

                            match1 = _search1.Match(buffer, writeOffset, read + patternLength - writeOffset);
                            match2 = _search2.Match(buffer, writeOffset, read + patternLength - writeOffset);
                            match = match1 != -1 && match2 != -1 ? Math.Min(match1, match2) : (match1 != -1 ? match1 : match2);
                        } while (match != -1);

                        // Write out the remainder of the data read
                        writeCount = read - writeOffset + patternLength;
                        if (writeCount > 0)
                        {
                            output.Write(buffer, writeOffset, writeCount);
                            bytesToRead -= writeCount;
                        }
                    }
                    // Copy last bytes read to the beginning of the buffer
                    if (read >= patternLength)
                        Array.Copy(buffer, read, buffer, 0, patternLength);
                    else
                        Array.Copy(buffer, patternLength, buffer, patternLength - read, read);
                } while (read > 0 && bytesToRead > 0);
                output.Flush();
            }
            if (bytesToRead > 0) log.ErrorFormat("Error extracting {0}: {1} bytes left to read and 0 bytes read from source", extractPath, bytesToRead);
        }

        class ArchiveIndexEntry
        {
            public string file; // Original filename (in .tmods this includes directory separators)
            public int archiveIndex; // <- Should be 0
            public int byteOffset; // Offset into allTheData
            public int size; // Size in bytes
            public int hash; // Data integrity
        }
    }

    public class MyBinaryReader : BinaryReader
    {
        public MyBinaryReader(Stream stream) : base(stream) { }
        public new int Read7BitEncodedInt() { return base.Read7BitEncodedInt(); }
    }
}
