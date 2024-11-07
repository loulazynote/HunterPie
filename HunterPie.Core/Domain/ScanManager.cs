using HunterPie.Core.Architecture;
using HunterPie.Core.Client;
using HunterPie.Core.Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HunterPie.Core.Domain;

public static class ScanManager
{
    private static Thread _thread;
    private static CancellationTokenSource _token = new();
    private static readonly HashSet<Scannable> Scannables = new();

    // Metrics

    public static readonly Observable<long> ScanTime = 0;

    internal static void Start()
    {
        if (_thread is not null)
            return;

        _thread = new Thread(async () =>
        {
            //do
            //try
            //{
            //    await RunScanLoopAsync();
            //var sw = Stopwatch.StartNew();

            //Scan();
            //sw.Stop();
            //ScanTime.Value = sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000);

            //if (_token.IsCancellationRequested)
            //    break;

            //Thread.Sleep((int)ClientConfig.Config.Client.PollingRate.Current);
            //}
            //catch (Exception err)
            //{
            // Logs the error if it came from a generic exception instead of a
            // cancel request
            //    Log.Error(err.ToString());
            //}
            //while (true);

            try
            {
                await RunScanLoopAsync(); // 將主要掃描邏輯移至另一個非同步方法
            }
            catch (Exception err)
            {
                Log.Error(err.ToString());
            }
            finally
            {
                _token = new();
            }

            //_token = new();
        })
        {
            Name = "ScanManager",
            IsBackground = true,
            //Priority = ThreadPriority.AboveNormal
            Priority = ThreadPriority.Normal
        };
        _thread.Start();
    }

    internal static void Stop()
    {
        if (_thread is null)
            return;

        lock (Scannables)
        {
            Scannables.Clear();
            _token.Cancel();
            _thread = null;
        }
    }

    private static async Task Scan()
    {

        Scannable[] readOnlyScannables = Scannables.ToArray();
        //var tasks = new Task[readOnlyScannables.Length];

        using var semaphore = new SemaphoreSlim(Environment.ProcessorCount);

        var tasks = readOnlyScannables.Select(async scannable =>
        {
            await semaphore.WaitAsync();
            try
            {
                await Task.Run(scannable.Scan);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        //for (int i = 0; i < readOnlyScannables.Length; i++)
        //    tasks[i] = Task.Run(readOnlyScannables[i].Scan);

        //Task.WaitAll(tasks);
    }

    public static void Add(params Scannable[] scannableList)
    {
        foreach (Scannable scannable in scannableList)
            Add(scannable);
    }

    public static void Add(Scannable scannable)
    {
        lock (Scannables)
        {
            if (Scannables.Contains(scannable))
                return;

            _ = Scannables.Add(scannable);
        }
    }

    public static void Remove(Scannable scannable)
    {
        lock (Scannables)
        {
            if (!Scannables.Contains(scannable))
                return;

            _ = Scannables.Remove(scannable);
        }
    }

    private static async Task RunScanLoopAsync()
    {
        while (!_token.IsCancellationRequested)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                await Scan();  // 假設 Scan() 已被修改為非同步方法

                sw.Stop();
                ScanTime.Value = sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000);

                int delay = Math.Max(
                    (int)ClientConfig.Config.Client.PollingRate.Current,
                    CalculateOptimalDelay(sw.ElapsedMilliseconds)
                );

                await Task.Delay(delay, _token.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception err)
            {
                Log.Error(err.ToString());
                await Task.Delay(100, _token.Token);
            }
        }
    }

    private static int CalculateOptimalDelay(long lastScanTime)
    {
        const int MIN_DELAY = 16;
        const int MAX_DELAY = 100;

        if (lastScanTime > 50)
            return MAX_DELAY;

        return MIN_DELAY;
    }
}
