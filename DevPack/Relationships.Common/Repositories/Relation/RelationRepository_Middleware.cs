namespace Skyline.DataMiner.Solutions.Relationships.Repositories
{
	using System;

	internal sealed partial class RelationRepository_Middleware : IDisposable
	{
		private bool disposedValue;

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposedValue)
			{
				return;
			}

			if (disposing)
			{
				(_middleware as IDisposable)?.Dispose();
				(_inner as IDisposable)?.Dispose();
			}

			disposedValue = true;
		}
	}
}
