using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace sh4bin
{
    class Program
    {
        static ChunkType IdentifyChunkType(uint magic, uint magic2)
        {
            ChunkType info = new ChunkType();

            if (magic == magic2)
            {
                info.Extension = ".textures";
                info.Name = "Textures";
            }
            else
            {
                switch (magic)
                {
                    case 0x7000:
                        if (magic2 == 0x0FC0)
                        {
                            info.Extension = ".shadow_mesh";
                            info.Name = "Shadow mesh";
                            break;
                        }
                        goto default;
                    case 0xFF11:
                        info.Extension = ".coll_mesh";
                        info.Name = "World collision mesh";
                        break;
                    case 0x0003:
                        info.Extension = ".mesh";
                        info.Name = "Object 3D mesh";
                        break;
                    case 0x8581:
                        info.Extension = ".sdb";
                        info.Name = ".SDB file";
                        break;
                    case 0x4554:
                        info.Extension = ".monsterIDList";
                        info.Name = "Monster ID list (unused by the game)";
                        break;
                    case 0x4C53:
                        info.Extension = ".slgt";
                        info.Name = "SLGT file";
                        break;
                    case 0x0001:
                        if (magic2 == 0xFF01)
                        {
                            info.Extension = ".anim";
                            info.Name = "Animation";
                            break;
                        }
                        if (magic2 == 0xFC03)
                        {
                            info.Extension = ".world_mesh";
                            info.Name = "World 3D Mesh";
                            break;
                        }
                        goto default;
                    default:
                        info.Extension = ".chunk";
                        info.Name = "Unknown chunk type";
                        break;
                }
            }

            return info;
        }

        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("sh4bin - A tool for unpacking and repacking Silent Hill 4 .bin files into their chunks." +
                                    "\n-------------------------------------" +
                                    "\nsh4bin unpack <file.bin> <output directory> - Unpacks a .bin file into chunks into the specified output directory" +
                                    "\nsh4bin pack <output directory> <file.bin> - Packs .bin file chunks in a directory into the specified .bin file" +
                                    "\nsh4bin analyze <file.bin> - Analyzes a .bin file and tells you information about it");
                return;
            }

            if (args[0] == "analyze")
            {

                FileStream file = new FileStream(args[1], FileMode.Open);

                BinaryReader reader = new BinaryReader(file);

                int fileCount = reader.ReadInt32();

                Console.WriteLine("Number of files in .bin: " + fileCount);
                Console.WriteLine("--------------------------------------");

                for (int i = 0; i < fileCount; i++)
                {
                    int offset = reader.ReadInt32();
                    Console.WriteLine("Bin chunk file offset: " + offset.ToString("X"));

                    // seek to offset and identify chunk type
                    long start = reader.BaseStream.Position;
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    ChunkType info = IdentifyChunkType(reader.ReadUInt16(), reader.ReadUInt16());
                    Console.WriteLine("Bin chunk type: " + info.Name);
                    Console.WriteLine();
                    reader.BaseStream.Position = start;
                }
            }

            if (args[0] == "unpack")
            {
                FileStream file = new FileStream(args[1], FileMode.Open);

                BinaryReader reader = new BinaryReader(file);

                int fileCount = reader.ReadInt32();

                Console.WriteLine("Number of files in .bin: " + fileCount);
                Console.WriteLine("--------------------------------------");

                for (int i = 0; i < fileCount; i++)
                {
                    int offset = reader.ReadInt32();
                    Console.WriteLine("Bin chunk file offset: 0x" + offset.ToString("X"));

                    long origPos = reader.BaseStream.Position;

                    int nextFileOffset = reader.ReadInt32();

                    reader.BaseStream.Position = offset;

                    if (nextFileOffset != 0)
                    {
                        var filePath = i + ".chunk";

                        // Store the original position so we can return to the chunk after attempting to read it's "magic"
                        long originalPosition = reader.BaseStream.Position;

                        ChunkType info = IdentifyChunkType(reader.ReadUInt16(), reader.ReadUInt16());
                        filePath = i + info.Extension;
                        Console.WriteLine("Chunk type: " + info.Name);

                        // Return to the beginning of the chunk
                        reader.BaseStream.Position = originalPosition;

                        // Attempt to read from the current file until the next one begins
                        Directory.CreateDirectory(args[2]);
                        File.WriteAllBytes(args[2] + "/" + filePath, reader.ReadBytes(nextFileOffset - offset));

                        // Print a new line so the output is easier to read at a glance
                        Console.WriteLine();
                    }
                    else
                    {
                        var filePath = i + ".chunk";

                        // Store the original position so we can return to the chunk after attempting to read it's "magic"
                        long originalPosition = reader.BaseStream.Position;

                        ChunkType info = IdentifyChunkType(reader.ReadUInt16(), reader.ReadUInt16());
                        filePath = i + info.Extension;
                        Console.WriteLine("Chunk type: " + info.Name);

                        // Return to the beginning of the chunk
                        reader.BaseStream.Position = originalPosition;

                        // Read to EOF since we're at the final file
                        Directory.CreateDirectory(args[2]);
                        File.WriteAllBytes(args[2] + "/" + filePath, reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length) - offset));

                        // Print a new line so the output is easier to read at a glance
                        Console.WriteLine();
                    }

                    // Reset base stream position
                    reader.BaseStream.Position = origPos;
                }

                // Close the reader and the file because we are done using the files
                reader.Close();
                file.Close();
            }

            if (args[0] == "pack")
            {
                FileStream file = new FileStream(args[2], FileMode.Create);

                BinaryWriter writer = new BinaryWriter(file);

                string[] files = Directory.GetFiles(args[1]);

                var sortedFiles = files.CustomSort().ToArray();

                int fileCount = sortedFiles.Length;

                Console.WriteLine("Number of files in new .bin: " + fileCount);

                List<byte> binBody = new List<byte>();

                writer.Write(fileCount);

                // The game seems to have a couple of "safe" values for how much padding is in the header before things go wrong
                // For example, using 0x800 (which allows for 512 chunks in a bin, way more than any bin the game normally uses) stuff will load, but things will break and probably crash eventually
                // Henry's cutscene model for example looks and loads great, but his shadows go wonky and it will crash in random cutscenes, so we want to avoid this at all costs by using a set of if statements to determine what padding to give the header based the original bins

                int padding = 0x0;

                // Determine what padding to use depending on how many chunks are in the bin
                if (fileCount < 0x1F)
                {
                    padding = 0x80;
                }

                if (fileCount > 0x1F && fileCount < 0x3F)
                {
                    padding = 0x100;
                }

                if (fileCount > 0x3F && fileCount < 0x5F)
                {
                    padding = 0x180;
                }

                if (fileCount > 0x5F && fileCount < 0x7F)
                {
                    padding = 0x200;
                }

                // This one doesn't seem to be used by any existing bins, but we'll put it here anyway
                if (fileCount > 0x7F && fileCount < 0x9F)
                {
                    padding = 0x280;
                }

                if (fileCount > 0x9F && fileCount < 0xBF)
                {
                    padding = 0x300;
                }

                int tempLength = padding + 0x0;
                int previousLength = 0;

                // Loop through every bin chunk in the output directory and build a new bin file from it
                foreach (string inputFile in sortedFiles)
                {
                    // Write the file's offset into the new header
                    long length = new System.IO.FileInfo(inputFile).Length;

                    // Write the offset of the current file to the bin header
                    writer.Write(tempLength + previousLength);

                    previousLength = previousLength + Convert.ToInt32(length);


                    // Append the current bin chunk to the bin body
                    binBody.AddRange(File.ReadAllBytes(inputFile));

                }

                // Append extra bytes to pad the bin header
                writer.Write(new byte[padding - writer.BaseStream.Length]);

                // Write the bin body to the bin file
                writer.Write(binBody.ToArray());

                // Close the binary writer and file
                writer.Close();
                file.Close();

                Console.WriteLine(args[2] + " successfully created!");
            }
        }
    }
}
