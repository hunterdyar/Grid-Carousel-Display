﻿// See https://aka.ms/new-console-template for more information

using CommandLine;
using Raylib_cs;
using IronSoftware.Drawing;
using Color = Raylib_cs.Color;

namespace Thumbnailer;

public static class Program
{
	public class Options
	{
		[Option('d', "directory", Required = false, HelpText = "Directory For Images", Default = ".")]
		public string imagesDir { get; set; }

		[Option('c', "convert", Required = false, HelpText = "Convert Images to PNG before running", Default = true)]
		public bool Convert { get; set; }
		
		[Option('s', "scrollspeed", Required = false, HelpText = "Scroll Speed in Squares Per Second", Default = 0.5f)]
		public float scrollSpeed { get; set; }

		[Option('f', "fps", Required = false, HelpText = "Target FPS. Defualt is 60", Default = 60)]
		public int targetFPS { get; set; }

		[Option('a', "direction", Required = false, HelpText = "Scroll Direction. Options are are 'left' and 'right'", Default = ScrollDirection.Right)]
		public ScrollDirection direction { get; set; }
	}
	private static SquareGridLayout _layout;

	public enum ScrollDirection
	{
		Left,
		Right
	}
	
	public static void Main(string[] args)
	{
		CommandLine.Parser.Default.ParseArguments<Options>(args)
			.WithParsed(Run)
			.WithNotParsed(HandleParseError);
	}

	public static void Run(Options options)
	{
		options.targetFPS = options.targetFPS == 0 ? 60 : options.targetFPS;
		Raylib.SetTargetFPS(options.targetFPS);

		Raylib.InitWindow(1920, 1080, "Grid Viewer");
		Raylib.ToggleBorderlessWindowed();
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.White);
		Raylib.DrawText("Loading Images", 10, 10, 20, Color.Black);
		Raylib.EndDrawing();

		if (options.Convert)
		{
			ConvertDirToPNG(options.imagesDir);
		}

		_layout = new SquareGridLayout(3, options.scrollSpeed, options.direction);
		int size = _layout.Size;
		var thumbnails = Thumbnail.GetThumbnailsFromDirectory(options.imagesDir, size);
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
	
	static void HandleParseError(IEnumerable<Error> errs)
	{
		//handle errors
		foreach (var error in errs)
		{
			Console.WriteLine(error);
		}
	}
	private static void ConvertDirToPNG(string inputDir)
	{
		int y = 10;
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
				string path = Path.GetDirectoryName(f.FullName);
				Console.WriteLine($"Converting {name} to png");
				IronSoftware.Drawing.AnyBitmap jpg = IronSoftware.Drawing.AnyBitmap.FromFile(f.FullName);
				var newFile = path + @"/" + name + ".png";
				if(File.Exists(newFile))
				{
					continue;
				}
				//shit it's not on luinux
				//convert and save.
				jpg.SaveAs(Path.Combine([path, name+".png"]), AnyBitmap.ImageFormat.Png);
				jpg.Dispose();
			}
		}
	}
}