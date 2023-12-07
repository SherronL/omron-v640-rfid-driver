using System.Text;
using System;
using System.IO.Ports;

class RFIDReader
{
    static Dictionary<string, string> CmdList = new Dictionary<string, string>
    {
        {"getVersion", "1402\r" },
        {"readRFID", "01000000000C\r"},
    };
    public static void IncludeCheckSum(ref string message)
    {
        char checksum;
        int addition_checksum;
        int i;
        if (!message.EndsWith("\r"))
        {
            return;
        }

        int endStringPos = message.IndexOf("\r");
        checksum = Convert.ToChar(message[0].ToString());
        addition_checksum = Convert.ToInt32(checksum);
        int strpos = 1;
        while (strpos <= endStringPos)
        {
            char currentChar = Convert.ToChar(message[strpos].ToString());
            checksum ^= currentChar;
            addition_checksum += Convert.ToInt32(currentChar);
            strpos++;
        }

        ////highbyte checksum XOR
        i = (checksum & 0xf0) >> 4;
        string lowString = i.ToString("X");
        message += lowString;
        ////lowbyte checksum XOR
        i = checksum & 0x0f;
        string highString = i.ToString("X");
        message += highString;
        ////highbyte checksum addition
        i = (addition_checksum & 0x00f0) >> 4;
        string highbyteString = i.ToString("X");
        message += highbyteString;
        ////highbyte checksum addition
        i = addition_checksum & 0x000f;
        string highbyteStringhigh = i.ToString("X");
        message += highbyteStringhigh;
    }
    public static void PrintCountsAndBytes(char[] chars, Encoding enc)
    {
        // Display the name of the encoding used.
        Console.Write("{0,-30} :", enc.ToString());

        // Display the exact byte count.
        int iBC = enc.GetByteCount(chars);
        Console.Write(" {0,-3}", iBC);

        // Display the maximum byte count.
        int iMBC = enc.GetMaxByteCount(chars.Length);
        Console.Write(" {0,-3} :", iMBC);

        // Encode the array of chars.
        byte[] bytes = enc.GetBytes(chars);

        // Display all the encoded bytes.
        PrintHexBytes(ref bytes);
    }
    public static void PrintHexBytes(ref byte[] bytes)
    {
        if ((bytes == null) || (bytes.Length == 0))
        {
            Console.WriteLine("<none>");
        }
        else
        {
            for (int i = 0; i < bytes.Length; i++)
                Console.Write("{0:X2} ", bytes[i]);
            Console.WriteLine();
        }
    }
    public static void SendCommand(SerialPort serialPort, ref byte[] packets)
    {
        try
        {
            Thread.Sleep(1000);
            if (serialPort.IsOpen)
            {
                serialPort.Write(packets, 0, packets.Length);
                Console.WriteLine("Sent: ");
                PrintHexBytes(ref packets);
                Thread.Sleep(300);
                string response = serialPort.ReadExisting();
                byte[] data = Encoding.ASCII.GetBytes(response);
                Console.WriteLine("Received: ");
                PrintHexBytes(ref data);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            serialPort.Close();
        }
    }
    static void Main(string[] args)
    {
        Console.WriteLine("Omron V640 RFID reader driver\nUsage: enter 1 to get device verison; enter 2 to read RFID");

        // port settings
        SerialPort serialPort = new SerialPort("COM3");
        serialPort.BaudRate = 19200;   //BaudRate
        serialPort.DataBits = 8;   //DataBits
        serialPort.StopBits = StopBits.One;  //StopBits
        serialPort.Parity = Parity.Even; //Parity
        //serialPort.Open();

        while (true)
        {
            string userInput = Console.ReadLine();

            string currCmd = "";

            switch (userInput)
            {
                case "1":
                    Console.WriteLine("Getting version number\n");
                    currCmd = CmdList["getVersion"];
                    break;
                case "2":
                    Console.WriteLine("Reading RFID info\n");
                    currCmd = CmdList["readRFID"];
                    break;
                case "exit":
                    serialPort.Close();
                    System.Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("unrecognized command\n");
                    break;
            }
            // add checksum to end of message
            IncludeCheckSum(ref currCmd);

            // encode message 
            byte[] packets = Encoding.ASCII.GetBytes(currCmd);

            SendCommand(serialPort, ref packets);
        }
    }
}
