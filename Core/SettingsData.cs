using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace MindustrySaveEditor.Core;

public class SettingsData
{
    private const byte TypeBool = 0;
    private const byte TypeInt = 1;
    private const byte TypeLong = 2;
    private const byte TypeFloat = 3;
    private const byte TypeString = 4;
    private const byte TypeBinary = 5;

    /// <summary>
    /// Les valeurs décodées, typées comme en Java :
    /// bool, int, long, float, string, byte[]
    /// </summary>
    public Dictionary<string, object> Values { get; set; } = new();

    public RichSettingsData ToRichData()
    {
        return new RichSettingsData(this);
    }

    // ──────────────────────────────────────────────
    //  DECODE
    // ──────────────────────────────────────────────

    /// <summary>
    /// Décode un fichier settings Mindustry (compressé Deflate ou raw).
    /// </summary>
    public static SettingsData Decode(byte[] raw)
    {
        byte[] data = TryDecompress(raw);
        using var ms = new MemoryStream(data);
        using var reader = new BigEndianReader(ms);

        var settings = new SettingsData();
        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            string key = reader.ReadUTF();
            byte type = reader.ReadByte();

            object value = type switch
            {
                TypeBool => reader.ReadBoolean(),
                TypeInt => reader.ReadInt32(),
                TypeLong => reader.ReadInt64(),
                TypeFloat => reader.ReadFloat(),
                TypeString => reader.ReadUTF(),
                TypeBinary => reader.ReadBinary(),
                _ => throw new InvalidDataException($"Type inconnu: {type} à la clé '{key}'")
            };

            settings.Values[key] = value;
        }

