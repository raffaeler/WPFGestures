using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace TouchOne;

public static class FormatHelper
{
	public static string F(this double value)
	{
		return value.ToString("0000.00");
	}

	public static string F(this Vector value)
	{
		return string.Format("({0}, {1})", value.X.ToString("0000.00"), value.Y.ToString("0000.00"));
	}

	public static string F(this Point value)
	{
		return string.Format("({0}, {1})", value.X.ToString("0000.00"), value.Y.ToString("0000.00"));
	}

}
