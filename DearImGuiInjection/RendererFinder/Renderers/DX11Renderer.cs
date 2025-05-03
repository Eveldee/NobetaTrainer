﻿using System;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Hook;
using CppInterop;
using Il2CppInterop.Runtime;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace RendererFinder.Renderers;

/// <summary>
/// Contains a full list of IDXGISwapChain functions to be used
/// as an indexer into the SwapChain Virtual Function Table entries.
/// </summary>
public enum IDXGISwapChain
{
    // IUnknown
    QueryInterface = 0,
    AddRef = 1,
    Release = 2,

    // IDXGIObject
    SetPrivateData = 3,
    SetPrivateDataInterface = 4,
    GetPrivateData = 5,
    GetParent = 6,

    // IDXGIDeviceSubObject
    GetDevice = 7,

    // IDXGISwapChain
    Present = 8,
    GetBuffer = 9,
    SetFullscreenState = 10,
    GetFullscreenState = 11,
    GetDesc = 12,
    ResizeBuffers = 13,
    ResizeTarget = 14,
    GetContainingOutput = 15,
    GetFrameStatistics = 16,
    GetLastPresentCount = 17,
}

public class DX11Renderer : IRenderer
{
    public static void AttachThread()
    {
        IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
    }

    // SwapChainPresent hook
    private delegate IntPtr CDXGISwapChainPresentDelegate(IntPtr self, uint syncInterval, uint flags);

    private static readonly CDXGISwapChainPresentDelegate _swapChainPresentHookDelegate = new(SwapChainPresentHook);
    private static CDXGISwapChainPresentDelegate _swapChainPresentHookOriginal;
    private static INativeDetour _swapChainPresentHook;

    public static event Action<SwapChain, uint, uint> OnPresent { add { _onPresentAction += value; } remove { _onPresentAction -= value; } }
    private static Action<SwapChain, uint, uint> _onPresentAction;

    // SwapChainResizeBuffer hook
    private delegate IntPtr CDXGISwapChainResizeBuffersDelegate(IntPtr self, uint bufferCount, uint width, uint height, Format newFormat, uint swapchainFlags);

    private static readonly CDXGISwapChainResizeBuffersDelegate _swapChainResizeBuffersHookDelegate = new(SwapChainResizeBuffersHook);
    private static CDXGISwapChainResizeBuffersDelegate _swapChainResizeBuffersHookOriginal;
    private static INativeDetour _swapChainResizeBuffersHook;

    public static event Action<SwapChain, uint, uint, uint, Format, uint> PreResizeBuffers { add { _preResizeBuffers += value; } remove { _preResizeBuffers -= value; } }
    private static Action<SwapChain, uint, uint, uint, Format, uint> _preResizeBuffers;

    public static event Action<SwapChain, uint, uint, uint, Format, uint> PostResizeBuffers { add { _postResizeBuffers += value; } remove { _postResizeBuffers -= value; } }
    private static Action<SwapChain, uint, uint, uint, Format, uint> _postResizeBuffers;

    public bool Init()
    {
        Log.Info("DX11Renderer.Init()");

        var windowHandle = Windows.User32.CreateFakeWindow();

        var desc = new SwapChainDescription()
        {
            BufferCount = 1,
            ModeDescription = new ModeDescription(500, 300, new Rational(60, 1), Format.R8G8B8A8_UNorm),
            IsWindowed = true,
            OutputHandle = windowHandle,
            SampleDescription = new SampleDescription(1, 0),
            Usage = Usage.RenderTargetOutput
        };

        Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out var device, out var swapChain);
        var swapChainVTable = VirtualFunctionTable.FromObject((nuint)(nint)swapChain.NativePointer, (nuint)Enum.GetNames(typeof(IDXGISwapChain)).Length);
        var swapChainPresentFunctionPtr = swapChainVTable.TableEntries[(int)IDXGISwapChain.Present].FunctionPointer;
        var swapChainResizeBuffersFunctionPtr = swapChainVTable.TableEntries[(int)IDXGISwapChain.ResizeBuffers].FunctionPointer;

        swapChain.Dispose();
        device.Dispose();

        Windows.User32.DestroyWindow(windowHandle);

        _swapChainPresentHook = INativeDetour.CreateAndApply(
            (nint)swapChainPresentFunctionPtr,
            _swapChainPresentHookDelegate,
            out _swapChainPresentHookOriginal
        );

        _swapChainResizeBuffersHook = INativeDetour.CreateAndApply(
            (nint)swapChainResizeBuffersFunctionPtr,
            _swapChainResizeBuffersHookDelegate,
            out _swapChainResizeBuffersHookOriginal
        );

        Log.Info("DX11Renderer.Init() end");

        return true;
    }

    public void Dispose()
    {
        _swapChainResizeBuffersHook?.Dispose();
        _swapChainResizeBuffersHook = null;

        _swapChainPresentHook?.Dispose();
        _swapChainPresentHook = null;

        _onPresentAction = null;
    }

    private static IntPtr SwapChainPresentHook(IntPtr self, uint syncInterval, uint flags)
    {
        AttachThread();

        var swapChain = new SwapChain(self);

        if (_onPresentAction != null)
        {
            foreach (Action<SwapChain, uint, uint> item in _onPresentAction.GetInvocationList())
            {
                try
                {
                    item(swapChain, syncInterval, flags);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        return _swapChainPresentHookOriginal(self, syncInterval, flags);
    }

    private static IntPtr SwapChainResizeBuffersHook(IntPtr swapchainPtr, uint bufferCount, uint width, uint height, Format newFormat, uint swapchainFlags)
    {
        AttachThread();

        var swapChain = new SwapChain(swapchainPtr);

        if (_preResizeBuffers != null)
        {
            foreach (Action<SwapChain, uint, uint, uint, Format, uint> item in _preResizeBuffers.GetInvocationList())
            {
                try
                {
                    item(swapChain, bufferCount, width, height, newFormat, swapchainFlags);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        var result = _swapChainResizeBuffersHookOriginal(swapchainPtr, bufferCount, width, height, newFormat, swapchainFlags);

        if (_postResizeBuffers != null)
        {
            foreach (Action<SwapChain, uint, uint, uint, Format, uint> item in _postResizeBuffers.GetInvocationList())
            {
                try
                {
                    item(swapChain, bufferCount, width, height, newFormat, swapchainFlags);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        return result;
    }
}