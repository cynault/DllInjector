using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;

namespace DllInject;

public class Injector
{
    private readonly SafeHandle _processHandle;

    private Injector(SafeHandle handle)
    {
        _processHandle = handle;
    }

    public Injector()
    {
        _processHandle = PInvoke.OpenProcess_SafeHandle(
            PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, 
            false,
            PInvoke.GetCurrentProcessId()
            );
    }

    public static Injector BindProcess(uint processId)
    {
        var processHandle = PInvoke.OpenProcess_SafeHandle(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, processId);
        if (processHandle.DangerousGetHandle() == IntPtr.Zero) throw new Exception("Could not open process");
        return new Injector(processHandle);
    }

    public static Injector BindProcess(string windowName)
    {
        var processId = FindWindowProcessId(windowName);
        var processHandle = PInvoke.OpenProcess_SafeHandle(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, processId);
        if (processHandle.DangerousGetHandle() == IntPtr.Zero) throw new Exception("Could not open process");
        return new Injector(processHandle);
    }

    //find windows windows process by window name
    private unsafe static uint FindWindowProcessId(string windowName, string? className = default)
    {
        var windowHandle = PInvoke.FindWindow(className, windowName);
        if (windowHandle.IsNull) throw new Exception("Could not find window");
        uint processId;
        PInvoke.GetWindowThreadProcessId(windowHandle, &processId);
        return processId;
    }


    private unsafe void* CopyToTargetProcess(byte[] content)
    {
        var allocMemPtr = PInvoke.VirtualAllocEx(
            _processHandle,
            default,
            (nuint)content.Length,
            VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT,
            PAGE_PROTECTION_FLAGS.PAGE_READWRITE);
        fixed (void* ptr = content)
        {
            var result = PInvoke.WriteProcessMemory(_processHandle, allocMemPtr, ptr, (uint)content.Length, default);
            if (!result) throw new Exception("Could not write to process memory");
        }

        return allocMemPtr;
    }
    
    private static TDelegate GetModuleFunctionDelegate<TDelegate>(string moduleName, string procName)where TDelegate:Delegate
    {
        var procAddress = PInvoke.GetProcAddress(PInvoke.GetModuleHandle(moduleName), procName);
        if (procAddress.IsNull) 
            throw new Exception("Could not get module function address");
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(procAddress);
    }
    
    public SafeHandle InjectLibrary(string dllName)
    {
        unsafe
        {
            var content = Encoding.ASCII.GetBytes(dllName).ToList();
            content.Add(0);
            var memPtr = CopyToTargetProcess(content.ToArray());
            var loadLibraryFunction = GetModuleFunctionDelegate<LPTHREAD_START_ROUTINE>("kernel32.dll", "LoadLibraryA");
            var remoteThread = PInvoke.CreateRemoteThread(
                _processHandle, 
                default,
                0,
                loadLibraryFunction,
                memPtr, 
                0, 
                default
                );
            PInvoke.WaitForSingleObject(remoteThread, 0xFFFFFFFF);
            return remoteThread;
        }
    }
}