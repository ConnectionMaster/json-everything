﻿using System;
using System.ComponentModel.DataAnnotations;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

/// <summary>
/// Adds a `maxLength` and an optional `minLength` keyword.
/// </summary>
/// <remarks>
/// `minLength` will be not be added if the value is less than or equal to zero.
/// </remarks>
public class StringLengthAttributeHandler : IAttributeHandler<StringLengthAttribute>
{
	/// <summary>
	/// Processes the type and any attributes (present on the context), and adds
	/// intents to the context.
	/// </summary>
	/// <param name="context">The generation context.</param>
	/// <param name="attribute">The attribute.</param>
	/// <remarks>
	/// A common pattern is to implement <see cref="IAttributeHandler"/> on the
	/// attribute itself.  In this case, the <paramref name="attribute"/> parameter
	/// will be the same instance as the handler and can likely be ignored.
	/// </remarks>
	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		var length = (StringLengthAttribute)attribute;

		if (length.MinimumLength > 0)
			context.Intents.Add(new MinLengthIntent((uint)length.MinimumLength));
		context.Intents.Add(new MaxLengthIntent((uint)length.MaximumLength));
	}
}
