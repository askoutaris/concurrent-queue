
namespace ProducerConsumerQueue
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var queue = new ConsumerQueue<string>(onItemConsume, onItemError);

			queue.Start();

			for (int i = 0; i < 5; i++)
			{
				var queueIdx = i;
				Task.Run(async () =>
				{
					for (int j = 0; j < 5; j++)
					{
						await Task.Delay(1000);
						queue.Enqueue($"Item {j} from queue {queueIdx}");
					}
				});
			}

			Console.WriteLine("ProducerConsumerQueue Started");
			Console.ReadLine();

			Console.WriteLine("ProducerConsumerQueue Stopping...");
			queue.Stop();
			Console.WriteLine("ProducerConsumerQueue Stopped");
			Console.ReadLine();
		}

		private static async Task onItemConsume(string item)
		{
			await Task.Delay(50);

			Console.WriteLine(item);
		}

		private static async Task onItemError(Exception ex, string item)
		{
			await Task.Delay(50);

			Console.WriteLine(ex.Message);
		}
	}
}
