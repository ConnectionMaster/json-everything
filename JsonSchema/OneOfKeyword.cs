﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `oneOf`.
	/// </summary>
	[Applicator]
	[SchemaPriority(20)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[Vocabulary(Vocabularies.Applicator202012Id)]
	[JsonConverter(typeof(OneOfKeywordJsonConverter))]
	public class OneOfKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaCollector, IEquatable<OneOfKeyword>
	{
		internal const string Name = "oneOf";

		/// <summary>
		/// The keywords schema collection.
		/// </summary>
		public IReadOnlyList<JsonSchema> Schemas { get; }

		/// <summary>
		/// Creates a new <see cref="OneOfKeyword"/>.
		/// </summary>
		/// <param name="values">The keywords schema collection.</param>
		public OneOfKeyword(params JsonSchema[] values)
		{
			Schemas = values.ToList() ?? throw new ArgumentNullException(nameof(values));
		}

		/// <summary>
		/// Creates a new <see cref="OneOfKeyword"/>.
		/// </summary>
		/// <param name="values">The keywords schema collection.</param>
		public OneOfKeyword(IEnumerable<JsonSchema> values)
		{
			Schemas = values.ToList();
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			var validCount = 0;
			for (var i = 0; i < Schemas.Count; i++)
			{
				context.Log(() => $"Processing {Name}[{i}]...");
				var schema = Schemas[i];
				var subContext = ValidationContext.From(context, subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create($"{i}")));
				schema.ValidateSubschema(subContext);
				validCount += subContext.IsValid ? 1 : 0;
				context.Log(() => $"{Name}[{i}] {subContext.IsValid.GetValidityString()}.");
				if (validCount > 1 && context.ApplyOptimizations) break;
				context.NestedContexts.Add(subContext);
			}

			context.ConsolidateAnnotations();
			context.IsValid = validCount == 1;
			if (!context.IsValid)
				context.Message = $"Expected 1 matching subschema but found {validCount}";
			context.ExitKeyword(Name, context.IsValid);
		}

		IRefResolvable? IRefResolvable.ResolvePointerSegment(string? value)
		{
			if (!int.TryParse(value, out var index)) return null;
			if (index < 0 || Schemas.Count <= index) return null;

			return Schemas[index];
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var schema in Schemas)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(OneOfKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Schemas.ContentsEqual(other.Schemas);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as OneOfKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Schemas.GetUnorderedCollectionHashCode();
		}
	}

	internal class OneOfKeywordJsonConverter : JsonConverter<OneOfKeyword>
	{
		public override OneOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options);
				return new OneOfKeyword(schemas);
			}
			
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
			return new OneOfKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, OneOfKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(OneOfKeyword.Name);
			writer.WriteStartArray();
			foreach (var schema in value.Schemas)
			{
				JsonSerializer.Serialize(writer, schema, options);
			}
			writer.WriteEndArray();
		}
	}
}