using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace TouchOne
{
	public sealed class RandomImage
	{
		string[] _Images = { "IMG_0196.JPG", "IMG_0255.JPG", "IMG_0309.JPG", "IMG_0659.JPG", "IMG_0829.JPG", "IMG_0917.JPG", "IMG_1573s.jpg", "IMG_1603sc.jpg" };

		Random _random;
		int _Max;

		public RandomImage()
		{
			_random = new Random((int)DateTime.Now.Ticks);
			_Max = _Images.Length;
		}

		public BitmapImage Next()
		{
			int Number = _random.Next(_Max);
			string image = _Images[Number];
			return new BitmapImage(new Uri(@"pack://application:,,,/Images\" + image, UriKind.Absolute));
		}

	}
}
