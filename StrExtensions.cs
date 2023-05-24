using UnityEngine;

namespace BlockyMapGen
{
	internal static class StrExtensions
	{
		public static bool disableColors = false;

		// credits for these extensions goes to https://forum.unity.com/threads/easy-text-format-your-debug-logs-rich-text-format.906464/
		public static string Bold(this string str) => "<b>" + str + "</b>";
		public static string Color(this string str, Color color) => disableColors ? str : $"<color=#{color.ToHexString()}>{str}</color>";
		public static string Italic(this string str) => "<i>" + str + "</i>";
		public static string Size(this string str, int size) => string.Format("<size={0}>{1}</size>",size,str);

		/// <summary>
		/// exact copy of <see cref="Unity.VisualScripting.XColor.ToHexString"/>. duplicated for removal of dependency
		/// </summary>
		/// <param name="color">Color to turn to hex</param>
		/// <returns>The hex string of the color</returns>
		public static string ToHexString(this Color color)
		{
			return
				((byte)(color.r * 255)).ToString("X2") +
				((byte)(color.g * 255)).ToString("X2") +
				((byte)(color.b * 255)).ToString("X2") +
				((byte)(color.a * 255)).ToString("X2");
		}

	}
}
 