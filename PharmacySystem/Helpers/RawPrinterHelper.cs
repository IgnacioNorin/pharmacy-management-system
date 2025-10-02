using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Helpers
{
    public class RawPrinterHelper
    {
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true)]
        static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] ref DOCINFOA di);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        [StructLayout(LayoutKind.Sequential)]
        public struct DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        public static bool SendStringToPrinter(string printerName, string document)
        {
            IntPtr pPrinter;
            DOCINFOA docInfo = new DOCINFOA
            {
                pDocName = "Ticket",
                pDataType = "RAW"
            };

            if (!OpenPrinter(printerName.Normalize(), out pPrinter, IntPtr.Zero))
                return false;

            if (!StartDocPrinter(pPrinter, 1, ref docInfo))
            {
                ClosePrinter(pPrinter);
                return false;
            }

            StartPagePrinter(pPrinter);
            IntPtr pBytes = Marshal.StringToCoTaskMemAnsi(document);
            WritePrinter(pPrinter, pBytes, document.Length, out int bytesWritten);
            EndPagePrinter(pPrinter);
            EndDocPrinter(pPrinter);
            ClosePrinter(pPrinter);
            Marshal.FreeCoTaskMem(pBytes);

            return true;
        }
    }
}
