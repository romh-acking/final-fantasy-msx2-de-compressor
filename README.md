# Final Fantasy MSX2: (De)Compressor Tool
## About

This is a decompression and compression tool for `.PIC` files in Final Fantasy for the MSX2.

## Technical
The compression for this game has is fairly odd. The scheme has write and fill behaviors, which is theoretically simple enough. When the algorithm first starts, it checks the first two bytes. If they're equal then is writes them to the decompression buffer `YY` times; `YY` being the value of the next byte. If the bytes are not equal, it just writes the first byte and the moves on to the next byte. The algorithm continues this process until it's done. Here's a breakdown of how this works:
* `XX XX YY` = Fill
* `XX` = Write byte

Conceptually, this algorithm is already quite flawed. Most algorithms have a byte that identifies what type of action the algorithm should take (if it's a fill or a write). I'm also oversimplifying the process for brevity sake.

There's other factors to keep in mind, such as the header. The header for the `.PIC` files is `0x30` bytes long: the last `0x20` is palette data, and the first `0x10` bytes I have not fully reverse engineered. Key values are the length and width size. In the decompression buffer, if the width is filled, then the remaining pixels are padded out. As for length, I couldn't figure out how it worked. As a result, when dumping data, a length size attribute must be specified.

While my explanation sounds reasonable enough, the implementation is a different story. To get the compression and decompression working, I had to essentially write C# code that looks and functions like assembly code to match the game's original Z80 assembly language code. There's a few edge cases that forced me to do so that don't make apparent sense. Once I got the something that worked, I ran away as fast as I could.

## Usage
See below for terminal usage information.

### Dumping
```bash
"FF MSX GFX.exe" "Dump" "%AddressOffset%" "%InputPathBaseGfxCompressed%" "%OutputPathBaseGfxUncompressed%"
```

* `AddressOffset`: Specify where in the binary file the .PIC file starts. If you're working off an extracted image set this to `"0x00"`
* `InputPathBaseGfxCompressed`: Path of the compressed image / binary file
* `OutputPathBaseGfxUncompressed`: Where to write the uncompressed data. The file must already exist as this is where the header data is retrieved from.

### Writing

```bash
"FF MSX GFX.exe" "Write" "%OutputPathGfxCompressed%" "%InputPathBaseGfxCompressed%"
```
* `OutputPathGfxCompressed`: The file path where compressed date will be output to
* `InputPathBaseGfxCompressed`: This in the path of the original .PIC file. It is needed for the header information.

## Special thanks
* TWE#3544