        return settings;
    }

    // ──────────────────────────────────────────────
    //  ENCODE
    // ──────────────────────────────────────────────

    /// <summary>
    /// Encode les settings au format Mindustry (compressé Deflate).
    /// </summary>
    public byte[] Encode(bool compressed = true)
    {
        using var rawMs = new MemoryStream();
        using (var writer = new BigEndianWriter(rawMs))
        {
            writer.WriteInt32(Values.Count);

            foreach (var (key, value) in Values)
            {
                writer.WriteUTF(key);

                switch (value)
                {
                    case bool b:
                        writer.WriteByte(TypeBool);
                        writer.WriteBoolean(b);
                        break;
                    case int n:
                        writer.WriteByte(TypeInt);
                        writer.WriteInt32(n);
                        break;
                    case long l:
                        writer.WriteByte(TypeLong);
                        writer.WriteInt64(l);
                        break;
                    case float f:
                        writer.WriteByte(TypeFloat);
                        writer.WriteFloat(f);
                        break;
                    case string s:
                        writer.WriteByte(TypeString);
                        writer.WriteUTF(s);
                        break;
                    case byte[] bytes:
                        writer.WriteByte(TypeBinary);
                        writer.WriteInt32(bytes.Length);
                        writer.WriteBytes(bytes);
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Type non supporté pour la clé '{key}': {value.GetType().Name}");
                }
            }
        }

        byte[] rawData = rawMs.ToArray();

        if (!compressed)
            return rawData;

        using var compressedMs = new MemoryStream();
        using (var deflate = new DeflateStream(compressedMs, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(rawData, 0, rawData.Length);
        }

        return compressedMs.ToArray();
    }
    
    // ──────────────────────────────────────────────
    //  UBJSON : GET / PUT JSON OBJECTS
    // ──────────────────────────────────────────────

    /// <summary>
    /// Lit un objet riche stocké comme byte[] UBJSON dans les settings.
    /// Retourne un JsonElement (System.Text.Json) que tu peux ensuite
    /// désérialiser vers ton type C# avec JsonSerializer.Deserialize&lt;T&gt;().
    /// </summary>
    public T? GetJson<T>(string name, T? defaultValue = default)
    {
        if (!Values.TryGetValue(name, out var raw) || raw is not byte[] bytes)
            return defaultValue;

        try
        {
            var jsonElement = UBJsonCodec.Decode(bytes);
            return JsonSerializer.Deserialize<T>(jsonElement);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Retourne le JsonElement brut (pour inspection ou manipulation dynamique).
    /// </summary>
    public JsonElement? GetJsonRaw(string name)
    {
        if (!Values.TryGetValue(name, out var raw) || raw is not byte[] bytes)
            return null;

        return UBJsonCodec.Decode(bytes);
    }

    /// <summary>
    /// Sérialise un objet C# en UBJSON et le stocke comme byte[] dans les settings.
    /// </summary>
    public void PutJson<T>(string name, T value)
    {
        var jsonElement = JsonSerializer.SerializeToElement(value);
        byte[] ubjsonBytes = UBJsonCodec.Encode(jsonElement);
        Values[name] = ubjsonBytes;
    }

    // ──────────────────────────────────────────────
    //  HELPERS
    // ──────────────────────────────────────────────

    /// <summary>
    /// Tente de décompresser (Deflate). Si ça échoue, on assume que c'est du raw.
    /// </summary>
    private static byte[] TryDecompress(byte[] data)
    {
        try
        {
            using var input = new MemoryStream(data);
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            deflate.CopyTo(output);
            return output.ToArray();
        }
        catch
        {
            return data; // Pas compressé
        }
    }

    // ──────────────────────────────────────────────
    //  TYPED ACCESSORS (confort)
    // ──────────────────────────────────────────────

    public bool GetBool(string key, bool defaultValue = false)
        => Values.TryGetValue(key, out var v) && v is bool b ? b : defaultValue;

    public int GetInt(string key, int defaultValue = 0)
        => Values.TryGetValue(key, out var v) && v is int n ? n : defaultValue;

    public long GetLong(string key, long defaultValue = 0)
        => Values.TryGetValue(key, out var v) && v is long l ? l : defaultValue;

    public float GetFloat(string key, float defaultValue = 0f)
        => Values.TryGetValue(key, out var v) && v is float f ? f : defaultValue;

    public string GetString(string key, string defaultValue = "")
        => Values.TryGetValue(key, out var v) && v is string s ? s : defaultValue;

    public byte[] GetBinary(string key)
        => Values.TryGetValue(key, out var v) && v is byte[] b ? b : [];

    public void Set(string key, object value) => Values[key] = value;
    public bool Remove(string key) => Values.Remove(key);
}

// ══════════════════════════════════════════════════
//  BigEndianReader — simule Java DataInputStream
// ══════════════════════════════════════════════════

internal class BigEndianReader : IDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buffer = new byte[8];

    public BigEndianReader(Stream stream) => _stream = stream;

    public byte ReadByte()
    {
        int b = _stream.ReadByte();
        if (b == -1) throw new EndOfStreamException();
        return (byte)b;
    }

    public bool ReadBoolean() => ReadByte() != 0;

    public int ReadInt32()
    {
        ReadExact(_buffer, 4);
        return BinaryPrimitives.ReadInt32BigEndian(_buffer);
    }

    public long ReadInt64()
    {
        ReadExact(_buffer, 8);
        return BinaryPrimitives.ReadInt64BigEndian(_buffer);
    }

    public float ReadFloat()
    {
        ReadExact(_buffer, 4);
        int bits = BinaryPrimitives.ReadInt32BigEndian(_buffer);
        return BitConverter.Int32BitsToSingle(bits);
    }

    /// <summary>
    /// Lit une string au format Java Modified UTF-8 :
    /// 2 bytes Big Endian (longueur en bytes) + bytes UTF-8.
    /// </summary>
    public string ReadUTF()
    {
        ReadExact(_buffer, 2);
        int length = BinaryPrimitives.ReadUInt16BigEndian(_buffer);
        byte[] strBytes = new byte[length];
        ReadExact(strBytes, length);
        return Encoding.UTF8.GetString(strBytes);
    }

    public byte[] ReadBinary()
    {
        int length = ReadInt32();
        byte[] data = new byte[length];
        ReadExact(data, length);
        return data;
    }

    private void ReadExact(byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = _stream.Read(buffer, offset, count - offset);
            if (read == 0) throw new EndOfStreamException();
            offset += read;
        }
    }

    public void Dispose() => _stream.Dispose();
}

