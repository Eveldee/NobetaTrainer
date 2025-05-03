﻿using System;
using System.Linq;
using BepInEx.Unity.IL2CPP.Hook;
using CppInterop;
using Il2CppInterop.Runtime;
using SharpDX.Direct3D12;
using SharpDX.DXGI;

namespace RendererFinder.Renderers;

public class DX12Renderer : IRenderer
{
    public static void AttachThread()
    {
        IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
    }

    private delegate IntPtr CDXGISwapChainPresent1Delegate(IntPtr self, uint syncInterval, uint presentFlags, IntPtr presentParametersRef);

    private static readonly CDXGISwapChainPresent1Delegate _swapchainPresentHookDelegate = new(SwapChainPresentHook);
    private static CDXGISwapChainPresent1Delegate _swapchainPresentHookOriginal;
    private static INativeDetour _swapChainPresentHook;

    public static event Action<SwapChain3, uint, uint, IntPtr> OnPresent { add => _onPresentAction += value; remove => _onPresentAction -= value; }
    private static Action<SwapChain3, uint, uint, IntPtr> _onPresentAction;

    private delegate IntPtr CDXGISwapChainResizeBuffersDelegate(IntPtr self, int bufferCount, int width, int height, int newFormat, int swapchainFlags);

    private static readonly CDXGISwapChainResizeBuffersDelegate _swapChainResizeBufferHookDelegate = new(SwapChainResizeBuffersHook);
    private static CDXGISwapChainResizeBuffersDelegate _swapChainResizeBufferHookOriginal;
    private static INativeDetour _swapChainResizeBufferHook;

    public static event Action<SwapChain3, int, int, int, int, int> PreResizeBuffers { add => _preResizeBuffers += value; remove => _preResizeBuffers -= value; }
    private static Action<SwapChain3, int, int, int, int, int> _preResizeBuffers;

    public static event Action<SwapChain3, int, int, int, int, int> PostResizeBuffers { add => _postResizeBuffers += value; remove => _postResizeBuffers -= value; }
    private static Action<SwapChain3, int, int, int, int, int> _postResizeBuffers;

    private delegate void CommandQueueExecuteCommandListDelegate(IntPtr self, uint numCommandLists, IntPtr ppCommandLists);

    private static readonly CommandQueueExecuteCommandListDelegate _commandQueueExecuteCommandListHookDelegate = new(CommandQueueExecuteCommandListHook);
    private static CommandQueueExecuteCommandListDelegate _commandQueueExecuteCommandListHookOriginal;
    private static INativeDetour _commandQueueExecuteCommandListHook;

    public static event Func<CommandQueue, uint, IntPtr, bool> OnExecuteCommandList
    {
        add
        {
            lock (_onExecuteCommandListActionLock)
            {
                OnExecuteCommandListAction += value;
            }
        }
        remove
        {
            lock (_onExecuteCommandListActionLock)
            {
                OnExecuteCommandListAction -= value;
            }
        }
    }
    private static readonly object _onExecuteCommandListActionLock = new();

    private static event Func<CommandQueue, uint, IntPtr, bool> OnExecuteCommandListAction;

    public bool Init()
    {
        var windowHandle = Windows.User32.CreateFakeWindow();

        var desc = new SwapChainDescription()
        {
            BufferCount = 2,
            ModeDescription = new ModeDescription(500, 300, new Rational(60, 1), Format.R8G8B8A8_UNorm),
            Usage = Usage.RenderTargetOutput,
            SwapEffect = SwapEffect.FlipDiscard,
            OutputHandle = windowHandle,
            SampleDescription = new SampleDescription(1, 0),
            IsWindowed = true
        };

        using var factory = new Factory4();

        var device = new SharpDX.Direct3D12.Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0);
        CommandQueueDescription queueDesc = new CommandQueueDescription(CommandListType.Direct);
        var commandQueue = device.CreateCommandQueue(queueDesc);
        var tempSwapChain = new SwapChain(factory, commandQueue, desc);
        var swapChain = tempSwapChain.QueryInterface<SwapChain3>();
        tempSwapChain.Dispose();

        const int Present1MethodTableIndex = 22;
        const int ResizeBufferMethodTableIndex = 13;
        // not accurate, probably, don't care. Just make sure to extend it if needed
        const int SwapChainFunctionCount = Present1MethodTableIndex + 1;
        var swapChainVTable = VirtualFunctionTable.FromObject((nuint)(nint)swapChain.NativePointer, SwapChainFunctionCount);

