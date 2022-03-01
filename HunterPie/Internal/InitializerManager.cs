﻿using HunterPie.Core.Logger;
using HunterPie.Domain.Interfaces;
using HunterPie.Internal.Initializers;
using System;
using System.Collections.Generic;

namespace HunterPie.Internal
{
    internal class InitializerManager
    {
        private static HashSet<IInitializer> _initializers = new()
        {
            // Core
            new LocalConfigInitializer(),
            new ClientConfigInitializer(),
            new ConfigManagerInitializer(),
            new HunterPieLoggerInitializer(),
            new FeatureFlagsInitializer(),

            new NativeLoggerInitializer(),
            new ExceptionCatcherInitializer(),
            new DialogManagerInitializer(),
            new UITracerInitializer(),
            new ClientLocalizationInitializer(),
            new SystemTrayInitializer(),

            // GUI
            new MenuInitializer(),
        };

        private static HashSet<IInitializer> _uiInitializers = new()
        {
            new HotkeyInitializer(),

            // Debugging
            new DebugWidgetInitializer(),
        };

        public static void Initialize()
        {
            Log.Benchmark();
            
            foreach (IInitializer initializer in _initializers)
                initializer.Init();

            Log.BenchmarkEnd();
        } 

        public static void InitializeGUI()
        {
            Log.Benchmark();

            foreach (IInitializer initializer in _uiInitializers)
                initializer.Init();

            Log.BenchmarkEnd();
        }

        public static void Unload()
        {
            foreach (IInitializer initializer in _initializers)
                if (initializer is IDisposable disposable)
                    disposable.Dispose();

            foreach (IInitializer initializer in _uiInitializers)
                if (initializer is IDisposable disposable)
                    disposable.Dispose();
        }
    }
}