// ══════════════════════════════════════════════════
//  BigEndianWriter — simule Java DataOutputStream
// ══════════════════════════════════════════════════

internal class BigEndianWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buffer = new byte[8];

    public BigEndianWriter(Stream stream) => _stream = stream;

    public void WriteByte(byte value) => _stream.WriteByte(value);

    public void WriteBoolean(bool value) => _stream.WriteByte(value ? (byte)1 : (byte)0);

    public void WriteInt32(int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(_buffer, value);
        _stream.Write(_buffer, 0, 4);
    }

    public void WriteInt64(long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(_buffer, value);
        _stream.Write(_buffer, 0, 8);
    }

    public void WriteFloat(float value)
    {
        int bits = BitConverter.SingleToInt32Bits(value);
        BinaryPrimitives.WriteInt32BigEndian(_buffer, bits);
        _stream.Write(_buffer, 0, 4);
    }

    /// <summary>
    /// Écrit une string au format Java Modified UTF-8 :
    /// 2 bytes Big Endian (longueur en bytes) + bytes UTF-8.
    /// </summary>
    public void WriteUTF(string value)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(value);
        if (strBytes.Length > ushort.MaxValue)
            throw new InvalidOperationException($"String trop longue pour WriteUTF: {strBytes.Length} bytes");
        BinaryPrimitives.WriteUInt16BigEndian(_buffer, (ushort)strBytes.Length);
        _stream.Write(_buffer, 0, 2);
        _stream.Write(strBytes, 0, strBytes.Length);
    }

    public void WriteBytes(byte[] data) => _stream.Write(data, 0, data.Length);

    public void Dispose() => _stream.Flush();
}

// ══════════════════════════════════════════════════
//  UBJSON Codec — compatible Arc/libGDX UBJsonWriter/UBJsonReader
// ══════════════════════════════════════════════════

/// <summary>
/// Encode/Decode UBJSON au format Arc (Mindustry).
/// Le format est Big Endian partout (hérité de Java DataOutputStream).
///
/// Markers UBJSON utilisés par Arc :
///   'Z' = null
///   'T' = true, 'F' = false
///   'i' = int8,  'U' = uint8
///   'I' = int16, 'l' = int32, 'L' = int64
///   'd' = float32, 'D' = float64
///   'S' = string (précédé d'un length encodé avec i/I/l)
///   'C' = char (int16)
///   '[' = array start, ']' = array end
///   '{' = object start, '}' = object end
///   '$' = type marker (optimized container), '#' = count marker
/// </summary>
public static class UBJsonCodec
{
    // ─── DECODE ──────────────────────────────────

    public static JsonElement Decode(byte[] ubjsonBytes)
    {
        using var ms = new MemoryStream(ubjsonBytes);
        using var reader = new UBJsonReader(ms);
        return reader.ReadValue();
    }

    // ─── ENCODE ──────────────────────────────────

    public static byte[] Encode(JsonElement element)
    {
        using var ms = new MemoryStream();
        using var writer = new UBJsonWriterNet(ms);
        writer.WriteValue(element);
        writer.Flush();
        return ms.ToArray();
    }
}

// ══════════════════════════════════════════════════
//  UBJsonReader — lit le UBJSON format Arc
// ══════════════════════════════════════════════════

