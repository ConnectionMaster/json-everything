using System.Collections.Generic;

namespace Json.Schema.Generation.XmlComments;

/// <summary>
///     Enum type comments
/// </summary>
internal class EnumComments : CommonComments
{
	/// <summary>
	///     "summary" comments of enum values. List contains names, values and
	///     comments for each enum value.
	///     If none of values have any summary comments then this list may be empty.
	///     If at least one value has summary comment then this list contains
	///     all enum values with empty comments for values without comments.
	/// </summary>
	public List<EnumValueComment> ValueComments { get; set; } = [];
}
