namespace Bezoro.Unity.Core.DI;

/// <summary>
///     Contract for objects that receive a dependency bundle after construction or
///     Unity deserialization.
/// </summary>
public interface IInjectable<in T>
{
	/// <summary>
	///     Checks whether the supplied dependencies are usable before
	///     <see cref="Inject" /> mutates the target.
	/// </summary>
	public bool ValidateDependencies(T dependencies, object? context = null);

	/// <summary>
	///     Applies a validated dependency bundle to the target object.
	/// </summary>
	public void Inject(T dependencies);
}
