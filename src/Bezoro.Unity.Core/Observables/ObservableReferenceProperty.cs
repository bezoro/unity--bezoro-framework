namespace Bezoro.Unity.Core.Observables;

/// <summary>
///     Constrains ObservableProperty to reference-type values.
/// </summary>
public abstract class ObservableReferenceProperty<T> : ObservableProperty<T>
	where T : class { }
