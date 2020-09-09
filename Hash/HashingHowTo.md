
# How to use hashing

### General hashing

```
var hasher = Hasher.Create();
hasher.Add("hello");
hasher.Add(42);
hasher.Add(myByteArray);
var result = hasher.Finalize64(); // for a 64 bit hash of added data
```

### Hashing files

```
var result = FileHash.Hash("myFile.txt");
```

### Hashing small bits of data
```
var result = HashUtil.Hash64("someString"); // internally creates a Hasher and adds it
```
