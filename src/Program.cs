using System.Runtime.InteropServices;

class TapeReader
{

    // Constants for MTIOCTOP
    const int MTIOCTOP = 0x40086d01;  // IOCTL for tape operations
    
    const int MTSTATUS = 3;       // Operation to get status

    // Operation results
    const int MT_EOF = 0x01;      // File mark detected
    const int MT_EOT = 0x02;      // End of tape detected

    [StructLayout(LayoutKind.Sequential)]
    public struct MTCommand
    {
        public int mt_op;         // Operation code
        public int mt_count;      // Count for the operation
    }

    [DllImport("libc.so.6", SetLastError = true)]
    public static extern int ioctl(int fd, int request, ref MTCommand command);



    static bool IsFileMarkReached(FileStream tapeStream)
    {
        try
        {
            // Get the file descriptor
            int fd = tapeStream.SafeFileHandle.DangerousGetHandle().ToInt32();

            MTCommand cmd = new()
            {
                mt_op = MTSTATUS,
                mt_count = 0
            };

            int result = ioctl(fd, MTIOCTOP, ref cmd);

            if (result < 0)
            {
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine($"Error reading tape status: {error}");
                Console.WriteLine($"Error message: {new System.ComponentModel.Win32Exception(error).Message}");
                return false;
            }

            // Analyze the command results (if your driver/device populates cmd.mt_count)
            if (cmd.mt_count == MT_EOF)
            {
                Console.WriteLine("File mark detected.");
                return true;
            }
            else if (cmd.mt_count == MT_EOT)
            {
                Console.WriteLine("End of tape detected.");
                return false;
            }

            Console.WriteLine("No file mark or end of tape detected.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while checking tape status: {ex.Message}");
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
            // Open the tape device for reading
            using FileStream tapeStream = new(tapeDevice, FileMode.Open, FileAccess.Read);


            IsFileMarkReached(tapeStream);

            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            Console.WriteLine($"Reading from tape device: {tapeDevice}");

            while (true)
            {

                // Read data from the tape device
                while ((bytesRead = tapeStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Convert the buffer to a string for demonstration purposes
                    // If the tape data is binary, handle it accordingly
                    string data = BitConverter.ToString(buffer, 0, bytesRead);

                    Console.WriteLine($"Read {bytesRead} bytes...");                 
                }

                Console.WriteLine("File mark detected.");

                // if (!IsFileMarkReached(tapeStream))
                // {
                //     break;
                // }
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
