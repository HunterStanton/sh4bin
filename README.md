
# sh4bin
A tool to unpack/repack Silent Hill 4 .bin files. Is able to repack and unpack all Silent Hill 4 bin files successfully, however they do not currently load in-game and just crash.

## Usage
* sh4bin.exe pack < input directory> <file.bin> - Packs a bin file from loose files in a folder
* sh4bin.exe unpack <file.bin> < output directory> - Unpacks a bin into an output directory
* sh4bin.exe analyze <file.bin> - Analyzes a Silent Hill 4 bin file and tells you information about it

Silent Hill 4 .bin files are very devoid of information regarding naming, so it is difficult to tell what things are (the chunks have no name anywhere, they are simply referenced by some sort of index in the executable when they are needed) so keep this in mind when you unpack a .bin. However, the file extension that is provided by fingerprinting when unpacking can shed a bit of light on what type of data is contained in the chunk.

## Chunk Type Fingerprinting
This utility is capable of fingerprinting .bin chunk types automatically when they are extracted. For example, it can identify the following (and more) chunk types with a decent level of accuracy:
* Texture chunks
* 3D Mesh Data
* .sdb File chunks
* SLGT chunks
* Animation chunks

This is based on a "magic" that holds true for the majority of the .bin chunk types, however some fall through the cracks and won't get identified properly. I'm working to eventually be able to accurately identify every possible chunk type, but this will take some time to manually verify everything. For now though, it should work and provide a bit more insight into what the .bin file chunks actually are for further research and analysis or modification.

## To-Do
### Game Crash When Repacking Bins
Figure out if there is some sort of offset directory or table somewhere, because *all* repacked bins currently crash the game due to the way that offsets are calculated. If this cannot be overcome, then I will eventually create a "magic" file in the output directory that tells sh4bin how to set the offsets up as they were in the original bin. This way, it should be possible to actually *use* the repacked bin files. Right now the game just crashes, even though the bins it creates are absolutely correct in a technical sense.

### Possible Future Improvements
The code could use some optimizations and could be simplified by someone who is actually good at C#. Swift is my wheelhouse, I haven't written much C# in a long time.

### Error Handling
Right now if sh4bin cannot get a handle on a file (such as if it is being used) it will throw an exception and crash. In the future I would like to add error handling so that the user isn't left to parse an exception to figure out what went wrong.

## Compatibility
This tool correctly unpacks .bin files from all Silent Hill 4 versions (PC, XBox, and PS2). Repacking will also technically work, but will crash the game on all versions due to the problem mentioned above. 

## Credits
* Hunter Stanton (@hunterstanton)
