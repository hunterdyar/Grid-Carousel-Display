using Raylib_cs;

namespace Thumbnailer;

public class Thumbnail
{
	public bool IsLoaded;
	private string _imagePath;
	public Texture2D Texture;
	int imgSize;
	public Thumbnail(string imagePath, int imgSize)
	{
		IsLoaded = false;
		this._imagePath = imagePath;
		this.imgSize = imgSize;
	}
	
	public void Load()
	{
		var img  = Raylib.LoadImage(_imagePath);
		
		//crop to center, ensure is square.
		float smallSize = img.Width < img.Height ? img.Width : img.Height;
		float x=0, y = 0;
		if (img.Width < smallSize)
		{
			x = (smallSize - img.Width) / 2;
		}

		if (img.Height < smallSize)
		{
			y = (smallSize - img.Height) / 2;
		}
		
		Raylib.ImageCrop(ref img, new Rectangle(x, y,smallSize, smallSize));
		Raylib.ImageResize(ref img, imgSize, imgSize);
		Texture = Raylib.LoadTextureFromImage(img);
		Raylib.UnloadImage(img);
		IsLoaded = true;
	}

	public void Unload()
	{
		Raylib.UnloadTexture(Texture);
		IsLoaded = false;
	}

	
	
	
	
	public static Thumbnail[] GetThumbnailsFromDirectory(string directoryPath, int size)
	{
		var dirInfo = new DirectoryInfo(directoryPath);
		if (!dirInfo.Exists)
		{
			throw new Exception($"Invalid Directory for images: {directoryPath}");
		}

		var thumbnails = new List<Thumbnail>();
		foreach (var f in dirInfo.EnumerateFiles())
		{
			if(f.Extension.ToLower() == ".png")
			{
				thumbnails.Add(new Thumbnail(f.FullName, size));
			}
		}

		return thumbnails.ToArray();
	}
}