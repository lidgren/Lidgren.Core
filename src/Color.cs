#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lidgren.Core
{
	/// <summary>
	/// Byte order: RGBA (red in lowest byte)
	/// </summary>
	public readonly struct Color : IEquatable<Color>, IComparable<Color>
	{
		public readonly uint RGBA;

		public byte R { get { return (byte)RGBA; } }
		public byte G { get { return (byte)(RGBA >> 8); } }
		public byte B { get { return (byte)(RGBA >> 16); } }
		public byte A { get { return (byte)(RGBA >> 24); } }

		public Color(uint rgba)
		{
			RGBA = rgba;
		}

		public Color(byte red, byte green, byte blue, byte alpha)
		{
			RGBA = (uint)red | ((uint)green << 8) | ((uint)blue << 16) | ((uint)alpha << 24);
		}

		public Color(byte red, byte green, byte blue)
		{
			RGBA = (uint)red | ((uint)green << 8) | ((uint)blue << 16) | (255u << 24);
		}

		public Color(float red, float green, float blue)
		{
			uint r = (uint)MathUtils.Clamp(255.0f * red, 0.0f, 255.0f);
			uint g = (uint)MathUtils.Clamp(255.0f * green, 0.0f, 255.0f);
			uint b = (uint)MathUtils.Clamp(255.0f * blue, 0.0f, 255.0f);
			RGBA = r | (g << 8) | (b << 16) | (255u << 24);
		}

		public Color(float red, float green, float blue, float alpha)
		{
			uint r = (uint)MathUtils.Clamp(255.0f * red, 0.0f, 255.0f);
			uint g = (uint)MathUtils.Clamp(255.0f * green, 0.0f, 255.0f);
			uint b = (uint)MathUtils.Clamp(255.0f * blue, 0.0f, 255.0f);
			uint a = (uint)MathUtils.Clamp(255.0f * alpha, 0.0f, 255.0f);
			RGBA = r | (g << 8) | (b << 16) | (a << 24);
		}

		public void GetRGBA(out byte red, out byte green, out byte blue, out byte alpha)
		{
			red = (byte)(RGBA & 255);
			green = (byte)(RGBA >> 8 & 255);
			blue = (byte)(RGBA >> 16 & 255);
			alpha = (byte)(RGBA >> 24 & 255);
		}

		public static Color FromBgra(uint color)
		{
			byte a = (byte)(color >> 24 & 255);
			byte r = (byte)(color >> 16 & 255);
			byte g = (byte)(color >> 8 & 255);
			byte b = (byte)(color & 255);
			return new Color(r, g, b, a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(in Color one, in Color two)
		{
			return one.RGBA == two.RGBA;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(in Color one, in Color two)
		{
			return one.RGBA != two.RGBA;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Color other)
		{
			return RGBA == other.RGBA;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEqual(in Color one, in Color two)
		{
			return one.RGBA == two.RGBA;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsLesser(in Color one, in Color two)
		{
			return one.RGBA < two.RGBA;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsGreater(in Color one, in Color two)
		{
			return one.RGBA > two.RGBA;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Compare(in Color one, in Color two)
		{
			return Comparer<UInt32>.Default.Compare(one.RGBA, two.RGBA);
		}

		private const float kByteToFloat = (float)(1.0 / 255.0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector4 ToVector4()
		{
			byte red = (byte)(RGBA & 255);
			byte green = (byte)(RGBA >> 8 & 255);
			byte blue = (byte)(RGBA >> 16 & 255);
			byte alpha = (byte)(RGBA >> 24 & 255);
			return new Vector4(
				(float)red * kByteToFloat,
				(float)green * kByteToFloat,
				(float)blue * kByteToFloat,
				(float)alpha * kByteToFloat
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 ToVector3()
		{
			byte red = (byte)(RGBA & 255);
			byte green = (byte)(RGBA >> 8 & 255);
			byte blue = (byte)(RGBA >> 16 & 255);
			return new Vector3(
				(float)red * kByteToFloat,
				(float)green * kByteToFloat,
				(float)blue * kByteToFloat
			);
		}

		public override bool Equals(object? obj) => obj is Color color && color.RGBA == RGBA;
		public override int GetHashCode() => (int)RGBA;

		/// <summary>
		/// To big endian hex value (ie. RRGGBBAA, for example #FFA50033 for orange with 20% opacity)
		/// </summary>
		public string ToHex()
		{
			uint big = ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | (uint)A;
			return StringUtils.ToHex(big, 8);
		}

		/// <summary>
		/// To alpha prefixed hex value (ie. AARRGGBB, for example #33FFA500 for orange with 20% opacity)
		/// </summary>
		public string ToHexARGB()
		{
			uint big = ((uint)A << 24) | ((uint)R << 16) | ((uint)G << 8) | (uint)B;
			return StringUtils.ToHex(big, 8);
		}

		/// <summary>
		/// To big endian hex value (ie. RRGGBBAA, for example #FFA50033 for orange with 20% opacity)
		/// </summary>
		public int ToHex(Span<char> str)
		{
			uint big = ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | (uint)A;
			return StringUtils.ToHex(big, str, 8);
		}

		/// <summary>
		/// Convert from big endian hex value (ie. RRGGBBAA, for example #FFA50033 for orange with 20% opacity; or #FF0000 which is Solid Red)
		/// </summary>
		public static Color FromHex(ReadOnlySpan<char> hex)
		{
			if (hex.Length > 0 && hex[0] == '#')
				hex = hex.Slice(1);

			if (hex.Length == 6)
			{
				uint big = (uint)StringUtils.FromHex(hex);
				uint little =
					((big & 0x00ff0000u) >> 16) | // R
					(big & 0x0000ff00u) | // G
					(big & 0x000000ffu) << 16 | // B
					(255u << 24); // A
				return new Color(little);
			}
			else if (hex.Length == 8)
			{
				uint big = (uint)StringUtils.FromHex(hex);
				uint little =
					((big & 0xff000000u) >> 24) |
					((big & 0x00ff0000u) >> 8) |
					((big & 0x0000ff00u) << 8) |
					((big & 0x000000ffu) << 24);
				return new Color(little);
			}
			else
			{
				CoreException.Throw("Expected 6 or 8 character string; optionally with # prefix");
				return Color.Black;
			}
		}

		public override string ToString()
		{
			return ToHex();
		}

		public int CompareTo(Color other)
		{
			if (RGBA < other.RGBA)
				return -1;
			if (RGBA > other.RGBA)
				return 1;
			return 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator uint(Color color)
		{
			return color.RGBA;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Color(uint value)
		{
			return new Color(value);
		}

		/// <summary>
		/// Generate a random color
		/// </summary>
		public static Color Random(float saturation = 0.95f)
		{
			var hue = PRNG.NextFloat();
			double r, g, b;
			HSVtoRGB(hue, saturation, 0.95f, out r, out g, out b);
			return new Color((float)r, (float)g, (float)b);
		}

		/// <summary>
		/// Generate x colors which are as far apart as possible
		/// </summary>
		public static void RandomMultiple(Span<Color> output, float saturation = 0.95f)
		{
			for (int i = 0; i < output.Length; i++)
			{
				var hue = ((0.618033988749895 * (i + 1)) % 1.0);
				double r, g, b;
				HSVtoRGB(hue, saturation, 0.95f, out r, out g, out b);
				output[i] = new Color((float)r, (float)g, (float)b);
			}
		}

		// Expects and returns values in the range 0 to 1
		public static void HSVtoRGB(double h, double s, double v, out double r, out double g, out double b)
		{
			if (s == 0.0)
			{
				r = v;
				g = v;
				b = v;
			}
			else
			{
				double varH = h * 6;
				double varI = Math.Floor(varH);
				double var1 = v * (1 - s);
				double var2 = v * (1 - (s * (varH - varI)));
				double var3 = v * (1 - (s * (1 - (varH - varI))));

				if (varI == 0.0)
				{
					r = v;
					g = var3;
					b = var1;
				}
				else if (varI == 1.0)
				{
					r = var2;
					g = v;
					b = var1;
				}
				else if (varI == 2.0)
				{
					r = var1;
					g = v;
					b = var3;
				}
				else if (varI == 3.0)
				{
					r = var1;
					g = var2;
					b = v;
				}
				else if (varI == 4.0)
				{
					r = var3;
					g = var1;
					b = v;
				}
				else
				{
					r = v;
					g = var1;
					b = var2;
				}
			}
		}

		// Expects and returns values in the range 0 to 1
		public static void RGBtoHSV(double r, double g, double b, out double h, out double s, out double v)
		{
			double varMin = Math.Min(r, Math.Min(g, b));
			double varMax = Math.Max(r, Math.Max(g, b));
			double delMax = varMax - varMin;

			v = varMax;

			if (delMax == 0.0)
			{
				h = 0;
				s = 0;
			}
			else
			{
				double delR = (((varMax - r) / 6) + (delMax / 2)) / delMax;
				double delG = (((varMax - g) / 6) + (delMax / 2)) / delMax;
				double delB = (((varMax - b) / 6) + (delMax / 2)) / delMax;

				s = delMax / varMax;

				if (r == varMax)
					h = delB - delG;
				else if (g == varMax)
					h = (1.0 / 3) + delR - delB;
				else //// if (b == varMax) 
					h = (2.0 / 3) + delG - delR;

				if (h < 0)
					h += 1;
				if (h > 1)
					h -= 1;
			}
		}

		public static Color Interpolate(Color fromColor, Color toColor, float amount)
		{
			uint from = fromColor.RGBA;
			uint to = toColor.RGBA;

			CoreException.Assert(amount >= 0.0f && amount <= 1.0f);
			uint r = (uint)((1.0f - amount) * (float)(from & 0xFF) + amount * (float)(to & 0xFF));
			uint g = (uint)((1.0f - amount) * (float)((from >> 8) & 0xFF) + amount * (float)((to >> 8) & 0xFF));
			uint b = (uint)((1.0f - amount) * (float)((from >> 16) & 0xFF) + amount * (float)((to >> 16) & 0xFF));
			uint a = (uint)((1.0f - amount) * (float)((from >> 24) & 0xFF) + amount * (float)((to >> 24) & 0xFF));
			return new Color(r | (g << 8) | (b << 16) | (a << 24));
		}

		public static readonly Color Zero = Color.FromBgra(0);
		public static readonly Color Transparent = Color.FromBgra(0);
		public static readonly Color AliceBlue = Color.FromBgra(4293982463u);
		public static readonly Color AntiqueWhite = Color.FromBgra(4294634455u);
		public static readonly Color Aqua = Color.FromBgra(4278255615u);
		public static readonly Color Aquamarine = Color.FromBgra(4286578644u);
		public static readonly Color Azure = Color.FromBgra(4293984255u);
		public static readonly Color Beige = Color.FromBgra(4294309340u);
		public static readonly Color Bisque = Color.FromBgra(4294960324u);
		public static readonly Color Black = Color.FromBgra(4278190080u);
		public static readonly Color BlanchedAlmond = Color.FromBgra(4294962125u);
		public static readonly Color Blue = Color.FromBgra(4278190335u);
		public static readonly Color BlueViolet = Color.FromBgra(4287245282u);
		public static readonly Color Brown = Color.FromBgra(4289014314u);
		public static readonly Color BurlyWood = Color.FromBgra(4292786311u);
		public static readonly Color CadetBlue = Color.FromBgra(4284456608u);
		public static readonly Color Chartreuse = Color.FromBgra(4286578432u);
		public static readonly Color Chocolate = Color.FromBgra(4291979550u);
		public static readonly Color Coral = Color.FromBgra(4294934352u);
		public static readonly Color CornflowerBlue = Color.FromBgra(4284782061u);
		public static readonly Color Cornsilk = Color.FromBgra(4294965468u);
		public static readonly Color Crimson = Color.FromBgra(4292613180u);
		public static readonly Color Cyan = Color.FromBgra(4278255615u);
		public static readonly Color DarkBlue = Color.FromBgra(4278190219u);
		public static readonly Color DarkCyan = Color.FromBgra(4278225803u);
		public static readonly Color DarkGoldenrod = Color.FromBgra(4290283019u);
		public static readonly Color DarkGray = Color.FromBgra(4289309097u);
		public static readonly Color DarkGreen = Color.FromBgra(4278215680u);
		public static readonly Color DarkKhaki = Color.FromBgra(4290623339u);
		public static readonly Color DarkMagenta = Color.FromBgra(4287299723u);
		public static readonly Color DarkOliveGreen = Color.FromBgra(4283788079u);
		public static readonly Color DarkOrange = Color.FromBgra(4294937600u);
		public static readonly Color DarkOrchid = Color.FromBgra(4288230092u);
		public static readonly Color DarkRed = Color.FromBgra(4287299584u);
		public static readonly Color DarkSalmon = Color.FromBgra(4293498490u);
		public static readonly Color DarkSeaGreen = Color.FromBgra(4287609995u);
		public static readonly Color DarkSlateBlue = Color.FromBgra(4282924427u);
		public static readonly Color DarkSlateGray = Color.FromBgra(4281290575u);
		public static readonly Color DarkTurquoise = Color.FromBgra(4278243025u);
		public static readonly Color DarkViolet = Color.FromBgra(4287889619u);
		public static readonly Color DeepPink = Color.FromBgra(4294907027u);
		public static readonly Color DeepSkyBlue = Color.FromBgra(4278239231u);
		public static readonly Color DimGray = Color.FromBgra(4285098345u);
		public static readonly Color DodgerBlue = Color.FromBgra(4280193279u);
		public static readonly Color Firebrick = Color.FromBgra(4289864226u);
		public static readonly Color FloralWhite = Color.FromBgra(4294966000u);
		public static readonly Color ForestGreen = Color.FromBgra(4280453922u);
		public static readonly Color Fuchsia = Color.FromBgra(4294902015u);
		public static readonly Color Gainsboro = Color.FromBgra(4292664540u);
		public static readonly Color GhostWhite = Color.FromBgra(4294506751u);
		public static readonly Color Gold = Color.FromBgra(4294956800u);
		public static readonly Color Goldenrod = Color.FromBgra(4292519200u);
		public static readonly Color Gray = Color.FromBgra(4286611584u);
		public static readonly Color Green = Color.FromBgra(4278222848u);
		public static readonly Color GreenYellow = Color.FromBgra(4289593135u);
		public static readonly Color Honeydew = Color.FromBgra(4293984240u);
		public static readonly Color HotPink = Color.FromBgra(4294928820u);
		public static readonly Color IndianRed = Color.FromBgra(4291648604u);
		public static readonly Color Indigo = Color.FromBgra(4283105410u);
		public static readonly Color Ivory = Color.FromBgra(4294967280u);
		public static readonly Color Khaki = Color.FromBgra(4293977740u);
		public static readonly Color Lavender = Color.FromBgra(4293322490u);
		public static readonly Color LavenderBlush = Color.FromBgra(4294963445u);
		public static readonly Color LawnGreen = Color.FromBgra(4286381056u);
		public static readonly Color LemonChiffon = Color.FromBgra(4294965965u);
		public static readonly Color LightBlue = Color.FromBgra(4289583334u);
		public static readonly Color LightCoral = Color.FromBgra(4293951616u);
		public static readonly Color LightCyan = Color.FromBgra(4292935679u);
		public static readonly Color LightGoldenrodYellow = Color.FromBgra(4294638290u);
		public static readonly Color LightGray = Color.FromBgra(4292072403u);
		public static readonly Color LightGreen = Color.FromBgra(4287688336u);
		public static readonly Color LightPink = Color.FromBgra(4294948545u);
		public static readonly Color LightSalmon = Color.FromBgra(4294942842u);
		public static readonly Color LightSeaGreen = Color.FromBgra(4280332970u);
		public static readonly Color LightSkyBlue = Color.FromBgra(4287090426u);
		public static readonly Color LightSlateGray = Color.FromBgra(4286023833u);
		public static readonly Color LightSteelBlue = Color.FromBgra(4289774814u);
		public static readonly Color LightYellow = Color.FromBgra(4294967264u);
		public static readonly Color Lime = Color.FromBgra(4278255360u);
		public static readonly Color LimeGreen = Color.FromBgra(4281519410u);
		public static readonly Color Linen = Color.FromBgra(4294635750u);
		public static readonly Color Magenta = Color.FromBgra(4294902015u);
		public static readonly Color Maroon = Color.FromBgra(4286578688u);
		public static readonly Color MediumAquamarine = Color.FromBgra(4284927402u);
		public static readonly Color MediumBlue = Color.FromBgra(4278190285u);
		public static readonly Color MediumOrchid = Color.FromBgra(4290401747u);
		public static readonly Color MediumPurple = Color.FromBgra(4287852763u);
		public static readonly Color MediumSeaGreen = Color.FromBgra(4282168177u);
		public static readonly Color MediumSlateBlue = Color.FromBgra(4286277870u);
		public static readonly Color MediumSpringGreen = Color.FromBgra(4278254234u);
		public static readonly Color MediumTurquoise = Color.FromBgra(4282962380u);
		public static readonly Color MediumVioletRed = Color.FromBgra(4291237253u);
		public static readonly Color MidnightBlue = Color.FromBgra(4279834992u);
		public static readonly Color MintCream = Color.FromBgra(4294311930u);
		public static readonly Color MistyRose = Color.FromBgra(4294960353u);
		public static readonly Color Moccasin = Color.FromBgra(4294960309u);
		public static readonly Color NavajoWhite = Color.FromBgra(4294958765u);
		public static readonly Color Navy = Color.FromBgra(4278190208u);
		public static readonly Color OldLace = Color.FromBgra(4294833638u);
		public static readonly Color Olive = Color.FromBgra(4286611456u);
		public static readonly Color OliveDrab = Color.FromBgra(4285238819u);
		public static readonly Color Orange = Color.FromBgra(4294944000u);
		public static readonly Color OrangeRed = Color.FromBgra(4294919424u);
		public static readonly Color Orchid = Color.FromBgra(4292505814u);
		public static readonly Color PaleGoldenrod = Color.FromBgra(4293847210u);
		public static readonly Color PaleGreen = Color.FromBgra(4288215960u);
		public static readonly Color PaleTurquoise = Color.FromBgra(4289720046u);
		public static readonly Color PaleVioletRed = Color.FromBgra(4292571283u);
		public static readonly Color PapayaWhip = Color.FromBgra(4294963157u);
		public static readonly Color PeachPuff = Color.FromBgra(4294957753u);
		public static readonly Color Peru = Color.FromBgra(4291659071u);
		public static readonly Color Pink = Color.FromBgra(4294951115u);
		public static readonly Color Plum = Color.FromBgra(4292714717u);
		public static readonly Color PowderBlue = Color.FromBgra(4289781990u);
		public static readonly Color Purple = Color.FromBgra(4286578816u);
		public static readonly Color Red = Color.FromBgra(4294901760u);
		public static readonly Color RosyBrown = Color.FromBgra(4290547599u);
		public static readonly Color RoyalBlue = Color.FromBgra(4282477025u);
		public static readonly Color SaddleBrown = Color.FromBgra(4287317267u);
		public static readonly Color Salmon = Color.FromBgra(4294606962u);
		public static readonly Color SandyBrown = Color.FromBgra(4294222944u);
		public static readonly Color SeaGreen = Color.FromBgra(4281240407u);
		public static readonly Color SeaShell = Color.FromBgra(4294964718u);
		public static readonly Color Sienna = Color.FromBgra(4288696877u);
		public static readonly Color Silver = Color.FromBgra(4290822336u);
		public static readonly Color SkyBlue = Color.FromBgra(4287090411u);
		public static readonly Color SlateBlue = Color.FromBgra(4285160141u);
		public static readonly Color SlateGray = Color.FromBgra(4285563024u);
		public static readonly Color Snow = Color.FromBgra(4294966010u);
		public static readonly Color SpringGreen = Color.FromBgra(4278255487u);
		public static readonly Color SteelBlue = Color.FromBgra(4282811060u);
		public static readonly Color Tan = Color.FromBgra(4291998860u);
		public static readonly Color Teal = Color.FromBgra(4278222976u);
		public static readonly Color Thistle = Color.FromBgra(4292394968u);
		public static readonly Color Tomato = Color.FromBgra(4294927175u);
		public static readonly Color Turquoise = Color.FromBgra(4282441936u);
		public static readonly Color Violet = Color.FromBgra(4293821166u);
		public static readonly Color Wheat = Color.FromBgra(4294303411u);
		public static readonly Color White = Color.FromBgra(uint.MaxValue);
		public static readonly Color WhiteSmoke = Color.FromBgra(4294309365u);
		public static readonly Color Yellow = Color.FromBgra(4294967040u);
		public static readonly Color YellowGreen = Color.FromBgra(4288335154u);
	}

	public sealed class ColorJsonConverter : JsonConverter<Color>
	{
		public static readonly ColorJsonConverter Instance = new ColorJsonConverter();

		public override Color Read(ref Utf8JsonReader rdr, Type typeToConvert, JsonSerializerOptions options)
		{
			return Read(ref rdr);
		}

		public static Color Read(ref Utf8JsonReader rdr)
		{
			Span<char> arr = stackalloc char[rdr.ValueSpan.Length];
			for (int i = 0; i < rdr.ValueSpan.Length; i++)
				arr[i] = (char)rdr.ValueSpan[i];
			return Color.FromHex(arr);
		}

		public static void Write(Utf8JsonWriter wrt, string propertyName, in Color value)
		{
			Span<char> arr = stackalloc char[8];
			var len = value.ToHex(arr);
			CoreException.Assert(len == 8);
			wrt.WriteString(propertyName, arr);
		}

		// SkipLocalsInit requires unsafe :-( 
		//[SkipLocalsInit]
		public override void Write(Utf8JsonWriter wrt, Color value, JsonSerializerOptions options)
		{
			Span<char> arr = stackalloc char[8];
			var len = value.ToHex(arr);
			CoreException.Assert(len == 8);
			wrt.WriteStringValue(arr);
		}
	}
}
