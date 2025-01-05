using System;
using System.IO;

class TapeReader
{
    static void Main(string[] args)
    {
        string tapeDevice = "/dev/st0";
        int bufferSize = 4096; // Adjust buffer size as needed

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
