using System.Collections.Generic;

namespace Bezoro.Framework.Unity.Utilities
{
	public class Dependency_Item<T>
	{
		public Dependency_Item( T item, IEnumerable<T> dependencies )
		{
			this.Item         = item;
			this.Dependencies = dependencies;
		}

		public T              Item         { get; }
		public IEnumerable<T> Dependencies { get; }
	}
}
