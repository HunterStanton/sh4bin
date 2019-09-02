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
            if (args.Length != 3)
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

                    var filePath = i + ".chunk";

                    if (nextFileOffset != 0)
                    {
                        // Attempt to read from the current file until the next one begins
                        Directory.CreateDirectory(args[2]);
                        File.WriteAllBytes(args[2]+"/"+filePath, reader.ReadBytes(nextFileOffset - offset));
                    }
                    else
                    {
                        // Read to EOF since we're at the final file
                        Directory.CreateDirectory(args[2]);
                        File.WriteAllBytes(args[2] + "/" + filePath, reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length) - offset));
                    }

                    // Reset base stream position
                    reader.BaseStream.Position = origPos;
                }
            }

            if (args[0] == "pack")
            {
                // Create a new file
                FileStream file = new FileStream(args[2], FileMode.Create);

                // Create a binary writer that will write the new bin file
                BinaryWriter writer = new BinaryWriter(file);

                // Get the files inside the output directory
                string[] files = Directory.GetFiles(args[1]);

                // Grab the number of files inside the user's output directory
                int fileCount = files.Length;

                Console.WriteLine("Number of files in new .bin: " + fileCount);

                List<byte> binBody = new List<byte>();

                writer.Write(fileCount);

                // Leave enough space for 1024 files
                // TODO: Figure out why the game crashes when using any repacked bin file
                int tempLength = 0x400;

                // Loop through every bin chunk in the output directory and build a bin file from it
                foreach(string inputFile in files)
                {
                    // Write the file's offset into the new header
                    long length = new System.IO.FileInfo(inputFile).Length;

                    if (inputFile != files.First())
                    {
                        tempLength = Convert.ToInt32(length + tempLength);
                    }

                    // Write the offset of the file to the bin header
                    writer.Write(tempLength);

                    // Append the current bin chunk to the bin body
                    binBody.AddRange(File.ReadAllBytes(inputFile));

                }

                // Append extra bytes to pad the bin header to 0x2000
                writer.Write(new byte[0x400 - writer.BaseStream.Length]);

                // Write the bin body to the bin file
                writer.Write(binBody.ToArray());

                // Close the binary writer
                writer.Close();

                Console.WriteLine(args[2] + " successfully created!");
            }
        }
    }
}