        const int ExecuteCommandListTableIndex = 10;
        // not accurate, probably, don't care. Just make sure to extend it if needed
        const int CommandListFunctionCount = ExecuteCommandListTableIndex + 1;
        var commandQueueVTable = VirtualFunctionTable.FromObject((nuint)(nint)commandQueue.NativePointer, CommandListFunctionCount);

        swapChain.Dispose();
        commandQueue.Dispose();
        device.Dispose();

        Windows.User32.DestroyWindow(windowHandle);

        _swapChainPresentHook = INativeDetour.CreateAndApply(
            (nint)swapChainVTable.TableEntries[Present1MethodTableIndex].FunctionPointer,
            _swapchainPresentHookDelegate,
            out _swapchainPresentHookOriginal
        );

        _swapChainResizeBufferHook = INativeDetour.CreateAndApply(
            (nint)swapChainVTable.TableEntries[ResizeBufferMethodTableIndex].FunctionPointer,
            _swapChainResizeBufferHookDelegate,
            out _swapChainResizeBufferHookOriginal
        );

        _commandQueueExecuteCommandListHook = INativeDetour.CreateAndApply(
            (nint)commandQueueVTable.TableEntries[ExecuteCommandListTableIndex].FunctionPointer,
            _commandQueueExecuteCommandListHookDelegate,
            out _commandQueueExecuteCommandListHookOriginal
        );

        return true;
    }

    public void Dispose()
    {
        _swapChainResizeBufferHook?.Dispose();
        _swapChainResizeBufferHook = null;

        _commandQueueExecuteCommandListHook?.Dispose();
        _commandQueueExecuteCommandListHook = null;

        _swapChainPresentHook?.Dispose();
        _swapChainPresentHook = null;

        _onPresentAction = null;
    }

    private static IntPtr SwapChainPresentHook(IntPtr self, uint syncInterval, uint flags, IntPtr presentParameters)
    {
        AttachThread();

        var swapChain = new SwapChain3(self);

        if (_onPresentAction != null)
        {
            foreach (Action<SwapChain3, uint, uint, IntPtr> item in _onPresentAction.GetInvocationList().Cast<Action<SwapChain3, uint, uint, IntPtr>>())
            {
                try
                {
                    item(swapChain, syncInterval, flags, presentParameters);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        return _swapchainPresentHookOriginal(self, syncInterval, flags, presentParameters);
    }

    private static IntPtr SwapChainResizeBuffersHook(IntPtr swapchainPtr, int bufferCount, int width, int height, int newFormat, int swapchainFlags)
    {
        AttachThread();

        var swapChain = new SwapChain3(swapchainPtr);

        if (_preResizeBuffers != null)
        {
            foreach (Action<SwapChain3, int, int, int, int, int> item in _preResizeBuffers.GetInvocationList().Cast<Action<SwapChain3, int, int, int, int, int>>())
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

        var result = _swapChainResizeBufferHookOriginal(swapchainPtr, bufferCount, width, height, newFormat, swapchainFlags);

        if (_postResizeBuffers != null)
        {
            foreach (Action<SwapChain3, int, int, int, int, int> item in _postResizeBuffers.GetInvocationList().Cast<Action<SwapChain3, int, int, int, int, int>>())
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

    private static void CommandQueueExecuteCommandListHook(IntPtr self, uint numCommandLists, IntPtr ppCommandLists)
    {
        var executedThings = false;

        lock (_onExecuteCommandListActionLock)
        {
            if (OnExecuteCommandListAction != null)
            {
                var commandQueue = new CommandQueue(self);

                foreach (Func<CommandQueue, uint, IntPtr, bool> item in OnExecuteCommandListAction.GetInvocationList().Cast<Func<CommandQueue, uint, IntPtr, bool>>())
                {
                    try
                    {
                        var res = item(commandQueue, numCommandLists, ppCommandLists);
                        if (res)
                        {
                            executedThings = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        _commandQueueExecuteCommandListHookOriginal(self, numCommandLists, ppCommandLists);

        // Investigate at some point why it's needed for unity...
        if (executedThings)
        {
            _commandQueueExecuteCommandListHook.Dispose();
        }
    }
}