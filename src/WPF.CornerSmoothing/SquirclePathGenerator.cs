namespace WPF.CornerSmoothing;

using System.Globalization;
using System.Windows.Media;

public static class SquirclePathGenerator
{
	private const double MinSmoothnessRatio = 0.447;
	private const double MaxRadiusMultiply = 2.341;

	public static Geometry CreateGeometry(double width, double height, double radius, double smoothness)
	{
		var geometry = GetGeometry(width, height, radius, smoothness);
		return geometry;
		//var pathGeometry = PathGeometry.CreateFromGeometry(geometry);
		//return pathGeometry;
	}

	private static Geometry GetGeometry(double width, double height, double radius, double smoothPercent)
	{
		if (radius < 0)
		{
			radius = 0;
		}

		var maxBorderRadius = Math.Min(width, height) / 2;
		radius = Math.Min(radius, maxBorderRadius);

		smoothPercent = smoothPercent switch
		{
			< 0 => 0,
			> 1 => 1,
			_ => smoothPercent
		};

		var smoothnessRatio = GetSmoothnessRatio(smoothPercent, radius, maxBorderRadius);
		var radiusMultiply = GetRadiusMultiply(smoothnessRatio);

		var pathData = GenerateSquirclePath(
			width,
			height,
			radius * smoothnessRatio,
			radius * radiusMultiply
		);

		var geometry = Geometry.Parse(pathData);
		return geometry;
	}

	private static double GetMaxSmoothnessRatio(double radius, double maxRadius)
	{
		var maxRadiusMultiply = maxRadius / radius;

		if (maxRadiusMultiply > MaxRadiusMultiply)
		{
			return 0;
		}

		var maxSmoothnessRatio = (MaxRadiusMultiply - maxRadiusMultiply) / 3;
		return maxSmoothnessRatio;
	}

	//private static double GetSmoothnessRatio(
	//	double smoothness, double radius, double maxRadius) 
	//{
	//	var maxRatio = GetMaxSmoothnessRatio(radius, maxRadius);
	//	//min is bigger than max /reverse direction of min/max

	//	var range = MinSmoothnessRatio - maxRatio;
	//	var percent = range * smoothness;
	//	var smoothnessRatio = MinSmoothnessRatio - percent * MinSmoothnessRatio;

	//	return smoothnessRatio;
	//}

	private static double GetSmoothnessRatio(
		double smoothness, double radius, double maxRadius)
	{
		var maxRatio = GetMaxSmoothnessRatio(radius, maxRadius);
		var range = MinSmoothnessRatio - maxRatio;
		var smoothnessRatio = MinSmoothnessRatio * (1 - (smoothness * range));

		return smoothnessRatio;
	}

	private static double GetRadiusMultiply(double smoothnessRatio) => MaxRadiusMultiply - (smoothnessRatio * 3);


	private static string GenerateSquirclePath(double w, double h, double r1, double r2)
	{
		r1 = Math.Min(r1, r2);

		var path = string.Format(CultureInfo.InvariantCulture,
			"""
			M 0,{0}
			C 0,{1} {1},0 {0},0
			L {2},0
			C {3},0 {4},{1} {4},{0}
			L {4},{5}
			C {4},{6} {3},{7} {2},{7}
			L {0},{7}
			C {1},{7} 0,{6} 0,{5}
			L 0,{0}
			""",
			r2,
			r1,
			w - r2,
			w - r1,
			w,
			h - r2,
			h - r1,
			h
		);

		return path.Trim().Replace('\n', ' ');
	}
}
