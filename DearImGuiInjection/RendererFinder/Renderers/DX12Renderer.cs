﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CppInterop;
using Reloaded.Hooks;
using SharpDX.Direct3D12;
using SharpDX.DXGI;

namespace RendererFinder.Renderers;

public class DX12Renderer : IRenderer
{
#if NETSTANDARD2_0 || NET462
#else
    [DllImport("GameAssembly")]
    private static extern void il2cpp_thread_attach(IntPtr domain);
    [DllImport("GameAssembly")]
    private static extern IntPtr il2cpp_domain_get();

    public static void AttachThread()
    {
        il2cpp_thread_attach(il2cpp_domain_get());
    }
#endif

    // https://github.com/BepInEx/BepInEx/blob/master/Runtimes/Unity/BepInEx.Unity.IL2CPP/Hook/INativeDetour.cs#L54
    // Workaround for CoreCLR collecting all delegates
    private static readonly List<object> _cache = new();

    [Reloaded.Hooks.Definitions.X64.Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(Reloaded.Hooks.Definitions.X86.CallingConventions.Stdcall)]
    private delegate IntPtr CDXGISwapChainPresent1Delegate(IntPtr self, uint syncInterval, uint presentFlags, IntPtr presentParametersRef);

    private static readonly CDXGISwapChainPresent1Delegate _swapchainPresentHookDelegate = new(SwapChainPresentHook);
    private static Hook<CDXGISwapChainPresent1Delegate> _swapChainPresentHook;

    public static event Action<SwapChain3, uint, uint, IntPtr> OnPresent { add => _onPresentAction += value; remove => _onPresentAction -= value; }
    private static Action<SwapChain3, uint, uint, IntPtr> _onPresentAction;

    [Reloaded.Hooks.Definitions.X64.Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(Reloaded.Hooks.Definitions.X86.CallingConventions.Stdcall)]
    private delegate IntPtr CDXGISwapChainResizeBuffersDelegate(IntPtr self, int bufferCount, int width, int height, int newFormat, int swapchainFlags);

    private static readonly CDXGISwapChainResizeBuffersDelegate _swapChainResizeBufferHookDelegate = new(SwapChainResizeBuffersHook);
    private static Hook<CDXGISwapChainResizeBuffersDelegate> _swapChainResizeBufferHook;

    public static event Action<SwapChain3, int, int, int, int, int> PreResizeBuffers { add => _preResizeBuffers += value; remove => _preResizeBuffers -= value; }
    private static Action<SwapChain3, int, int, int, int, int> _preResizeBuffers;

    public static event Action<SwapChain3, int, int, int, int, int> PostResizeBuffers { add => _postResizeBuffers += value; remove => _postResizeBuffers -= value; }
    private static Action<SwapChain3, int, int, int, int, int> _postResizeBuffers;

    [Reloaded.Hooks.Definitions.X64.Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(Reloaded.Hooks.Definitions.X86.CallingConventions.Stdcall)]
    private delegate void CommandQueueExecuteCommandListDelegate(IntPtr self, uint numCommandLists, IntPtr ppCommandLists);

    private static readonly CommandQueueExecuteCommandListDelegate _commandQueueExecuteCommandListHookDelegate = new(CommandQueueExecuteCommandListHook);
    private static Hook<CommandQueueExecuteCommandListDelegate> _commandQueueExecuteCommandListHook;

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

    public unsafe bool Init()
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

        {
            _cache.Add(_swapchainPresentHookDelegate);

            _swapChainPresentHook = new(_swapchainPresentHookDelegate, swapChainVTable.TableEntries[Present1MethodTableIndex].FunctionPointer);
            _swapChainPresentHook.Activate();
        }

        {
            _cache.Add(_swapChainResizeBufferHookDelegate);

            _swapChainResizeBufferHook = new(_swapChainResizeBufferHookDelegate, swapChainVTable.TableEntries[ResizeBufferMethodTableIndex].FunctionPointer);
            _swapChainResizeBufferHook.Activate();
        }

        {
            _cache.Add(_commandQueueExecuteCommandListHookDelegate);

            _commandQueueExecuteCommandListHook = new(_commandQueueExecuteCommandListHookDelegate, commandQueueVTable.TableEntries[ExecuteCommandListTableIndex].FunctionPointer);
            _commandQueueExecuteCommandListHook.Activate();
        }

        return true;
    }

    public void Dispose()
    {
        _swapChainResizeBufferHook?.Disable();
        _swapChainResizeBufferHook = null;

        _commandQueueExecuteCommandListHook?.Disable();
        _commandQueueExecuteCommandListHook = null;

        _swapChainPresentHook?.Disable();
        _swapChainPresentHook = null;

        _onPresentAction = null;
    }

    private static IntPtr SwapChainPresentHook(IntPtr self, uint syncInterval, uint flags, IntPtr presentParameters)
    {
#if NETSTANDARD2_0 || NET462
#else
        AttachThread();
#endif
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

        return _swapChainPresentHook.OriginalFunction(self, syncInterval, flags, presentParameters);
    }

    private static IntPtr SwapChainResizeBuffersHook(IntPtr swapchainPtr, int bufferCount, int width, int height, int newFormat, int swapchainFlags)
    {
#if NETSTANDARD2_0 || NET462
#else
        AttachThread();
#endif
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

        var result = _swapChainResizeBufferHook.OriginalFunction(swapchainPtr, bufferCount, width, height, newFormat, swapchainFlags);

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

        _commandQueueExecuteCommandListHook.OriginalFunction(self, numCommandLists, ppCommandLists);

        // Investigate at some point why it's needed for unity...
        if (executedThings)
        {
            _commandQueueExecuteCommandListHook.Disable();
        }
    }
}