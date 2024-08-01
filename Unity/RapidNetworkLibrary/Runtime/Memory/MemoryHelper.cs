﻿using RapidNetworkLibrary.Logging;
using Smmalloc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

public static class MemoryHelper
{
    private static SmmallocInstance smalloc;
    public static unsafe IntPtr Write<T>(T value) where T : unmanaged
    {

        var ptr = Alloc(Marshal.SizeOf<T>());
        var s = new Span<T>(ptr.ToPointer(),1);
        s[0] = value;
        return ptr;
    }

    public static unsafe T Read<T>(IntPtr ptr) where T : unmanaged
    {
        var s = new ReadOnlySpan<T>(ptr.ToPointer(), 1);
        return s[0];
    }

    static volatile int memoryUsed = 0;


   
    public static unsafe IntPtr Alloc(int size)
    {
        if (size > smalloc.allocationLimit)
            throw new Exception("Cannot allocate more than " + smalloc.allocationLimit + " bytes of memory at one time using smmalloc!");

        Interlocked.Increment(ref memoryUsed);
        return smalloc.Malloc(size);
    }

    public static unsafe void Free(IntPtr ptr)
    {
        var val = Interlocked.Decrement(ref memoryUsed);
        Logger.Log(LogLevel.Warning, val + " number of allocations persisting.");
        smalloc.Free(ptr);
    }

    internal static void SetMalloc(SmmallocInstance malloc)
    {
        smalloc = malloc;
    }
}
