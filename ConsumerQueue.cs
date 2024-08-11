using System.Collections.Concurrent;

namespace ProducerConsumerQueue
{
	public class ConsumerQueue<T>
	{
		private readonly ConcurrentQueue<T> _queue;
		private readonly Func<T, Task> _onItemConsume;
		private readonly Func<Exception, T, Task> _onItemError;
		private readonly AutoResetEvent _sync;
		private CancellationTokenSource _tokenSource;
		private Task? _task;

		public ConsumerQueue(Func<T, Task> onItemConsume, Func<Exception, T, Task> onItemError)
		{
			_queue = new ConcurrentQueue<T>();
			_sync = new AutoResetEvent(false);
			_tokenSource = new CancellationTokenSource();
			_onItemConsume = onItemConsume;
			_onItemError = onItemError;
		}

		public void Enqueue(T item)
		{
			_queue.Enqueue(item);

			_sync.Set();
		}

		private async Task Consume()
		{
			_sync.WaitOne();

			while (!_tokenSource.IsCancellationRequested && _queue.TryDequeue(out T? item))
			{
				try
				{
					await _onItemConsume.Invoke(item);
				}
				catch (Exception ex)
				{
					await _onItemError(ex, item);
				}
			}
		}

		public void Start()
		{
			lock (_sync)
			{
				if (_tokenSource.IsCancellationRequested)
					_tokenSource = new CancellationTokenSource();

				_task = Task.Run(async () =>
				{
					while (!_tokenSource.IsCancellationRequested)
						await Consume();
				});
			}
		}

		public void Stop()
		{
			lock (_sync)
			{
				_tokenSource.Cancel();
				_sync.Set();
				_task?.GetAwaiter().GetResult();
			}
		}
	}
}
