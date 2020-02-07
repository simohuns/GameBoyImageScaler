using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;

namespace GameBoyImageScaler
{
    public static class Program
    {
        // Constants
        public const int GAME_BOY_IMAGE_WIDTH = 128;
        public const int GAME_BOY_IMAGE_HEIGHT = 112;
        public const float DEFAULT_SCALE_FACTOR = 10;
        public static readonly ImageFormat GAME_BOY_IMAGE_FORMAT_INPUT = ImageFormat.Bmp;
        public static readonly ImageFormat GAME_GOY_IMAGE_FORMAT_OUTPUT = ImageFormat.Png;
        public const string PROCESSED_IMAGE_SUFFIX = "-scaled";

        // Main program
        public static void Main(string[] args)
        {
            string workspace;
            float scaleFactor;

            // Set main variables
            Console.WriteLine("Reading arguments");
            try
            {
                ReadArgs(args, out workspace, out scaleFactor);
            }
            catch (Exception ex)
            {
                PrintException(ex);
                return;
            }

            // load image file list
            List<string> imageFiles;
            try
            {
                imageFiles = GetImageFormatFileList(workspace, GAME_BOY_IMAGE_FORMAT_INPUT);
            }
            catch (Exception ex)
            {
                PrintException(ex);
                return;
            }

            if (imageFiles.Count == 0)
            {
                Console.WriteLine($"No {GAME_BOY_IMAGE_FORMAT_INPUT.ToString()} files found in workspace \"{workspace}\"");
                return;
            }

            // scale and save each file
            using (Bitmap scale = new Bitmap((int)Math.Round(GAME_BOY_IMAGE_WIDTH * scaleFactor), (int)Math.Round(GAME_BOY_IMAGE_HEIGHT * scaleFactor)))
            {
                using (Graphics g = Graphics.FromImage(scale))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.SmoothingMode = SmoothingMode.None;

                    foreach(string imageFile in imageFiles)
                    {
                        try
                        {
                            ScaleAndSaveImageFile(g, scale, scaleFactor, GAME_GOY_IMAGE_FORMAT_OUTPUT, imageFile, workspace);
                        }
                        catch (Exception ex)
                        {
                            PrintException(ex);
                        }
                    }
                }
            }
        }

        // Print an exception out on screen
        private static void PrintException(Exception ex)
        {
            if (ex != null)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Read the application arguments and assign values
        private static void ReadArgs(string[] args, out string workspace, out float scaleFactor)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments, selecting defaults");
                workspace = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                scaleFactor = DEFAULT_SCALE_FACTOR;
            }
            else if (args.Length == 2)
            {
                Console.WriteLine("Selecting workspace from arguments");
                workspace = args[0];
                if (!float.TryParse(args[1], out scaleFactor) || scaleFactor <= 0f)
                {
                    throw new ArgumentException($"Invalid argument \"{args[1]}\"");
                }
            }
            else
            {
                throw new ArgumentException("Invalid argument count provided");
            }
        }

        // Get the list of image files with the specified format from the workspace
        private static List<string> GetImageFormatFileList(string workspace, ImageFormat format)
        {
            return Directory.GetFiles(workspace).Where(x => Path.GetExtension(x).Replace(".", "").Equals(format.ToString(), StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        // Scale and save specified image file
        private static void ScaleAndSaveImageFile(Graphics g, Bitmap scale, float scaleFactor, ImageFormat format, string imageFile, string workspace)
        {
            string name = Path.GetFileNameWithoutExtension(imageFile);
            string extension = Path.GetExtension(imageFile);
            string processedPath = workspace + "\\" + name + PROCESSED_IMAGE_SUFFIX + "." + format.ToString().ToLower();

            try
            {
                using (Bitmap original = new Bitmap(Image.FromFile(imageFile)))
                {
                    if (original.Width == GAME_BOY_IMAGE_WIDTH && original.Height == GAME_BOY_IMAGE_HEIGHT)
                    {
                        Console.WriteLine($"Processing \"{name + extension}\"");
                        g.DrawImage(original, 0, 0, scale.Width, scale.Height);

                        if (File.Exists(processedPath))
                        {
                            //Force garbage collection to clear previous unmanaged Bitmaps that may be placing a file lock
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            File.Delete(processedPath);
                        }

                        Console.WriteLine($"Saving \"{processedPath}\"");
                        scale.Save(processedPath, format);
                    }
                    else
                    {
                        Console.WriteLine($"Skipping \"{name + extension}\" because size is not the expected {GAME_BOY_IMAGE_WIDTH}x{GAME_BOY_IMAGE_HEIGHT}");
                    }
                }
            }
            catch
            {
                Console.WriteLine($"Error processing \"{name + extension}\"");
            } 

        }
    }
}
