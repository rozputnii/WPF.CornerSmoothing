namespace WPF.CornerSmoothing;

using System.Windows;

internal static class Extensions
{
	public static Size AsSize(this Thickness th)
	{
		if (double.IsFinite(th.Left) &&
		    double.IsFinite(th.Top) &&
		    double.IsFinite(th.Right) &&
		    double.IsFinite(th.Bottom))
		{
			return new Size(Math.Max(0d, th.Left + th.Right) , Math.Max(0d, th.Top + th.Bottom) );
		}
		return new Size(0, 0);
	}

	public static Rect DeflateRect(this Rect rt, Thickness th)
	{
		if (double.IsFinite(th.Left) &&
		    double.IsFinite(th.Top) &&
		    double.IsFinite(th.Right) &&
		    double.IsFinite(th.Bottom))
		{
			return new Rect(
				rt.X + th.Left,
				rt.Y + th.Top,
				Math.Max(0.0, rt.Width - th.Left - th.Right),
				Math.Max(0.0, rt.Height - th.Top - th.Bottom)
			);
		}
		return rt;
	}

	public static bool EqualsWithTolerance(this double x, double y, double tolerance = 0.001d) => Math.Abs(x - y) < tolerance;

	public static bool EqualsWithTolerance(this double x, double? y, double tolerance = 0.001d)
		=> y is { } yv && EqualsWithTolerance(x, yv, tolerance);
}
