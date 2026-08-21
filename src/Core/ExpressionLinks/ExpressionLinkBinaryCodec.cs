using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ExpressionLinkBinaryCodec
    {
        private const byte FormatVersion = 1;
        private const byte RecordVersion = 1;
        private const byte EnabledFlag = 1;
        private const int HeaderLength = 8;
        private const int MinimumRecordLength = 62;

        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        internal static byte[] Encode(IList<ExpressionLinkDefinition> links)
        {
            ExpressionLinkDefinition[] normalized;
            string error;
            if (!ExpressionLinkValidator.TryNormalizeList(
                links,
                out normalized,
                out error))
            {
                throw new ArgumentException(error, "links");
            }

            using (MemoryStream payload = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(payload, Encoding.UTF8))
            {
                WriteHeader(writer, normalized.Length);
                for (int i = 0; i < normalized.Length; i++)
                {
                    byte[] record = EncodeRecord(normalized[i]);
                    if (record.Length > ExpressionLinkLimits.MaximumRecordBytes)
                    {
                        throw new InvalidOperationException(
                            "Expression link " + i +
                            " exceeds the maximum record size.");
                    }

                    writer.Write(record.Length);
                    writer.Write(record);
                    if (payload.Length > ExpressionLinkLimits.MaximumPayloadBytes)
                    {
                        throw new InvalidOperationException(
                            "The expression link payload exceeds the maximum size.");
                    }
                }

                writer.Flush();
                return payload.ToArray();
            }
        }

        internal static bool TryEncode(
            IList<ExpressionLinkDefinition> links,
            out byte[] payload,
            out string error)
        {
            payload = new byte[0];
            error = string.Empty;
            try
            {
                payload = Encode(links);
                return true;
            }
            catch (ArgumentException exception)
            {
                error = exception.Message;
                return false;
            }
            catch (InvalidOperationException exception)
            {
                error = exception.Message;
                return false;
            }
        }

        internal static bool TryDecode(
            byte[] payload,
            out ExpressionLinkDefinition[] links,
            out string error)
        {
            links = new ExpressionLinkDefinition[0];
            error = string.Empty;

            if (payload == null)
            {
                error = "The expression link payload is null.";
                return false;
            }

            if (payload.Length > ExpressionLinkLimits.MaximumPayloadBytes)
            {
                error = "The expression link payload exceeds the maximum size.";
                return false;
            }

            if (payload.Length < HeaderLength)
            {
                error = "The expression link payload is truncated.";
                return false;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(payload, false))
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
                {
                    if (reader.ReadByte() != (byte)'E' ||
                        reader.ReadByte() != (byte)'L' ||
                        reader.ReadByte() != (byte)'N' ||
                        reader.ReadByte() != (byte)'K')
                    {
                        error = "The expression link payload magic is invalid.";
                        return false;
                    }

                    byte formatVersion = reader.ReadByte();
                    if (formatVersion != FormatVersion)
                    {
                        error = "Unsupported expression link payload version " +
                            formatVersion + ".";
                        return false;
                    }

                    byte headerFlags = reader.ReadByte();
                    if (headerFlags != 0)
                    {
                        error = "The expression link payload uses unknown flags.";
                        return false;
                    }

                    int count = reader.ReadUInt16();
                    if (count > ExpressionLinkLimits.MaximumLinks)
                    {
                        error = "The expression link count exceeds the supported maximum.";
                        return false;
                    }

                    ExpressionLinkDefinition[] decoded =
                        new ExpressionLinkDefinition[count];
                    HashSet<Guid> ids = new HashSet<Guid>();
                    for (int i = 0; i < count; i++)
                    {
                        if (stream.Length - stream.Position < sizeof(int))
                        {
                            error = "Expression link record " + i + " is truncated.";
                            return false;
                        }

                        int recordLength = reader.ReadInt32();
                        if (recordLength < MinimumRecordLength ||
                            recordLength > ExpressionLinkLimits.MaximumRecordBytes)
                        {
                            error = "Expression link record " + i +
                                " has an invalid size.";
                            return false;
                        }

                        long recordEnd = stream.Position + recordLength;
                        if (recordEnd > stream.Length)
                        {
                            error = "Expression link record " + i + " is truncated.";
                            return false;
                        }

                        ExpressionLinkDefinition decodedLink;
                        if (!TryDecodeRecord(
                            reader,
                            recordEnd,
                            out decodedLink,
                            out error))
                        {
                            error = "Expression link record " + i +
                                " is invalid: " + error;
                            return false;
                        }

                        if (stream.Position != recordEnd)
                        {
                            error = "Expression link record " + i +
                                " has trailing or missing data.";
                            return false;
                        }

                        if (!ids.Add(decodedLink.Id))
                        {
                            error = "Expression link record " + i +
                                " duplicates ID " + decodedLink.Id + ".";
                            return false;
                        }

                        decoded[i] = decodedLink;
                    }

                    if (stream.Position != stream.Length)
                    {
                        error = "The expression link payload contains trailing data.";
                        return false;
                    }

                    links = decoded;
                    return true;
                }
            }
            catch (EndOfStreamException)
            {
                error = "The expression link payload is truncated.";
                return false;
            }
            catch (DecoderFallbackException exception)
            {
                error = "The expression link payload contains invalid UTF-8 text: " +
                    exception.Message;
                return false;
            }
            catch (IOException exception)
            {
                error = "The expression link payload could not be read: " +
                    exception.Message;
                return false;
            }
            catch (ArgumentException exception)
            {
                error = "The expression link payload is invalid: " +
                    exception.Message;
                return false;
            }
            catch (OverflowException exception)
            {
                error = "The expression link payload contains an invalid length: " +
                    exception.Message;
                return false;
            }
        }

        private static void WriteHeader(BinaryWriter writer, int count)
        {
            writer.Write((byte)'E');
            writer.Write((byte)'L');
            writer.Write((byte)'N');
            writer.Write((byte)'K');
            writer.Write(FormatVersion);
            writer.Write((byte)0);
            writer.Write((ushort)count);
        }

        private static byte[] EncodeRecord(ExpressionLinkDefinition link)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write(RecordVersion);
                writer.Write(link.Id.ToByteArray());
                writer.Write(link.Enabled ? EnabledFlag : (byte)0);
                writer.Write((short)link.Priority);
                writer.Write((byte)link.Scope);
                writer.Write((short)link.SlotIndex);
                writer.Write((short)link.ComponentIndex);
                writer.Write((byte)link.Mode);
                writer.Write(link.Threshold);
                writer.Write(link.InputMin);
                writer.Write(link.InputMax);
                writer.Write(link.OutputMin);
                writer.Write(link.OutputMax);
                writer.Write(link.SmoothingSpeed);
                WriteString(writer, link.Name);
                WriteString(writer, link.Source);
                WriteString(writer, link.RendererPath);
                WriteString(writer, link.RendererHint);
                WriteString(writer, link.MeshHint);
                WriteString(writer, link.BlendshapeName);
                writer.Flush();
                return stream.ToArray();
            }
        }

        private static bool TryDecodeRecord(
            BinaryReader reader,
            long recordEnd,
            out ExpressionLinkDefinition link,
            out string error)
        {
            link = null;
            error = string.Empty;

            byte recordVersion = reader.ReadByte();
            if (recordVersion != RecordVersion)
            {
                error = "Unsupported record version " + recordVersion + ".";
                return false;
            }

            byte[] guidBytes = ReadExact(reader, 16);
            Guid id = new Guid(guidBytes);
            byte flags = reader.ReadByte();
            if ((flags & ~EnabledFlag) != 0)
            {
                error = "The record uses unknown flags.";
                return false;
            }

            int priority = reader.ReadInt16();
            ExpressionTargetScope scope =
                (ExpressionTargetScope)reader.ReadByte();
            int slotIndex = reader.ReadInt16();
            int componentIndex = reader.ReadInt16();
            ExpressionLinkMode mode =
                (ExpressionLinkMode)reader.ReadByte();

            float threshold = reader.ReadSingle();
            float inputMin = reader.ReadSingle();
            float inputMax = reader.ReadSingle();
            float outputMin = reader.ReadSingle();
            float outputMax = reader.ReadSingle();
            float smoothingSpeed = reader.ReadSingle();

            string name;
            string source;
            string rendererPath;
            string rendererHint;
            string meshHint;
            string blendshapeName;
            if (!TryReadString(
                    reader,
                    recordEnd,
                    ExpressionLinkLimits.MaximumNameBytes,
                    out name,
                    out error) ||
                !TryReadString(
                    reader,
                    recordEnd,
                    ExpressionLinkLimits.MaximumSourceBytes,
                    out source,
                    out error) ||
                !TryReadString(
                    reader,
                    recordEnd,
                    ExpressionLinkLimits.MaximumRendererPathBytes,
                    out rendererPath,
                    out error) ||
                !TryReadString(
                    reader,
                    recordEnd,
                    ExpressionLinkLimits.MaximumRendererHintBytes,
                    out rendererHint,
                    out error) ||
                !TryReadString(
                    reader,
                    recordEnd,
                    ExpressionLinkLimits.MaximumMeshHintBytes,
                    out meshHint,
                    out error) ||
                !TryReadString(
                    reader,
                    recordEnd,
                    ExpressionLinkLimits.MaximumBlendshapeNameBytes,
                    out blendshapeName,
                    out error))
            {
                return false;
            }

            ExpressionLinkDefinition decoded = new ExpressionLinkDefinition
            {
                Id = id,
                Name = name,
                Enabled = (flags & EnabledFlag) != 0,
                Source = source,
                Scope = scope,
                SlotIndex = slotIndex,
                RendererPath = rendererPath,
                ComponentIndex = componentIndex,
                RendererHint = rendererHint,
                MeshHint = meshHint,
                BlendshapeName = blendshapeName,
                Mode = mode,
                Threshold = threshold,
                InputMin = inputMin,
                InputMax = inputMax,
                OutputMin = outputMin,
                OutputMax = outputMax,
                SmoothingSpeed = smoothingSpeed,
                Priority = priority
            };

            ExpressionLinkDefinition normalized;
            if (!ExpressionLinkValidator.TryNormalize(
                decoded,
                out normalized,
                out error))
            {
                return false;
            }

            link = normalized;
            return true;
        }

        private static void WriteString(BinaryWriter writer, string value)
        {
            byte[] bytes = StrictUtf8.GetBytes(value ?? string.Empty);
            writer.Write((ushort)bytes.Length);
            writer.Write(bytes);
        }

        private static bool TryReadString(
            BinaryReader reader,
            long recordEnd,
            int maximumBytes,
            out string value,
            out string error)
        {
            value = string.Empty;
            error = string.Empty;

            Stream stream = reader.BaseStream;
            if (recordEnd - stream.Position < sizeof(ushort))
            {
                error = "A string length is truncated.";
                return false;
            }

            int byteCount = reader.ReadUInt16();
            if (byteCount > maximumBytes)
            {
                error = "A string exceeds its UTF-8 byte limit.";
                return false;
            }

            if (recordEnd - stream.Position < byteCount)
            {
                error = "A string value is truncated.";
                return false;
            }

            byte[] bytes = ReadExact(reader, byteCount);
            value = StrictUtf8.GetString(bytes);
            return true;
        }

        private static byte[] ReadExact(BinaryReader reader, int count)
        {
            byte[] bytes = reader.ReadBytes(count);
            if (bytes.Length != count)
            {
                throw new EndOfStreamException();
            }

            return bytes;
        }
    }
}
