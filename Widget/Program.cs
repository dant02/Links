using System.Runtime.InteropServices;
using COM;

namespace Links;

internal static class Program
{
    [DllImport("ole32.dll")]
    private static extern int CoRegisterClassObject(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
        [MarshalAs(UnmanagedType.IUnknown)] object pUnk,
        uint dwClsContext,
        uint flags,
        out uint lpdwRegister);

    [DllImport("ole32.dll")]
    private static extern int CoRevokeClassObject(uint dwRegister);

    private const uint ClsctxLocalServer = 0x4;
    private const uint RegclsMultipleUse = 0x1;

    private static void Main()
    {
        var result = CoRegisterClassObject(
            WidgetProvider.ClassId,
            new WidgetProviderFactory<WidgetProvider>(),
            ClsctxLocalServer,
            RegclsMultipleUse,
            out var registration);

        Marshal.ThrowExceptionForHR(result);

        try
        {
            // The Widgets Board starts this process on demand. Keep the COM server
            // alive while at least one of this provider's widgets is pinned.
            WidgetProvider.WaitForAllWidgetsToBeRemoved();
        }
        finally
        {
            CoRevokeClassObject(registration);
        }
    }
}
