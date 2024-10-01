using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Reflection;

namespace TouchOne;

public sealed class RandomSolidColorBrush
{
	Random _random;
	PropertyInfo[] _props;
	int _MaxProps;

	public RandomSolidColorBrush()
	{
		_random = new Random((int)DateTime.Now.Ticks);
		_props = typeof(Brushes).GetProperties();
		_MaxProps = _props.Length;
	}

	public SolidColorBrush Next()
	{
		int Number = _random.Next(_MaxProps);
		return (SolidColorBrush)_props[Number].GetValue(null, null)!;
	}

}
