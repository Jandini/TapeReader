using System.Runtime.InteropServices;

class Program
{
    // Constants for IOCTL commands
    const int MTIOCTOP = 0x6d01;  // IOCTL for tape operations
    const int MTFSF = 10;         // Forward space file operation
    const int MTSTATUS = 3;       // Status operation (alternative)

    // Tape operation structure
    [StructLayout(LayoutKind.Sequential)]
    public struct MTCommand
    {
        public int mt_op;         // Operation code
        public int mt_count;      // Count for the operation
    }

    [DllImport("libc.so.6", SetLastError = true)]
    public static extern int ioctl(int fd, int request, ref MTCommand command);

    static void ForwardSpaceFile(FileStream tapeStream, int count)
    {
        int fd = tapeStream.SafeFileHandle.DangerousGetHandle().ToInt32();
        MTCommand cmd = new()
        {
            mt_op = MTFSF, // Forward space file
            mt_count = count
        };

        int result = ioctl(fd, MTIOCTOP, ref cmd);

        if (result < 0)
        {
            int error = Marshal.GetLastWin32Error();
            Console.WriteLine($"Error skipping file mark: {error}");
            Console.WriteLine($"Error message: {new System.ComponentModel.Win32Exception(error).Message}");
        }
        else
        {
            Console.WriteLine($"Skipped {count} file mark(s).");
        }
    }

    static bool ReadFromTape(FileStream tapeStream, byte[] buffer, out int bytesRead)
    {
        try
        {
            bytesRead = tapeStream.Read(buffer, 0, buffer.Length);

            if (bytesRead == 0)
            {
                Console.WriteLine("File mark detected.");
                return false; // Indicates a file mark was reached
            }

            Console.WriteLine($"Read {bytesRead} bytes...");
            return true;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error occurred: {ex.Message}");
            bytesRead = 0;
            return false;
        }
    }

    static void Main(string[] args)
    {
        string tapeDevice = "/dev/st1";
        int bufferSize = 65536; // Adjust buffer size as needed

        Console.WriteLine($"Accessing tape device: {tapeDevice}");

        try
        {
            using FileStream tapeStream = new(tapeDevice, FileMode.Open, FileAccess.Read);

            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            Console.WriteLine($"Reading from tape device: {tapeDevice}");

            while (true)
            {
                while (ReadFromTape(tapeStream, buffer, out bytesRead))
                {
                    // Process data if needed
                }

                // If we reach here, it's because of a file mark or an error
                if (bytesRead == 0) 
                {
                    // Handle the file mark and skip to the next file
                    ForwardSpaceFile(tapeStream, 1);
                }
                else
                {
                    // End of tape or an error
                    break;
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
}