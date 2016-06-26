using System;
using System.Data;
using System.IO;
using Gulliver.Base;

namespace Gulliver.Managers {
    [HelpTopic("endian", Topic.Settings, "Defines the endianness of the struct interpretation.")]
    internal static class DataManager {
        internal enum EndianValue {
            Unknown,
            Little,
            LittleAuto,
            Big,
            BigAuto
        }

        public static int Count { get; private set; }

        [Setting("endian", typeof(EndianValue), EndianValue.Unknown, nameof(ValidateEndianness))]
        public static EndianValue Endianness { get; private set; }

        public static object ValidateEndianness(object value) {
            var s = value as string;
            if (s == null)
                throw new ArgumentException("Value must be a string!", nameof(value));
            EndianValue o;
            if (!Enum.TryParse(s, true, out o))
                throw new ArgumentException("Value could not be parsed!", nameof(value));
            if (o == EndianValue.LittleAuto || o == EndianValue.BigAuto)
                throw new ArgumentException("Can not set auto endianness!", nameof(value));
            return o;
        }

        public static double EndianConfidence { get; private set; }

        public static string Representation { get; private set; }

        public static string DataFile { get; private set; }

        public static long DataSize { get; private set; }

        public static void SetData(string filepath) {
            using (var file = File.OpenText(filepath)) {
                var lenStr = file.ReadLine();
                int len;
                if (!int.TryParse(lenStr, out len))
                    throw new DataException($"'{lenStr}' is not a valid value for the data length.");
                long length = 0;
                var big = 0;
                var little = 0;
                for (var i = 0; i < len; i++) {
                    var line = file.ReadLine();
                    if (line == null)
                        throw new DataException($"Failed to read data entry {i}");
                    try {
                        var data = Convert.FromBase64String(line);
                        length += data.LongLength;
                        if (data.Length >= 4) {
                            if (data[0] + data[1] < data[2] + data[3])
                                big++;
                            else
                                little++;
                        }
                    } catch (FormatException e) {
                        throw new DataException($"Failed to decode data entry {i}", e);
                    }
                }
                DataSize = length;
                Count = len;
                if (Math.Abs(big - little) > Math.Max(10, Count / 10)) {
                    Endianness = big > little ? EndianValue.BigAuto : EndianValue.LittleAuto;
                    EndianConfidence = (big > little ? big : little) / (double)Count;
                } else Endianness = EndianValue.Unknown;
            }
            DataFile = filepath;
        }
    }
}