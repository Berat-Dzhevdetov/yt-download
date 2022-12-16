using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoLibrary;

namespace ytdownload
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var allVideos = File.ReadAllLines("../../music.txt").ToHashSet();

            var originalVideos = allVideos.Distinct().ToHashSet();
            Console.WriteLine($"{originalVideos.Count} unique songs");

            var source = @"../../music/";

            if (Directory.Exists(source))
            {
                Directory.Delete(source, true);
            }
            Directory.CreateDirectory(source);

            var youtube = YouTube.Default;

            var allTasks = new HashSet<Task>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            var doneTasks = 0;

            foreach (var video in originalVideos)
            {

                if (string.IsNullOrEmpty(video)) continue;
                var downloadTask = Task.Run(() =>
                {
                    try
                    {
                        var vid = youtube.GetVideo(video);
                        var bytes = vid.GetBytes();
                        
                        if(bytes.Length == 0) throw new InvalidDataException("Bytes were zero/null");

                        File.WriteAllBytes(source + vid.FullName, bytes);

                        var fullSource = source + Path.GetFileNameWithoutExtension(vid.FullName);
                        var inputFile = new MediaFile { Filename =  source + vid.FullName };
                        var outputFile = new MediaFile { Filename = source + Path.GetFileNameWithoutExtension(vid.FullName)+ ".mp3" };

                        using (var engine = new Engine())
                        {
                            engine.GetMetadata(inputFile);

                            engine.Convert(inputFile, outputFile);
                        }

                        File.Delete(inputFile.Filename);
                        doneTasks++;
                        Console.WriteLine($"{doneTasks}/{allTasks.Count} ({(doneTasks / (allTasks.Count * 1.0)) * 100}%)");
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine($"Failed to download: {video}");
                        System.Console.WriteLine(ex.Message);
                    }
                });
                allTasks.Add(downloadTask);
            }

            Task.WaitAll(allTasks.ToArray());
        }
    }
}
