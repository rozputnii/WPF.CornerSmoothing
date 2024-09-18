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
	private Geometry? _geometry;
	private Pen? _pen;

	#region Dependency Properties

	private const FrameworkPropertyMetadataOptions PropertyFlags = FrameworkPropertyMetadataOptions.AffectsMeasure |
																   FrameworkPropertyMetadataOptions.AffectsRender;

	public static readonly DependencyProperty CornerSmoothingProperty = DependencyProperty.Register(
		nameof(CornerSmoothing), typeof(double), typeof(SmoothBorder),
		new FrameworkPropertyMetadata(1d, PropertyFlags), IsCornerSmoothingValid);

	public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
		nameof(CornerRadius), typeof(double), typeof(SmoothBorder),
		new FrameworkPropertyMetadata(0d, PropertyFlags));

	public static readonly DependencyProperty ClipContentProperty
		= DependencyProperty.Register(nameof(ClipContent), typeof(bool), typeof(SmoothBorder),
			new FrameworkPropertyMetadata(true, PropertyFlags));


	public static readonly DependencyProperty BorderThicknessProperty
		= DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(SmoothBorder),
			new FrameworkPropertyMetadata(new Thickness(0d), PropertyFlags, OnClearPenCache),
			IsBorderThicknessValid);

	public static readonly DependencyProperty PaddingProperty
		= DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(SmoothBorder),
			new FrameworkPropertyMetadata(new Thickness(), PropertyFlags));

	public static readonly DependencyProperty BorderBrushProperty
		= DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(SmoothBorder),
			new FrameworkPropertyMetadata(default(Brush?),
				FrameworkPropertyMetadataOptions.AffectsRender |
				FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
				OnClearPenCache));

	public static readonly DependencyProperty BackgroundProperty =
		Panel.BackgroundProperty.AddOwner(typeof(SmoothBorder),
			new FrameworkPropertyMetadata(default(Brush?),
				FrameworkPropertyMetadataOptions.AffectsRender |
				FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

	public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(
		nameof(Geometry), typeof(Geometry), typeof(SmoothBorder));

	private static bool IsCornerSmoothingValid(object value) => value is >= 0d and <= 1d;

	private static bool IsBorderThicknessValid(object value) => value is Thickness { Left: >= 0d };

	private static void OnClearPenCache(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var border = (SmoothBorder)d;
		border._pen = null;
	}

	#endregion

	#region Public Properties

	public Geometry? Geometry
	{
		get => (Geometry?)GetValue(GeometryProperty);
		private set => SetValue(GeometryProperty, value);
	}

	public double CornerSmoothing
	{
		get => (double)GetValue(CornerSmoothingProperty);
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
		get => (Thickness)GetValue(PaddingProperty);
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

	protected override Size MeasureOverride(Size constraint)
	{
		var child = Child;

		var paddingSize = Padding.AsSize();
		var borderThicknessSize = BorderThickness.AsSize();

		Size borderSize = new(
			paddingSize.Width + borderThicknessSize.Width,
			paddingSize.Height + borderThicknessSize.Height);

		if (child == null)
		{
			return borderSize;
		}

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

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		Rect boundRect = new(arrangeSize);

		var borderGeometry = CreateGeometry(boundRect, CornerRadius, CornerSmoothing);

		Geometry = _geometry = borderGeometry;

		var child = Child;
		if (child is null)
		{
			return arrangeSize;
		}

		var childRect = boundRect
			.DeflateRect(Padding)
			.DeflateRect(BorderThickness);

		child.Arrange(childRect);

		if (!ClipContent)
		{
			child.Clip = null;
			return arrangeSize;
		}

		if (childRect == boundRect)
		{
			child.Clip = borderGeometry;
		}
		else
		{
			var childCornerRadius = CornerRadius - Padding.Left - BorderThickness.Left;
			var childBorderGeometry = CreateGeometry(childRect, childCornerRadius, CornerSmoothing);

			child.Clip = childBorderGeometry;
		}

		return arrangeSize;
	}

	protected virtual Geometry CreateGeometry(Rect rect, double cornerRadius, double cornerSmoothing)
	{
		var geometry = SquirclePathGenerator.CreateGeometry(
			rect.Width,
			rect.Height,
			cornerRadius,
			cornerSmoothing
		);

		geometry.Freeze();

		return geometry;
	}

	protected override void OnRender(DrawingContext dc)
	{
		if (_geometry is null)
		{
			return;
		}


		var borderPen = GetPen();
		if (borderPen == null && Background is null)
		{
			return;
		}

		if (borderPen is not null)
		{
			dc.PushClip(_geometry);
		}

		dc.DrawGeometry(Background, borderPen, _geometry);

		if (borderPen is not null)
		{
			dc.Pop();
		}
	}

	private Pen? GetPen()
	{
		if (BorderBrush is null || BorderThickness.Left == 0)
		{
			_pen = null;
			return null;
		}

		var pen = _pen;
		if (pen is not null)
		{
			return pen;
		}

		pen = new Pen
		{
			Brush = BorderBrush,
			Thickness = BorderThickness.Left * 2, // we are clip part of 1/2 of Thickness
			LineJoin = PenLineJoin.Round
		};

		if (BorderBrush.IsFrozen)
		{
			pen.Freeze();
		}

		_pen = pen;

		return pen;
	}

	#endregion Protected Methods
}
