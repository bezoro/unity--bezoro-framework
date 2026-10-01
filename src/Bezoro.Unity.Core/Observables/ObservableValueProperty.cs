namespace Bezoro.Unity.Core.Observables;

/// <summary>
///     Constrains ObservableProperty to value-type values.
/// </summary>
public abstract class ObservableValueProperty<T> : ObservableProperty<T>
	where T : struct { }
