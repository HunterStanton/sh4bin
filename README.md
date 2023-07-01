# sh4bin

A tool to unpack/repack Silent Hill 4 .bin files. sh4bin is capable of perfectly unpacking and repacking all Silent Hill 4 bin files successfully.

**This is a low-level tool. If you are not familiar with the command line and a hex editor, you will not be able to use this tool for anything meaningful.**

## Building

sh4bin uses .NET 6.0 and as such it can be built for any platform and architecture that is supported by .NET 6.0. Just run `dotnet build` to compile it.

## Usage

* `sh4bin pack < input directory> <file.bin>` - Packs a bin file from loose files in a folder
* `sh4bin unpack <file.bin> < output directory>` - Unpacks a bin into an output directory
* `sh4bin analyze <file.bin>` - Analyzes a Silent Hill 4 bin file and tells you information about it without unpacking

## Chunk Type Fingerprinting

Silent Hill 4 .bin files do not include any naming information, so it is difficult to tell what things are beyond the filename of the .bin itself.

That being said, sh4bin is capable of fingerprinting .bin chunk types automatically when they are extracted. For example, it can identify the following chunk types with a high level of accuracy:

* Texture chunks
* Object mesh chunks
* World mesh chunks
* Collision mesh chunks
* .sdb File chunks
* SLGT chunks
* Animation chunks
* Shadow mesh chunks

It does this by looking for magic bytes to identify the type of data that is in the chunk.

# 

## Compatibility

This tool correctly unpacks and repacks .bin files from all Silent Hill 4 versions on all platforms.

## Credits

* Hunter Stanton (@hun10sta)