internal class UBJsonReader : IDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buf = new byte[8];

    public UBJsonReader(Stream stream) => _stream = stream;

    public JsonElement ReadValue()
    {
        byte marker = ReadByte();
        return ReadValue(marker);
    }

    private JsonElement ReadValue(byte marker)
    {
        switch ((char)marker)
        {
            case 'Z': return JsonSerializer.SerializeToElement<object?>(null);
            case 'T': return JsonSerializer.SerializeToElement(true);
            case 'F': return JsonSerializer.SerializeToElement(false);
            case 'i': return JsonSerializer.SerializeToElement((int)ReadSByte());
            case 'U': return JsonSerializer.SerializeToElement((int)ReadUByte());
            case 'I': return JsonSerializer.SerializeToElement((int)ReadInt16());
            case 'l': return JsonSerializer.SerializeToElement(ReadInt32());
            case 'L': return JsonSerializer.SerializeToElement(ReadInt64());
            case 'd': return JsonSerializer.SerializeToElement(ReadFloat32());
            case 'D': return JsonSerializer.SerializeToElement(ReadFloat64());
            case 'C': return JsonSerializer.SerializeToElement(((char)ReadInt16()).ToString());
            case 'S': return JsonSerializer.SerializeToElement(ReadSizedString());
            case '[': return ReadArray();
            case '{': return ReadObject();
            default:
                throw new InvalidDataException($"Marqueur UBJSON inconnu: 0x{marker:X2} ('{(char)marker}')");
        }
    }

    private JsonElement ReadArray()
    {
        var list = new List<JsonElement>();

        // Vérifier optimized format: $type #count
        byte peek = ReadByte();

        if ((char)peek == '$')
        {
            // Strongly-typed array
            byte valueType = ReadByte();
            byte countMarker = ReadByte();
            if ((char)countMarker != '#')
                throw new InvalidDataException("Expected '#' after '$type' in optimized array");

            long count = ReadSizeValue();

            for (long i = 0; i < count; i++)
                list.Add(ReadValue(valueType));

            // Pas de ']' terminal pour les optimized containers
        }
        else if ((char)peek == '#')
        {
            // Counted array (no type optimization)
            long count = ReadSizeValue();
            for (long i = 0; i < count; i++)
                list.Add(ReadValue());
            // Pas de ']' terminal
        }
        else
        {
            // Standard array — lire jusqu'à ']'
            if ((char)peek != ']')
            {
                list.Add(ReadValue(peek));
                while (true)
                {
                    byte next = ReadByte();
                    if ((char)next == ']') break;
                    list.Add(ReadValue(next));
                }
            }
        }

        return JsonSerializer.SerializeToElement(list);
    }

    private JsonElement ReadObject()
    {
        var dict = new Dictionary<string, JsonElement>();

        byte peek = ReadByte();

        if ((char)peek == '$')
        {
            // Strongly-typed object
            byte valueType = ReadByte();
            byte countMarker = ReadByte();
            if ((char)countMarker != '#')
                throw new InvalidDataException("Expected '#' after '$type' in optimized object");

            long count = ReadSizeValue();
            for (long i = 0; i < count; i++)
            {
                string key = ReadObjectKey();
                dict[key] = ReadValue(valueType);
            }
        }
        else if ((char)peek == '#')
        {
            // Counted object
            long count = ReadSizeValue();
            for (long i = 0; i < count; i++)
            {
                string key = ReadObjectKey();
                dict[key] = ReadValue();
            }
        }
        else
        {
            // Standard object — lire jusqu'à '}'
            if ((char)peek != '}')
            {
                // Le premier byte lu est déjà le début de la première clé
                string firstKey = ReadObjectKeyWithFirstByte(peek);
                dict[firstKey] = ReadValue();

                while (true)
                {
                    byte next = ReadByte();
                    if ((char)next == '}') break;
                    string key = ReadObjectKeyWithFirstByte(next);
                    dict[key] = ReadValue();
                }
            }
        }

        return JsonSerializer.SerializeToElement(dict);
    }

    /// <summary>
    /// Lit une clé d'objet UBJSON.
    /// Les clés n'ont PAS de marqueur 'S' — juste un length encodé (i/I/l) + bytes.
    /// </summary>
    private string ReadObjectKey()
    {
        byte sizeType = ReadByte();
        return ReadObjectKeyWithFirstByte(sizeType);
    }

    private string ReadObjectKeyWithFirstByte(byte sizeType)
    {
        long length = ReadSizeFromMarker(sizeType);
        byte[] strBytes = new byte[length];
        ReadExact(strBytes, (int)length);
        return Encoding.UTF8.GetString(strBytes);
    }

    private string ReadSizedString()
    {
        byte sizeType = ReadByte();
        long length = ReadSizeFromMarker(sizeType);
        byte[] strBytes = new byte[length];
        ReadExact(strBytes, (int)length);
        return Encoding.UTF8.GetString(strBytes);
    }

    /// <summary>
    /// Lit un entier de taille variable servant de longueur.
    /// Appelé après avoir lu le marqueur '#'.
    /// </summary>
    private long ReadSizeValue()
    {
        byte marker = ReadByte();
        return ReadSizeFromMarker(marker);
    }

    private long ReadSizeFromMarker(byte marker)
    {
        return (char)marker switch
        {
            'i' => ReadSByte(),
            'U' => ReadUByte(),
            'I' => ReadInt16(),
            'l' => ReadInt32(),
            'L' => ReadInt64(),
            _ => throw new InvalidDataException($"Type de taille UBJSON invalide: '{(char)marker}'")
        };
    }

    // ─── Primitives (Big Endian) ─────────────────

    private byte ReadByte()
    {
        int b = _stream.ReadByte();
        if (b == -1) throw new EndOfStreamException();
        return (byte)b;
    }

    private sbyte ReadSByte() => (sbyte)ReadByte();

    private int ReadUByte() => ReadByte() & 0xFF;

    private short ReadInt16()
    {
        ReadExact(_buf, 2);
        return BinaryPrimitives.ReadInt16BigEndian(_buf);
    }

    private int ReadInt32()
    {
        ReadExact(_buf, 4);
        return BinaryPrimitives.ReadInt32BigEndian(_buf);
    }

    private long ReadInt64()
    {
        ReadExact(_buf, 8);
        return BinaryPrimitives.ReadInt64BigEndian(_buf);
    }

    private float ReadFloat32()
    {
        ReadExact(_buf, 4);
        int bits = BinaryPrimitives.ReadInt32BigEndian(_buf);
        return BitConverter.Int32BitsToSingle(bits);
    }

    private double ReadFloat64()
    {
        ReadExact(_buf, 8);
        long bits = BinaryPrimitives.ReadInt64BigEndian(_buf);
        return BitConverter.Int64BitsToDouble(bits);
    }

    private void ReadExact(byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = _stream.Read(buffer, offset, count - offset);
            if (read == 0) throw new EndOfStreamException();
            offset += read;
        }
    }

    public void Dispose() => _stream.Dispose();
}

