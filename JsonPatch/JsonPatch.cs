﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Json.More;
using Json.Pointer;

namespace Json.Patch;

/// <summary>
/// Models a JSON Patch document.
/// </summary>
[JsonConverter(typeof(PatchJsonConverter))]
public class JsonPatch : IEquatable<JsonPatch>
{
#if NET8_0_OR_GREATER
	/// <summary>
	/// A TypeInfoResolver that can be used for serializing <see cref="JsonPointer"/> objects. Add to your custom
	/// JsonSerializerOptions's TypeInfoResolver or TypeInfoResolveChain.
	/// </summary>
	public static IJsonTypeInfoResolver TypeInfoResolver => PatchSerializerContext.OptionsManager.TypeInfoResolver;
#endif

	/// <summary>
	/// Gets the collection of operations.
	/// </summary>
	public IReadOnlyList<PatchOperation> Operations { get; }

	/// <summary>
	/// Creates a new instance of the <see cref="JsonPatch"/> class.
	/// </summary>
	/// <param name="operations">The collection of operations.</param>
	public JsonPatch(params PatchOperation[] operations)
	{
		Operations = operations.ToList().AsReadOnly();
	}

	/// <summary>
	/// Creates a new instance of the <see cref="JsonPatch"/> class.
	/// </summary>
	/// <param name="operations">The collection of operations.</param>
	public JsonPatch(IEnumerable<PatchOperation> operations)
	{
		Operations = operations.ToList().AsReadOnly();
	}

	/// <summary>
	/// Applies the patch to a JSON document.
	/// </summary>
	/// <param name="source">The JSON document.</param>
	/// <returns>A result object containing the output JSON and a possible error message.</returns>
	public PatchResult Apply(JsonNode? source)
	{
		var context = new PatchContext(source.Copy());

		foreach (var operation in Operations)
		{
			operation.Handle(context);
			if (context.Message != null) break;
			context.Index++;
		}

		return new PatchResult(context);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(JsonPatch? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;
		return Operations.SequenceEqual(other.Operations);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		return Equals(obj as JsonPatch);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Operations.GetCollectionHashCode();
	}
}

/// <summary>
/// Provides JSON conversion logic for <see cref="JsonPatch"/>.
/// </summary>
public class PatchJsonConverter : AotCompatibleJsonConverter<JsonPatch>
{
	/// <summary>Reads and converts the JSON to type <see cref="JsonPatch"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override JsonPatch Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var operations = JsonSerializer.Deserialize(ref reader, PatchSerializerContext.Default.ListPatchOperation)!;

		return new JsonPatch(operations);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, JsonPatch value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Operations, options);
	}
}

[JsonSerializable(typeof(JsonPatch))]
[JsonSerializable(typeof(PatchOperation))]
[JsonSerializable(typeof(OperationType))]
[JsonSerializable(typeof(PatchResult))]
[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(List<PatchOperation>))]
[JsonSerializable(typeof(IReadOnlyList<PatchOperation>))]
[JsonSerializable(typeof(PatchOperationJsonConverter.Model))]
internal partial class PatchSerializerContext : JsonSerializerContext
{
	public static TypeResolverOptionsManager OptionsManager { get; }

	static PatchSerializerContext()
	{
		OptionsManager = new TypeResolverOptionsManager(
#if NET8_0_OR_GREATER
			Default
#endif
		);
	}
}