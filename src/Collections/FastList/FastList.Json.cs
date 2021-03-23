using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lidgren.Core
{
	/// <summary>
	/// Usage: Add instance of this class to myJsonSerializerOptions.Converters
	/// </summary>
	public class JsonConverterFactoryForFastListOfT : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert)
			=> typeToConvert.IsGenericType
			&& typeToConvert.GetGenericTypeDefinition() == typeof(FastList<>);

		public override JsonConverter CreateConverter(
			Type typeToConvert, JsonSerializerOptions options)
		{
			CoreException.Assert(typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(FastList<>));

			Type elementType = typeToConvert.GetGenericArguments()[0];

			JsonConverter converter = (JsonConverter)Activator.CreateInstance(
				typeof(JsonConverterForFastListOfT<>)
					.MakeGenericType(new Type[] { elementType }),
				BindingFlags.Instance | BindingFlags.Public,
				binder: null,
				args: null,
				culture: null)!;

			return converter;
		}
	}

	public class JsonConverterForFastListOfT<T> : JsonConverter<FastList<T>>
	{
		public override FastList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartArray)
			{
				throw new JsonException();
			}
			reader.Read();

			var elements = new FastList<T>();

			while (reader.TokenType != JsonTokenType.EndArray)
			{
				elements.Add(JsonSerializer.Deserialize<T>(ref reader, options));
				reader.Read();
			}

			return elements;
		}

		public override void Write(Utf8JsonWriter writer, FastList<T> value, JsonSerializerOptions options)
		{
			writer.WriteStartArray();

			var span = value.ReadOnlySpan;
			foreach(ref readonly var item in span)
				JsonSerializer.Serialize(writer, item, options);

			writer.WriteEndArray();
		}
	}
}
