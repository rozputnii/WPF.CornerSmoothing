namespace WPF.CornerSmoothing;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

/// <summary>
///     Smoothing border container
/// </summary>
public class SmoothBorder : Decorator
{
	private readonly DrawingVisual _backgroundVisual = new();
	private readonly DrawingVisual _borderVisual = new();
	private Thickness _padding;
	private Geometry? _geometry;
	private Geometry? _childClipGeometry;
	private Rect _lastBounds;
	private double _lastRadius, _lastSmooth;
	private Pen? _pen;



	public SmoothBorder()
	{
		AddVisualChild(_backgroundVisual);
		AddLogicalChild(_backgroundVisual);

		AddVisualChild(_borderVisual);
		AddLogicalChild(_borderVisual);
	}

	#region Dependency Properties

	private const FrameworkPropertyMetadataOptions PropertyFlags = FrameworkPropertyMetadataOptions.AffectsMeasure |
	                                                               FrameworkPropertyMetadataOptions.AffectsRender;

	public static readonly DependencyProperty CornerSmoothingProperty = DependencyProperty.Register(
		nameof(CornerSmoothing), typeof(double), typeof(SmoothBorder),
		new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, CornerSmoothingChanged),
		IsCornerSmoothingValid);

	private static void CornerSmoothingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SmoothBorder b)
		{
			b._cornerSmoothing = e.NewValue is double v ? v : 0.0;
		}
	}

	public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
		nameof(CornerRadius), typeof(double), typeof(SmoothBorder),
		new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

	public static readonly DependencyProperty ClipContentProperty
		= DependencyProperty.Register(nameof(ClipContent), typeof(bool), typeof(SmoothBorder),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

	public static readonly DependencyProperty BorderThicknessProperty
		= DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(SmoothBorder),
			new FrameworkPropertyMetadata(default(Thickness), PropertyFlags, BorderThicknessChanged),
			IsBorderThicknessValid);

	public static readonly DependencyProperty PaddingProperty
		= DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(SmoothBorder),
			new FrameworkPropertyMetadata(default(Thickness), PropertyFlags, OnPaddingChanged));

	private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SmoothBorder b)
		{
			b._padding = e.NewValue is Thickness th ? th : default;
		}
	}

	public static readonly DependencyProperty BorderBrushProperty
		= DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(SmoothBorder),
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.AffectsRender |
				FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
				BorderBrushChanged));

	public static readonly DependencyProperty BackgroundProperty =
		Panel.BackgroundProperty.AddOwner(typeof(SmoothBorder),
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.AffectsRender |
				FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

	public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(
		nameof(Geometry), typeof(Geometry), typeof(SmoothBorder),new PropertyMetadata(GeometryChanged));

	private static bool IsCornerSmoothingValid(object value) => value is >= 0d and <= 1d;
	private static void GeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SmoothBorder b)
		{			
			b._geometry = e.NewValue as Geometry;
		}
	}

	private static bool IsBorderThicknessValid(object value)
	{
		return value is Thickness { Left: >= 0d, Top: >= 0d, Right: >= 0d, Bottom: >= 0d };
	}

	private static void BorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var border = (SmoothBorder)d;
		border._pen = null;
	}

	private static void BorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var border = (SmoothBorder)d;
		border._pen = null;
	}

	#endregion

	#region Public Properties

	public Geometry? Geometry
	{
		get => _geometry;
		private set => SetValue(GeometryProperty, value);
	}

	private double _cornerSmoothing;
	public double CornerSmoothing
	{
		get => _cornerSmoothing;
		set => SetValue(CornerSmoothingProperty, value);
	}

	public double CornerRadius
	{
		get => (double)GetValue(CornerRadiusProperty);
		set => SetValue(CornerRadiusProperty, value);
	}

	[Bindable(true)]
	[Category("Appearance")]
	public Thickness BorderThickness
	{
		get => (Thickness)GetValue(BorderThicknessProperty);
		set => SetValue(BorderThicknessProperty, value);
	}

	public Thickness Padding
	{
		get => _padding;
		set => SetValue(PaddingProperty, value);
	}

	public bool ClipContent
	{
		get => (bool)GetValue(ClipContentProperty);
		set => SetValue(ClipContentProperty, value);
	}

	public Brush? BorderBrush
	{
		get => (Brush?)GetValue(BorderBrushProperty);
		set => SetValue(BorderBrushProperty, value);
	}

	public Brush? Background
	{
		get => (Brush?)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}

	#endregion

	#region Protected Methods

	protected override int VisualChildrenCount
		=> base.VisualChildrenCount + 2;

	protected override Visual GetVisualChild(int index)
	{
		return index switch
		{
			0 => _backgroundVisual,
			1 => Child,
			2 => _borderVisual,
			_ => throw new ArgumentOutOfRangeException(nameof(index))
		};
	}

	protected override Size MeasureOverride(Size constraint)
	{
		var child = Child;

		var borderSize = Padding.AsSize();

		if (child == null) return borderSize;

		Size childConstraint = new(
			Math.Max(0d, constraint.Width - borderSize.Width),
			Math.Max(0d, constraint.Height - borderSize.Height)
		);

		child.Measure(childConstraint);

		var childSize = child.DesiredSize;

		borderSize.Width = childSize.Width + borderSize.Width;
		borderSize.Height = childSize.Height + borderSize.Height;

		return borderSize;
	}

	private Rect? _prevChildBounds;
	

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		var bounds = new Rect(arrangeSize);

		if (bounds != _lastBounds || !CornerRadius.EqualsWithTolerance(_lastRadius)
		                          || !CornerSmoothing.EqualsWithTolerance(_lastSmooth))
		{
			Geometry = null;
		}

		if (Geometry is null)
		{
			Geometry = CreateGeometry(bounds, CornerRadius, CornerSmoothing);
			_lastBounds = bounds;
			_lastRadius = CornerRadius;
			_lastSmooth = CornerSmoothing;
			_childClipGeometry = null;
			_prevChildBounds = null;
		}

		RenderBackground();
		
		var child = Child;
		if (child != null)
		{
			var childBounds = bounds.DeflateRect(Padding);
			child.Arrange(childBounds);

			if (ClipContent)
			{
				if (childBounds == bounds)
				{
					_childClipGeometry = Geometry;
				}
				else if (childBounds != _prevChildBounds)
				{
					_childClipGeometry = new GeometryGroup
					{
						Children = { Geometry },
						Transform = new TranslateTransform(-childBounds.X, -childBounds.Y)
					};
					_childClipGeometry.Freeze();
				}

				Child.Clip = _childClipGeometry;
				_prevChildBounds = childBounds;
			}
			else
			{
				child.Clip = null;
				_prevChildBounds = null;
				_childClipGeometry = null;
			}
		}

		RenderBorder();
		return arrangeSize;
	}
	
	protected virtual Geometry CreateGeometry(Rect rect, double cornerRadius, double cornerSmoothing)
	{
		if (cornerSmoothing <= 0)
		{
			RectangleGeometry rectangleGeometry = new(rect, cornerRadius, cornerRadius);
			rectangleGeometry.Freeze();
			return rectangleGeometry;
		}

		var geometry = SquirclePathGenerator.CreateGeometry(
			rect.Width,
			rect.Height,
			cornerRadius,
			cornerSmoothing
		);

		geometry.Freeze();

		return geometry;
	}

	private void RenderBackground()
	{
		if (Geometry is null || Background is null) return;

		using var dc = _backgroundVisual.RenderOpen();
		dc.DrawGeometry(Background, null, Geometry);
	}

	private void RenderBorder()
	{
		var pen = GetPen();
		if (pen is null || Geometry is null) return;

		using var dc = _borderVisual.RenderOpen();
		dc.PushClip(Geometry);
		dc.DrawGeometry(null, pen, Geometry);
		dc.Pop();
	}

	private Brush? _prevBrush;
	private Thickness? _prevThickness;
	private Pen? GetPen()
	{
		var brush = BorderBrush;
		var thickness = BorderThickness;

		if (brush is null || thickness.Left == 0)
		{
			_pen = null;
			return null;
		}

		var pen = _pen;
		if (pen is not null && _prevBrush == brush && _prevThickness == thickness)
		{
			return pen;
		}

		pen = new Pen
		{
			Brush = brush,
			Thickness = thickness.Left * 2, // we are clip part of 1/2 of Thickness
			LineJoin = PenLineJoin.Round
		};

		if (pen.CanFreeze) pen.Freeze();

		_pen = pen;
		_prevBrush = brush;
		_prevThickness = thickness;

		return pen;
	}

	#endregion Protected Methods
}
