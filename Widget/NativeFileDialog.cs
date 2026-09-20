using System.Runtime.InteropServices;
using System.Text;

namespace Links;

internal static class NativeFileDialog
{
    private const int MaxPathChars = 1024;
    private const int OfnExplorer = 0x00080000;
    private const int OfnFileMustExist = 0x00001000;
    private const int OfnPathMustExist = 0x00000800;
    private const int OfnOverwritePrompt = 0x00000002;
    private const int OfnHideReadOnly = 0x00000004;
    private const int OfnNoChangeDir = 0x00000008;

    internal static string? OpenXml() =>
        RunSta(() => Show(
            title: "Import links",
            save: false,
            defaultFileName: null));

    internal static string? SaveXml() =>
        RunSta(() => Show(
            title: "Export links",
            save: true,
            defaultFileName: "links.xml"));

    private static string? RunSta(Func<string?> action)
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            return action();
        }

        string? result = null;
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (error is not null)
        {
            throw error;
        }

        return result;
    }

    private static string? Show(string title, bool save, string? defaultFileName)
    {
        var filter = AllocUni("XML files (*.xml)\0*.xml\0All files (*.*)\0*.*\0\0");
        var titlePtr = Marshal.StringToHGlobalUni(title);
        var defExtPtr = Marshal.StringToHGlobalUni("xml");
        var filePtr = Marshal.AllocHGlobal(MaxPathChars * 2);
        try
        {
            NativeMethods.ZeroMemory(filePtr, (UIntPtr)(MaxPathChars * 2));
            if (!string.IsNullOrEmpty(defaultFileName))
            {
                var bytes = Encoding.Unicode.GetBytes(defaultFileName);
                Marshal.Copy(bytes, 0, filePtr, bytes.Length);
            }

            var ofn = new OpenFileName
            {
                lStructSize = Marshal.SizeOf<OpenFileName>(),
                hwndOwner = NativeMethods.GetForegroundWindow(),
                lpstrFilter = filter,
                nFilterIndex = 1,
                lpstrFile = filePtr,
                nMaxFile = MaxPathChars,
                lpstrTitle = titlePtr,
                lpstrDefExt = defExtPtr,
                Flags = OfnExplorer | OfnPathMustExist | OfnHideReadOnly | OfnNoChangeDir
                    | (save ? OfnOverwritePrompt : OfnFileMustExist),
            };

            var ok = save ? NativeMethods.GetSaveFileName(ref ofn) : NativeMethods.GetOpenFileName(ref ofn);
            return ok ? Marshal.PtrToStringUni(filePtr) : null;
        }
        finally
        {
            Marshal.FreeHGlobal(filter);
            Marshal.FreeHGlobal(titlePtr);
            Marshal.FreeHGlobal(defExtPtr);
            Marshal.FreeHGlobal(filePtr);
        }
    }

    private static IntPtr AllocUni(string value)
    {
        var bytes = Encoding.Unicode.GetBytes(value);
        var ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        return ptr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public IntPtr lpstrFilter;
        public IntPtr lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public IntPtr lpstrFileTitle;
        public int nMaxFileTitle;
        public IntPtr lpstrInitialDir;
        public IntPtr lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public IntPtr lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public IntPtr lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }

    private static class NativeMethods
    {
        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetOpenFileName(ref OpenFileName ofn);

        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetSaveFileName(ref OpenFileName ofn);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr dest, UIntPtr length);
    }
}
