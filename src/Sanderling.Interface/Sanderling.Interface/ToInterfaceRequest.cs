namespace Sanderling.Interface;

public record ToInterfaceRequest
{
	public MemoryMeasurementInitParam? MemoryMeasurementInitTake { get; init; }

	public bool MemoryMeasurementInitGetLast { get; init; }

	public bool MemoryMeasurementTake { get; init; }

	public bool MemoryMeasurementGetLast { get; init; }
}
