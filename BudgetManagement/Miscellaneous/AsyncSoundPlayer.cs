using System.Collections.Concurrent;

namespace BudgetManagement.Miscellaneous;

public enum SoundEffect
{
    Success,
    Error,
    Warning,
    Info
}

public sealed class AsyncSoundPlayer : IDisposable
{
    private readonly BlockingCollection<SoundEffect> soundQueue = new();
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly Task worker;

    public AsyncSoundPlayer()
    {
        worker = Task.Run(ProcessQueue);
    }

    public void Play(SoundEffect soundEffect)
    {
        if (!soundQueue.IsAddingCompleted)
        {
            soundQueue.Add(soundEffect);
        }
    }

    private void ProcessQueue()
    {
        try
        {
            foreach (var soundEffect in soundQueue.GetConsumingEnumerable(cancellationTokenSource.Token))
            {
                PlayEffect(soundEffect);
            }
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
    }

    private static void PlayEffect(SoundEffect soundEffect)
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            switch (soundEffect)
            {
                case SoundEffect.Success:
                    Console.Beep(900, 120);
                    break;
                case SoundEffect.Error:
                    Console.Beep(300, 250);
                    break;
                case SoundEffect.Warning:
                    Console.Beep(550, 180);
                    break;
                case SoundEffect.Info:
                    Console.Beep(700, 80);
                    break;
                default:
                    Console.Beep(600, 100);
                    break;
            }
        }
        catch
        {
            // If audio is not available, ignore and continue.
        }
    }

    public void Dispose()
    {
        soundQueue.CompleteAdding();
        cancellationTokenSource.Cancel();
        try
        {
            worker.Wait(500);
        }
        catch
        {
            // ignore shutdown errors
        }

        cancellationTokenSource.Dispose();
        soundQueue.Dispose();
    }
}
