using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace sh4bin
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <= 2 && args.Length >= 3)
            {
                Console.WriteLine("sh4bin\nA tool for unpacking and repacking Silent Hill 4 .bin files into their chunks.\nThis does *NOT* extract textures, sounds, anims, etc. It simply splits .bin files into the chunks they contain - further tools would be needed to edit the textures/models/etc.\nUsage:\nsh4bin <file.bin> <output directory> - Unpacks a .bin file into chunks into the specified output directory\nsh4bin <output directory> <file.bin> - Packs bin file chunks into the specified .bin file\nsh4bin analyze <file.bin> - Analyzes a .bin file and tells you information about it");
                return;
            }

            if (args[0] == "analyze")
            {

                // Open a filestream with the user selected file
                FileStream file = new FileStream(args[1], FileMode.Open);

                // Create a binary reader that will be used to read the file
                BinaryReader reader = new BinaryReader(file);

                // Grab the number of files inside the .bin
                int fileCount = reader.ReadInt32();

                Console.WriteLine("Number of files in .bin: " + fileCount);

                for (int i = 0;i < fileCount;i++)
                {
                    int offset = reader.ReadInt32();
                    Console.WriteLine("Bin chunk file offset: " + offset.ToString("X"));
                    // Find all possible bin chunk types and figure out how to ID them
                    // Console.WriteLine("Bin chunk type: " + 0);
                }

                
            }

            if (args[0] == "unpack")
            {
                // Open a filestream with the user selected file
                FileStream file = new FileStream(args[1], FileMode.Open);

                // Create a binary reader that will be used to read the file
                BinaryReader reader = new BinaryReader(file);

                // Grab the number of files inside the .bin
                int fileCount = reader.ReadInt32();

                Console.WriteLine("Number of files in .bin: " + fileCount);

                for (int i = 0; i < fileCount; i++)
                {
                    int offset = reader.ReadInt32();
                    Console.WriteLine("Bin chunk file offset: " + offset.ToString("X"));

                    // Save the original position of the reader so we can reset it at the end
                    long origPos = reader.BaseStream.Position;

                    // Read the offset of the next file
                    int nextFileOffset = reader.ReadInt32();

                    // Advance the stream to the file's offset
                    reader.BaseStream.Position = offset;

                    if (nextFileOffset != 0)
                    {
                        var filePath = i + ".chunk";

                        // Attempt to guess what kind of chunk type it is
                        // This is pretty fucking rough but there's no real other way to do this besides have a chunk dictionary which would take *forever* to create

                        // Store the original position so we can return to the chunk after attempting to read it's "magic"
                        long originalPosition = reader.BaseStream.Position;

                        // Read the "magics"
                        // The second magic will always match the first if the file is texture data, because the two magics in that case aren't actually magics but the texture and palette count
                        // However this does not seem to always be the case, but it is in ~90% of scenarios, good enough
                        uint magic = reader.ReadUInt16();
                        uint magic2 = reader.ReadUInt16();

                        if (magic == magic2)
                        {
                            filePath = i + ".textures";
                            Console.WriteLine("Chunk type: Textures");
                        }
                        else
                        {
                            switch (magic)
                            {
                                // Unknown data, looks like 3d coordinates though
                                case 0xFF11:
                                    filePath = i + ".coll_mesh";
                                    Console.WriteLine("Chunk type: World collision mesh");
                                    break;
                                // Model data always starts with 0x0003
                                case 0x0003:
                                    filePath = i + ".mesh";
                                    Console.WriteLine("Chunk type: 3D Mesh data");
                                    break;
                                // SDB files embedded in a .bin
                                case 0x8581:
                                    filePath = i + ".sdb";
                                    Console.WriteLine("Chunk type: .SDB file");
                                    break;
                                // List of monster internal IDs
                                case 0x4554:
                                    filePath = i + ".monsterIDList";
                                    Console.WriteLine("Chunk type: Monster ID list");
                                    break;
                                // Chunk with SLGT magic, not sure what this controls
                                case 0x4C53:
                                    filePath = i + ".slgt";
                                    Console.WriteLine("Chunk type: SLGT file");
                                    break;
                                // Check if the second magic is also 0xFF01 which indicates animation data
                                // If not just jump to unknown chunk
                                case 0x0001:
                                    if (magic2 == 0xFF01)
                                    {
                                        filePath = i + ".anim";
                                        Console.WriteLine("Chunk type: Animation");
                                        break;
                                    }
                                    if (magic2 == 0xFC03)
                                    {
                                        filePath = i + ".world_mesh";
                                        Console.WriteLine("Chunk type: World 3D Mesh data");
                                        break;
                                    }
                                    goto default;
                                // No magic found
                                default:
                                    Console.WriteLine("Chunk type: Unknown");
                                    break;
                            }
                        }
                        // Return to the beginning of the chunk
                        reader.BaseStream.Position = originalPosition;

                        // Attempt to read from the current file until the next one begins
                        Directory.CreateDirectory(args[2]);
                        File.WriteAllBytes(args[2]+"/"+filePath, reader.ReadBytes(nextFileOffset - offset));

                        // Print a new line so the output is easier to read at a glance
                        Console.WriteLine();
                    }
                    else
                    {
                        var filePath = i + ".chunk";

                        // Attempt to guess what kind of chunk type it is
                        // This is pretty fucking rough but there's no real other way to do this besides have a chunk dictionary which would take *forever* to create

                        // Store the original position so we can return to the chunk after attempting to read it's "magic"
                        long originalPosition = reader.BaseStream.Position;

                        // Read the "magics"
                        // The second magic will always match the first if the file is texture data, because the two magics in that case aren't actually magics but the texture and palette count
                        // However this does not seem to always be the case, but it is in ~90% of scenarios, good enough
                        uint magic = reader.ReadUInt16();
                        uint magic2 = reader.ReadUInt16();

                        if (magic == magic2)
                        {
                            filePath = i + ".textures";
                            Console.WriteLine("Chunk type: Textures");
                        }
                        else
                        {
                            switch (magic)
                            {
                                // Unknown data, looks like 3d coordinates though
                                case 0xFF11:
                                    filePath = i + ".coll_mesh";
                                    Console.WriteLine("Chunk type: World collision mesh");
                                    break;
                                // Model data always starts with 0x0003
                                case 0x0003:
                                    filePath = i + ".mesh";
                                    Console.WriteLine("Chunk type: 3D Mesh data");
                                    break;
                                // SDB files embedded in a .bin
                                case 0x8581:
                                    filePath = i + ".sdb";
                                    Console.WriteLine("Chunk type: .SDB file");
                                    break;
                                // List of monster internal IDs
                                case 0x4554:
                                    filePath = i + ".monsterIDList";
                                    Console.WriteLine("Chunk type: Monster ID list");
                                    break;
                                // Chunk with SLGT magic, not sure what this controls
                                case 0x4C53:
                                    filePath = i + ".slgt";
                                    Console.WriteLine("Chunk type: SLGT file");
                                    break;
                                // Check if the second magic is also 0xFF01 which indicates animation data
                                // If not just jump to unknown chunk
                                case 0x0001:
                                    if (magic2 == 0xFF01)
                                    {
                                        filePath = i + ".anim";
                                        Console.WriteLine("Chunk type: Animation");
                                        break;
                                    }
                                    if (magic2 == 0xFC03)
                                    {
                                        filePath = i + ".world_mesh";
                                        Console.WriteLine("Chunk type: World 3D Mesh data");
                                        break;
                                    }
                                    goto default;
                                // No magic found
                                default:
                                    Console.WriteLine("Chunk type: Unknown");
                                    break;
                            }
                        }

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
                // Create a new file
                FileStream file = new FileStream(args[2], FileMode.Create);

                // Create a binary writer that will write the new bin file
                BinaryWriter writer = new BinaryWriter(file);

                // Get the files inside the output directory
                string[] files = Directory.GetFiles(args[1]);

                var sortedFiles = files.CustomSort().ToArray();

                // Grab the number of files inside the user's output directory
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
                foreach(string inputFile in sortedFiles)
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