// ══════════════════════════════════════════════════
//  UBJsonWriterNet — écrit du UBJSON format Arc
// ══════════════════════════════════════════════════

internal class UBJsonWriterNet : IDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buf = new byte[8];

    public UBJsonWriterNet(Stream stream) => _stream = stream;

    public void WriteValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                _stream.WriteByte((byte)'Z');
                break;

            case JsonValueKind.True:
                _stream.WriteByte((byte)'T');
                break;

            case JsonValueKind.False:
                _stream.WriteByte((byte)'F');
                break;

            case JsonValueKind.Number:
                WriteNumber(element);
                break;

            case JsonValueKind.String:
                WriteString(element.GetString()!);
                break;

            case JsonValueKind.Array:
                WriteArray(element);
                break;

            case JsonValueKind.Object:
                WriteObject(element);
                break;
        }
    }

    private void WriteNumber(JsonElement element)
    {
        // Essayer int d'abord, puis long, puis double
        if (element.TryGetInt32(out int i))
        {
            // Choisir le plus petit type comme Arc le fait
            if (i >= sbyte.MinValue && i <= sbyte.MaxValue)
            {
                _stream.WriteByte((byte)'i');
                _stream.WriteByte((byte)(sbyte)i);
            }
            else if (i >= short.MinValue && i <= short.MaxValue)
            {
                _stream.WriteByte((byte)'I');
                WriteInt16BE((short)i);
            }
            else
            {
                _stream.WriteByte((byte)'l');
                WriteInt32BE(i);
            }
        }
        else if (element.TryGetInt64(out long l))
        {
            _stream.WriteByte((byte)'L');
            WriteInt64BE(l);
        }
        else if (element.TryGetDouble(out double d))
        {
            // Si c'est un float "exact", écrire en float32
            float f = (float)d;
            if (Math.Abs(d - f) < double.Epsilon && !double.IsInfinity(d))
            {
                _stream.WriteByte((byte)'d');
                WriteFloat32BE(f);
            }
            else
            {
                _stream.WriteByte((byte)'D');
                WriteFloat64BE(d);
            }
        }
    }

    private void WriteString(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        _stream.WriteByte((byte)'S');
        WriteSizedLength(bytes.Length);
        _stream.Write(bytes, 0, bytes.Length);
    }

    private void WriteArray(JsonElement element)
    {
        _stream.WriteByte((byte)'[');
        foreach (var item in element.EnumerateArray())
            WriteValue(item);
        _stream.WriteByte((byte)']');
    }

    private void WriteObject(JsonElement element)
    {
        _stream.WriteByte((byte)'{');
        foreach (var prop in element.EnumerateObject())
        {
            // Écrire le nom (sans marqueur 'S', juste length + bytes)
            byte[] keyBytes = Encoding.UTF8.GetBytes(prop.Name);
            WriteSizedLength(keyBytes.Length);
            _stream.Write(keyBytes, 0, keyBytes.Length);

            // Écrire la valeur
            WriteValue(prop.Value);
        }
        _stream.WriteByte((byte)'}');
    }

    /// <summary>
    /// Écrit une longueur avec le plus petit type possible (i/I/l),
    /// exactement comme Arc UBJsonWriter.name() et .value(String).
    /// </summary>
    private void WriteSizedLength(int length)
    {
        if (length <= sbyte.MaxValue)
        {
            _stream.WriteByte((byte)'i');
            _stream.WriteByte((byte)(sbyte)length);
        }
        else if (length <= short.MaxValue)
        {
            _stream.WriteByte((byte)'I');
            WriteInt16BE((short)length);
        }
        else
        {
            _stream.WriteByte((byte)'l');
            WriteInt32BE(length);
        }
    }

    // ─── Primitives (Big Endian) ─────────────────

    private void WriteInt16BE(short value)
    {
        BinaryPrimitives.WriteInt16BigEndian(_buf, value);
        _stream.Write(_buf, 0, 2);
    }

    private void WriteInt32BE(int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(_buf, value);
        _stream.Write(_buf, 0, 4);
    }

    private void WriteInt64BE(long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(_buf, value);
        _stream.Write(_buf, 0, 8);
    }

    private void WriteFloat32BE(float value)
    {
        int bits = BitConverter.SingleToInt32Bits(value);
        BinaryPrimitives.WriteInt32BigEndian(_buf, bits);
        _stream.Write(_buf, 0, 4);
    }

    private void WriteFloat64BE(double value)
    {
        long bits = BitConverter.DoubleToInt64Bits(value);
        BinaryPrimitives.WriteInt64BigEndian(_buf, bits);
        _stream.Write(_buf, 0, 8);
    }

    public void Flush() => _stream.Flush();
    public void Dispose() => _stream.Flush();
}