#nullable enable

namespace Bezoro.Unity.Core.UI
{
	public readonly struct TextBubbleHandle
	{
		private readonly TextBubbleService? _service;

		internal TextBubbleHandle(TextBubbleService service, int id)
		{
			_service = service;
			Id       = id;
		}

		internal int Id { get; }

		internal bool IsOwnedBy(TextBubbleService service) =>
			ReferenceEquals(_service, service);

		public bool IsValid =>
			_service != null && _service.IsHandleValid(this);

		public bool SetText(string text) =>
			_service != null && _service.TrySetText(this, text);

		public bool SetAnchor(TextBubbleAnchor anchor) =>
			_service != null && _service.TrySetAnchor(this, anchor);

		public bool Show() =>
			_service != null && _service.TrySetVisible(this, true);

		public bool Hide() =>
			_service != null && _service.TrySetVisible(this, false);

		public bool Release() =>
			_service != null && _service.TryRelease(this);

		public bool Destroy() =>
			_service != null && _service.TryDestroy(this);
	}
}
