using System;
using System.IO;
using System.Runtime.InteropServices;

class TapeReader
{


    private const uint MTIOCGET = 0x80186d02; // Get tape status
    private const int MTIOCTOP = 0x40086d01; // Perform a tape operation
    private const int MTFSF = 4;            // Forward space file

    [StructLayout(LayoutKind.Sequential)]
    private struct Mtop
    {
        public short mt_op;    // Operation to perform
        public int mt_count;   // Number of operations
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct Mtget
    {
        public long mt_type;       // Type of tape device
        public long mt_resid;      // Residual count
        public long mt_fileno;     // Current file number
        public long mt_blkno;      // Current block number
        public int mt_flags;       // Flags (includes file mark flag)
        public int mt_bf;          // Optimal block size
    }

    private const int MT_EOF = 0x0001; // File mark flag


    static void Main(string[] args)
    {
        string tapeDevice = "/dev/st0";
        int bufferSize = 8192; // Adjust buffer size as needed

        Console.WriteLine($"Reading from tape device: {tapeDevice}");

        try
        {
            // Open the tape device for reading
            using FileStream tapeStream = new(tapeDevice, FileMode.Open, FileAccess.Read);

            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            // Read data from the tape device
            while ((bytesRead = tapeStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Convert the buffer to a string for demonstration purposes
                // If the tape data is binary, handle it accordingly
                string data = BitConverter.ToString(buffer, 0, bytesRead);
                Console.WriteLine($"Read {bytesRead} bytes: {data}");


                  // Check for file mark
                if (IsFileMarkReached(tapeStream, tapeDevice))
                {
                    Console.WriteLine("File mark detected!");
                }

            }

            Console.WriteLine("Finished reading from tape device.");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Permission denied: {ex.Message}");
            Console.WriteLine("Try running the program with elevated permissions.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error occurred: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }



    // /// <summary>
    // /// Retrieves the current status of the tape device using a FileStream.
    // /// </summary>
    // /// <param name="tapeStream">The FileStream object associated with the tape device.</param>
    // /// <returns>An Mtget structure containing the tape status.</returns>
    // public static Mtget GetStatus(FileStream tapeStream)
    // {
    //     if (tapeStream == null)
    //     {
    //         throw new ArgumentNullException(nameof(tapeStream), "The tapeStream parameter cannot be null.");
    //     }

    //     Mtget mtStatus = new Mtget();

    //     int result = ioctl(tapeStream.SafeFileHandle.DangerousGetHandle(), MTIOCGET, ref mtStatus);

    //     if (result != 0)
    //     {
    //         throw new IOException($"Failed to retrieve tape status. Errno: {Marshal.GetLastWin32Error()}");
    //     }

    //     return mtStatus;
    // }



    private static bool IsFileMarkReached(FileStream fileStream, string tapeDevice)
    {
        Mtop mtop = new Mtop
        {
            mt_op = MTFSF,   // Check for forward space file operation
            mt_count = 1     // Move forward by 1 file mark
        };

        int result = ioctl(fileStream.SafeFileHandle.DangerousGetHandle(), MTIOCTOP, ref mtop);

        if (result == 0)
        {
            Console.WriteLine("Tape position advanced to next file mark.");
            return true;
        }

        return false;
    }


    [DllImport("libc.so.6", SetLastError = true)]
    private static extern int ioctl(IntPtr fd, int request, ref Mtop mtop);
}
