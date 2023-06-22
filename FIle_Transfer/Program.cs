using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace File_Transfer
{
    internal class Program
    {
        private readonly ILogger _logger;
        /*
         * A simple program to copy over files from multiple sources into a single destination. 
         * This was made to make life easier when backing up my files over to my back-up drive.
         * It iteratively calls TransferDirectory to make sure we capture all source directories read in
         * except for the dest. folder.
         * TODO: add statistics for the end of the program
         */
        static int Main(string[] args)
        {
            //potentially set up verbose option as well to show the files being transferred
            //TODO: find logging library
            Option<bool> compressOption = new Option<bool>(
                    aliases: new string[] { "-c", "--compress" },
                    description: "decrease destination directory size",
                    getDefaultValue: () => false
            );
            Option<List<string>> sourceDirectories = new Option<List<string>>(
                    aliases: new string[] { "-f", "--files" },
                    description: "directories that are being transferred"
            )
            { AllowMultipleArgumentsPerToken = true, IsRequired = true };

            Option<string> destinationOption = new Option<string>(
                    aliases: new string[] { "-d", "--destination" },
                    description: "directory that files are being transferred to"
            )
            { IsRequired = true };
            
            Option<bool> statsOption = new Option<bool>(
                    aliases: new string[] { "-s", "--stats" },
                    description: "provides statistics on the files being transferred",
                    getDefaultValue: () => false
            );
            Option<int> compressLevel = new Option<int>(
                    aliases: new string[] { "-l", "--compress-level" },
                    description: "provides the level of compression. Link: https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.compressionlevel?view=net-6.0",
                    getDefaultValue: () => 1
            );

            var rootCommand = new RootCommand(description: "File Transferring program")
            {
                compressOption, sourceDirectories, destinationOption, statsOption
            };


            rootCommand.SetHandler(
                (bool stats, bool compress, List<string> sourceDirectories, string destination, int compressLevel) =>
                {
                    _logger.LogInformation("transfer starting");

                    Stats? s = null;
                    if (stats)
                    {
                        s = new Stats();
                        s.Time.Start();
                    }

                    foreach(string directory in sourceDirectories)
                    {
                        /*
                         * Both the parallel for and the normal for loop ended up performing about the same. Would probably need to test this on an ssd since
                         * initial testing is on an external hard drive, but in general, these performed within seconds of each other.
                         * After testing on an nvme ssd, they both completed within a second of each other.
             
                        Parallel.For(0, args.Length - 1, i =>
                        {
                            FileInfo srcFile = new FileInfo(args[i]);
                            var newDirInfo = Directory.CreateDirectory(args[args.Length - 1] + "\\" + srcFile.Name);
                            TransferDirectory(args[i], newDirInfo.FullName);
                        });
                        */
                        FileInfo srcDir = new FileInfo(directory);
                        var newDirInfo = Directory.CreateDirectory(destination + "\\" + srcDir.Name);

                        //send stats object into here
                        TransferDirectory(directory, newDirInfo.FullName, ref s);
                    }

                    if (compress)
                    {
                        Console.WriteLine("started compression");
                        CompressionLevel lvl = (CompressionLevel)compressLevel;

                        string compressedDirectory = destination + "_compressed.zip";
                        try
                        {
                            ZipFile.CreateFromDirectory(destination, compressedDirectory, lvl, true);
                        }
                        catch (IOException)
                        {
                            bool directoryMade = false;

                            while(!directoryMade)
                            {
                                Console.Write("{0} already exists, do you want to overwrite? (Y/N): ", compressedDirectory);
                                string? input = Console.ReadLine();
                                switch (input)
                                {
                                    case "Y":
                                        File.Delete(compressedDirectory);
                                        ZipFile.CreateFromDirectory(destination, compressedDirectory, lvl, true);
                                        directoryMade = true;
                                        break;
                                    case "N":
                                        directoryMade = true;
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect input (Type Y or N)");
                                        break;
                                }
                            }
                            

                        }
                        Console.WriteLine("started deletion");
                        //remove the destination directory once the zip file has been created
                        Directory.Delete(destination, true);

                        if(s != null)
                        {
                            FileInfo compressedInfo = new FileInfo(compressedDirectory);
                            s.CompressedSize = compressedInfo.Length;
                        }
                    }
                   

                    if (s != null)
                    {
                        s.Time.Stop();
                        Console.WriteLine("File_Transfer was successful");
                        Console.WriteLine(s.ToString());
                    }
                   
                },
                statsOption,
                compressOption,
                sourceDirectories,
                destinationOption,
                compressLevel
            );

            return rootCommand.InvokeAsync(args).Result;
        }

        /*
         * A recursive function that copies all the files and subdirectories within a directory. 
         * This will skip over any files that already exist in the directory
         * 
         */
        static void TransferDirectory(string sourceDirectory, string destinationDirectory, ref Stats? stats)
        {
            try
            {
                var directory = new DirectoryInfo(sourceDirectory);
                DirectoryInfo[] dirs = directory.GetDirectories();

                Directory.CreateDirectory(destinationDirectory);

                foreach (FileInfo file in directory.GetFiles())
                {
                    try
                    {
                        if (stats != null)
                        {
                            stats.SizeTransferred += file.Length;
                            stats.FilesTransferred++;
                        }

                        string targetFilePath = Path.Combine(destinationDirectory, file.Name);
                        file.CopyTo(targetFilePath);
                    }
                    catch (IOException e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                }

                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDirectory = Path.Combine(destinationDirectory, subDir.Name);
                    TransferDirectory(subDir.FullName, newDestinationDirectory, ref stats);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}