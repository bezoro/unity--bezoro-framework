using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bezoro.Unity.Core.AsyncLoading;

/// <summary>
///     Defines an item that can be loaded asynchronously by AsyncLoadingManager.
/// </summary>
public interface IAsyncLoadable
{
	bool    IsLoaded    { get; }
	int     Priority    { get; }
	string  LoadingName { get; }
	UniTask LoadAsync(CancellationToken ct);
}
