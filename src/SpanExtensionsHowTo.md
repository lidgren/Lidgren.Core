
# How to use SpanExtensions
Extension methods on Span<byte> and ReadOnlySpan<byte> for serialializing numbers and strings

### Writing

```
var arr = new byte[128];
var span = arr.AsSpan();

span.WriteUInt32(127);
span.WriteVariable(17);
span.WriteBool(true);

var result = arr.AsSpan(0, arr.Length - span.Length);
```

### Reading
```
// (continued from above)
ReadOnlySpan<byte> rdr = arr.AsSpan(); // not Span<byte>

uint val1 = rdr.ReadUInt32();
ulong val2 = rdr.ReadVariableUInt64();
bool val3 = rdr.ReadBool();
```
