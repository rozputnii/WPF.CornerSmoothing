namespace WPF.CornerSmoothing;

using System.Windows;

internal static class Extensions
{
	public static Size AsSize(this Thickness th) => new(th.Left + th.Right, th.Top + th.Bottom);

	public static Rect DeflateRect(this Rect rt, Thickness thick) =>
		new(
			rt.Left + thick.Left,
			rt.Top + thick.Top,
			Math.Max(0.0, rt.Width - thick.Left - thick.Right),
			Math.Max(0.0, rt.Height - thick.Top - thick.Bottom));
}
