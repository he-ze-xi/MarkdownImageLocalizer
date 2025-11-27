using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace MarkdownImageLocalizer
{
    class Program
    {
        private static readonly Regex ImageRegex = new Regex(
            @"!\[(.*?)\]\((?<url>https?://[^\s\)\""]+)(?: \""(?<title>[^\""]*)\"")?\)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        // Global cache to avoid downloading the same image multiple times
        private static readonly ConcurrentDictionary<string, string> UrlToLocalPath = new();

        static async Task Main()
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("   Markdown Image Localizer (Multi-threaded)");
            Console.WriteLine("   Automatically creates xxx.assets folders (Hexo-style)");
            Console.WriteLine("==================================================\n");

            string sourceDir = AskPath("Enter source folder path (containing original .md files)", mustExist: true);
            string outputDir = AskPath("Enter output folder path (where converted files will be saved)", mustExist: false);

            Directory.CreateDirectory(outputDir);

            var mdFiles = Directory.GetFiles(sourceDir, "*.md", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(sourceDir, "*.markdown", SearchOption.AllDirectories))
                .ToArray();

            if (mdFiles.Length == 0)
            {
                Console.WriteLine("No Markdown files found in the source directory.");
                WaitAndExit();
                return;
            }

            Console.WriteLine($"Found {mdFiles.Length} Markdown file(s). Starting parallel processing...\n");

            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            var progressLock = new object();
            int completed = 0;

            await Task.Run(() =>
            {
                Parallel.ForEach(mdFiles, options, sourceFile =>
                {
                    ProcessFile(sourceFile, sourceDir, outputDir);

                    lock (progressLock)
                    {
                        completed++;
                        Console.WriteLine($"[{completed,3} / {mdFiles.Length}] Completed: {GetRelativePath(sourceFile, sourceDir)}");
                    }
                });
            });

            Console.WriteLine($"\nAll {mdFiles.Length} file(s) processed successfully!");
            Console.WriteLine($"Output directory: {outputDir}");
            WaitAndExit();
        }

        static void ProcessFile(string sourceFile, string sourceRoot, string outputRoot)
        {
            string content = File.ReadAllText(sourceFile);
            var matches = ImageRegex.Matches(content);

            // Preserve original folder structure
            string relativePath = Path.GetRelativePath(sourceRoot, sourceFile);
            string relativeDir = Path.GetDirectoryName(relativePath)!;
            string fileName = Path.GetFileName(sourceFile);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceFile);

            string outputMdDir = Path.Combine(outputRoot, relativeDir);
            string outputMdPath = Path.Combine(outputMdDir, fileName);
            Directory.CreateDirectory(outputMdDir);

            // Create xxx.assets folder (Hexo convention)
            string assetsFolder = fileNameWithoutExt + ".assets";
            string imageDir = Path.Combine(outputMdDir, assetsFolder);
            Directory.CreateDirectory(imageDir);

            // Download images concurrently if any exist
            if (matches.Count > 0)
            {
                var tasks = matches.Cast<Match>()
                    .Select(m => m.Groups["url"].Value)
                    .Distinct()
                    .Select(url => DownloadImageAsync(url, imageDir));

                Task.WhenAll(tasks).Wait();
            }

            // Replace image links with local paths
            string newContent = ImageRegex.Replace(content, m =>
            {
                string url = m.Groups["url"].Value;
                string alt = m.Groups[1].Value;
                string title = m.Groups["title"].Success ? m.Groups["title"].Value : null;

                if (!UrlToLocalPath.TryGetValue(url, out string localPath))
                    return m.Value; // Keep original link on failure

                string fileNameInAssets = Path.GetFileName(localPath);
                return string.IsNullOrEmpty(title)
                    ? $"![{alt}]({assetsFolder}/{fileNameInAssets})"
                    : $"![{alt}]({assetsFolder}/{fileNameInAssets} \"{title}\")";
            });

            File.WriteAllText(outputMdPath, newContent);
        }

        static async Task DownloadImageAsync(string url, string imageDir)
        {
            if (UrlToLocalPath.ContainsKey(url)) return;

            try
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return;

                string ext = GetExtension(url, response);
                string name = GenerateSafeFileName(url, ext);
                string path = GetUniqueFilePath(Path.Combine(imageDir, name));

                byte[] data = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(path, data);

                UrlToLocalPath[url] = path;
            }
            catch
            {
                // Silently ignore failed downloads
            }
        }

        static string GetExtension(string url, HttpResponseMessage r) =>
            r.Content.Headers.ContentType?.MediaType switch
            {
                "image/jpeg" or "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/svg+xml" => ".svg",
                _ => Path.GetExtension(new Uri(url).AbsolutePath)?.ToLowerInvariant() ?? ".jpg"
            };

        static string GenerateSafeFileName(string url, string ext)
        {
            string name = Path.GetFileNameWithoutExtension(new Uri(url).Segments.Last() ?? "");
            if (string.IsNullOrWhiteSpace(name))
                name = "img_" + DateTime.Now.Ticks;

            name = new string(name.Take(100).ToArray()); // Limit length
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return name + ext;
        }

        static string GetUniqueFilePath(string path)
        {
            if (!File.Exists(path)) return path;

            string dir = Path.GetDirectoryName(path)!;
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            int i = 1;

            string newPath;
            do
            {
                newPath = Path.Combine(dir, $"{name}_{i++}{ext}");
            } while (File.Exists(newPath));

            return newPath;
        }

        static string AskPath(string prompt, bool mustExist)
        {
            while (true)
            {
                Console.Write($"{prompt}:\n> ");
                string input = Console.ReadLine()?.Trim('"', '\'', ' ');
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Path cannot be empty!\n");
                    continue;
                }

                string fullPath = Path.GetFullPath(input);
                if (mustExist && !Directory.Exists(fullPath))
                {
                    Console.WriteLine("Directory does not exist!\n");
                    continue;
                }

                return fullPath;
            }
        }

        static string GetRelativePath(string fullPath, string basePath)
        {
            var rel = Path.GetRelativePath(basePath, fullPath).Replace("\\", "/");
            return rel.StartsWith("./") || rel.StartsWith("../") ? rel : "./" + rel;
        }

        static void WaitAndExit()
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
