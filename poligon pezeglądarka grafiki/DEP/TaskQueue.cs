
using System.Threading.Channels;


namespace poligon_pezeglądarka_grafiki.Model;

public class TaskQueue
{
    private readonly Channel<Func<Task>> _queue = Channel.CreateUnbounded<Func<Task>>();
    private readonly Task _completionTask;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    // Dodatkowa właściwość pokazująca liczbę oczekujących zadań
    public int Count => _queue.Reader.Count;

    public TaskQueue()
    {
        _completionTask = Task.Run(ProcessQueueAsync);
    }

    public void Enqueue(Func<Task> job)
    {
        _ = job ?? throw new ArgumentNullException(nameof(job));
        _queue.Writer.TryWrite(job);
    }

    /// <summary>
    /// Czyści wszystkie oczekujące zadania z kolejki.
    /// Zadanie, które aktualnie się wykonuje, NIE zostanie przerwane.
    /// </summary>
    public int Clear()
    {
        int removedCount = 0;

        // TryRead natychmiast wyciąga element z kolejki bez blokowania wątku.
        // Wykonujemy to w pętli, dopóki w kanale są jakieś elementy.
        while (_queue.Reader.TryRead(out _))
        {
            removedCount++;
        }

        return removedCount; // Zwracamy informację, ile zadań zostało usuniętych
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            await foreach (var job in _queue.Reader.ReadAllAsync(_cts.Token))
            {
                try
                {
                    await job();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas wykonywania zadania: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Obsługa prawidłowego wyłączenia przez CancellationToken
        }
    }

    public async Task StopAsync()
    {
        _queue.Writer.Complete();
        _cts.Cancel(); // Przerywa oczekiwanie na nowe elementy

        try
        {
            await _completionTask;
        }
        catch (OperationCanceledException) { }
        finally
        {
            _cts.Dispose();
        }
    }
}
