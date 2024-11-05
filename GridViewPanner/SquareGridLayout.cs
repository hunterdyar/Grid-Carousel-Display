using Raylib_cs;

namespace Thumbnailer;

public class SquareGridLayout
{
	public int Size => _pixelPerSlice;
	private int _pixelPerSlice;

	private Thumbnail[] _thumbnails;
	private int _rowCount = 3;
	private int _columnCount;
	private int _pixelOffset = 0;
	private int _indexOffset;
	private float _scrollSpeed;
	private Program.ScrollDirection _direction;
	public SquareGridLayout(int rowCount, float scrollSpeed, Program.ScrollDirection direction = Program.ScrollDirection.Right)
	{
		_rowCount = rowCount;
		int h = Raylib_cs.Raylib.GetScreenHeight();
		int w = Raylib.GetScreenWidth();
		_pixelPerSlice = h / _rowCount;
		_columnCount = (w / _pixelPerSlice) + 2;
		_scrollSpeed = scrollSpeed;
		_direction = direction;
	}

	public void SetThumbnails(Thumbnail[] thumbnails)
	{
		if (_thumbnails != null)
		{
			foreach (var t in _thumbnails)
			{
				t.Unload();
			}
		}
		_thumbnails = thumbnails;
	}
	
	private Thumbnail GetThumbnail(int index)
	{
		if (_thumbnails.Length == 0)
		{
			return null;
		}

		if (index < 0)
		{
			index = _thumbnails.Length - index;
		}
		var i = index % (_thumbnails.Length);
		
		return _thumbnails[i];
	}

	public void Draw()
	{
		TickOffset();
		if (_thumbnails.Length == 0)
		{
			return;
		}

		int t = _indexOffset;
		for (int j = 0; j < _columnCount; j++)
		{
			for (int i = 0; i < _rowCount; i++)
			{
				var thumbnail = GetThumbnail(t);
				if (!thumbnail.IsLoaded)
				{
					thumbnail.Load();
				}

				Raylib.DrawTexture(thumbnail.Texture, j * _pixelPerSlice - _pixelOffset, i * _pixelPerSlice, Color.White);
				
				//debugging
				//Raylib.DrawText($"{t} - ({i},{j})", 10+ j * _pixelPerSlice - _pixelOffset, 10+ i * _pixelPerSlice, 50, Color.Yellow);

				t++;
			}
		}
	}
	
	private void TickOffset()
	{
		double t = Raylib.GetFrameTime();
		double speed = _scrollSpeed;//1 = one square per second.
		double x = speed * _pixelPerSlice;//pixels to offset per second
		if (_direction == Program.ScrollDirection.Left)
		{
			_pixelOffset += (int)(x * t);
		}
		else
		{
			_pixelOffset -= (int)(x * t);
		}
		
		var o  = _pixelOffset % _pixelPerSlice;
		if (_direction == Program.ScrollDirection.Left)
		{
			if (o < _pixelOffset)
			{
				//snap back!
				_pixelOffset = 0;
				_indexOffset += _rowCount;
				//prevent eventual integer overflow if it runs for too many ... years...
				if (_indexOffset % _thumbnails.Length == 0)
				{
					_indexOffset-= _thumbnails.Length;
				}
			}
		}
		else
		{
			if (_pixelOffset < 0)
			{
				_pixelOffset = _pixelPerSlice + _pixelOffset;
				_indexOffset -= _rowCount;
				if (_indexOffset % _thumbnails.Length == 0)
				{
					_indexOffset += _thumbnails.Length;
				}
			}
		}
	}
}