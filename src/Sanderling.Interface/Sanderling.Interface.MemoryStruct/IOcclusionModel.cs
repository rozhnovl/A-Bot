using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	/// <summary>
	/// Optional capability a measurement can expose so the motor can compute click occlusion natively —
	/// without the reflection-based graph walk (which throws on the Eve64 parser model). Elements here are
	/// opaque top-level windows carrying their draw order via <see cref="IUIElement.InTreeIndex"/>; a window
	/// drawn in front of a click target (higher index) covers it.
	/// </summary>
	public interface IOcclusionModel
	{
		IReadOnlyList<IUIElement> WindowRegionsForOcclusion { get; }
	}
}
