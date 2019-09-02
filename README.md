
# sh4bin
A tool to unpack/repack Silent Hill 4 .bin files. Is able to repack and unpack all Silent Hill 4 bin files successfully, however they do not currently load in-game and just crash.

## Usage
* sh4bin.exe pack < input directory> <file.bin> - Packs a bin file from loose files in a folder
* sh4bin.exe unpack <file.bin> < output directory> - Unpacks a bin into an output directory
* sh4bin.exe analyze <file.bin> - Analyzes a Silent Hill 4 bin file and tells you information about it

## To-Do
### Game Crash When Repacking Bins
Figure out if there is some sort of offset directory or table somewhere, because *all* repacked bins currently crash the game due to the way that offsets are calculated. If this cannot be overcome, then I will eventually create a "magic" file in the output directory that tells sh4bin how to set the offsets up as they were in the original bin. This way, it should be possible to actually *use* the repacked bin files. Right now the game just crashes, even though the bins it creates are absolutely correct in a technical sense.

### Possible Future Improvements
The code needs a major cleanup and could be simplified by someone who is actually good at C#. Swift is my wheelhouse, I haven't written much C# in a long time.

### Detection of Chunk Types
Right now, the sh4bin just spits out all chunks as binary files. I'd like for it to eventually be able to detect the type of chunk each chunk is (whether it's the meshes, textures, etc.) and name files accordingly.

## Compatibility
This has only been tested on Silent Hill 4 PC. However, it should technically work on the PS2 and Xbox versions, because they are the same endianness as the PC port. If someone could verify this, I'd be happy to amend the readme to indicate compatibility with Xbox and PS2.

## Credits
* Hunter Stanton (@hunterstanton)