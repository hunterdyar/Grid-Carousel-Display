// See https://aka.ms/new-console-template for more information

using Raylib_cs;
using IronSoftware.Drawing;
using Color = Raylib_cs.Color;
using System.CommandLine;
using CommandLine;

namespace Thumbnailer;

public static class Program
{
	public class Options
	{
		public string ImagesDir = ".";
		public bool Convert = false;
		public float ScrollSpeed = 0.5f;
		public int TargetFps = 60;
		public ScrollDirection Direction = ScrollDirection.Right;
		public bool WatchFiles = false;
	}

	public enum ScrollDirection
	{
		Left,
		Right
	}
	private static SquareGridLayout? _layout;
	private static FileSystemWatcher? _watcher;
	private static Options? _options;
	
	public static async Task Main(string[] args)
	{
		var rootCommand = new RootCommand("Grid View Panner");
		var directoryOption = new Option<string>(name: "-d", description: "Directory to load images from",getDefaultValue: ()=>".");
		rootCommand.Add(directoryOption);

		var speedOption = new Option<float>(name: "-s", description: "Image Scroll Speed",
			getDefaultValue: () => 0.2f);
		rootCommand.Add(speedOption);
		
		var directionOption = new Option<ScrollDirection>(name: "-a", description: "Scroll Direction",
			getDefaultValue: () => ScrollDirection.Right);
		rootCommand.Add(directionOption);

		var watchOption = new Option<bool>(name: "-w", description: "Watch folder for changes",
			getDefaultValue: () => false);
		rootCommand.Add(watchOption);

		var convertOption = new Option<bool>(name: "-c", description: "Convert jpg to png",
			getDefaultValue: () => false);
		rootCommand.Add(convertOption);

		var fpsOption = new Option<int>(name: "-fps", description: "Target FPS",
			getDefaultValue: () => 60);
		rootCommand.Add(fpsOption);
		
		
		rootCommand.SetHandler((dirOptionValue,speedOptionValue,directionOptionValue, watchOptionValue, fpsOptionValue, convertOptionValue) =>
		{
			var options = new Options()
			{
				ImagesDir = dirOptionValue,
				ScrollSpeed = speedOptionValue,
				Direction = directionOptionValue,
				WatchFiles = watchOptionValue,
				TargetFps = fpsOptionValue,
				Convert = convertOptionValue
			};
			Run(options);
		}, directoryOption,speedOption,directionOption, watchOption, fpsOption, convertOption);
		
		await rootCommand.InvokeAsync(args);
	}

	public static void Run(Options options)
	{
		_options = options;
		options.TargetFps = options.TargetFps == 0 ? 60 : options.TargetFps;
		Raylib.SetTargetFPS(options.TargetFps);

		Raylib.InitWindow(1920, 1080, "Grid Viewer");
		Raylib.ToggleBorderlessWindowed();
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.White);
		Raylib.DrawText("Loading Images", 10, 10, 20, Color.Black);
		Raylib.EndDrawing();

		if (options.Convert)
		{
			ConvertDirToPNG(options.ImagesDir);
		}

		//init the watcher AFTER we create the png's. 
		InitWatcher(options);
		
		_layout = new SquareGridLayout(3, options.ScrollSpeed, options.Direction);
		int size = _layout.Size;
		var thumbnails = Thumbnail.GetThumbnailsFromDirectory(options.ImagesDir, size);
		_layout.SetThumbnails(thumbnails);

		while (!Raylib.WindowShouldClose())
		{
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.White);
			_layout.Draw();
			//offset layout
			Raylib.EndDrawing();
		}

		Raylib.CloseWindow();
	}

	private static void InitWatcher(Options options)
	{
		if (!options.WatchFiles)
		{
			return;
		}
		var dirInfo = new DirectoryInfo(options.ImagesDir!);
		_watcher = new FileSystemWatcher(dirInfo.FullName);
		_watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.Attributes | NotifyFilters.LastWrite |
		                        NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.FileName;
		//changed?
		_watcher.Created += WatcherOnChanged;
		_watcher.Deleted += WatcherOnChanged;
		_watcher.Changed += WatcherOnChanged;


		_watcher.EnableRaisingEvents = true;
	}

	private static void WatcherOnChanged(object sender, FileSystemEventArgs e)
	{
		if (_options!.Convert)
		{
			_watcher!.EnableRaisingEvents = false;
			ConvertDirToPNG(_options.ImagesDir);
			_watcher.EnableRaisingEvents = true;
		}

		var thumbnails = Thumbnail.GetThumbnailsFromDirectory(_options.ImagesDir, _layout!.Size);
		_layout.SetThumbnails(thumbnails);
	}
	private static void ConvertDirToPNG(string? inputDir)
	{
		int y = 10;
		if (inputDir != null)
		{
			var dirInfo = new DirectoryInfo(inputDir);
			if (!dirInfo.Exists)
			{
				throw new Exception($"Invalid Directory {inputDir}");
			}
		
			foreach (var f in dirInfo.EnumerateFiles())
			{
				if (f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".jpeg")
				{
					y += 10;
					string name = Path.GetFileNameWithoutExtension(f.FullName);
					string? path = Path.GetDirectoryName(f.FullName);
					if (path == null)
					{
						return;
					}
					Console.WriteLine($"Converting {name} to png");
					IronSoftware.Drawing.AnyBitmap jpg = IronSoftware.Drawing.AnyBitmap.FromFile(f.FullName);
					var newFile = path + @"/" + name + ".png";
					if(File.Exists(newFile))
					{
						continue;
					}
					//shit it's not on luinux
					//convert and save.
					string?[] p  = {path, name+".png"};
					jpg.SaveAs(Path.Combine(p!), AnyBitmap.ImageFormat.Png);
					jpg.Dispose();
				}
			}
		}
	}
}