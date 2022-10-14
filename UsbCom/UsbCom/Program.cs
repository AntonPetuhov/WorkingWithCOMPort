using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Ports;

namespace UsbCom
{
    class Program
    {
        public static string COMPortName = "COM4"; // 
        public static bool ServiceIsActive;        // флаг для запуска и остановки потока

        public static string MessageForSend = @"H|\^&||||||||||||19990208000001" + '\r' +
                                     "Q|1|^ALL^ALL||ALL||19980502123000|19980613123000" + '\r' +
                                     "L|1|N";

        static bool _continue;
        static SerialPort _serialPort;

        #region Управляющие биты
        static byte[] STX = { 0x02 }; // начало текста
        static byte[] ETX = { 0x03 }; // конец текста
        static byte[] EOT = { 0x04 }; // конец передачи данных
        static byte[] ENQ = { 0x05 }; // запрос 
        static byte[] ACK = { 0x06 }; // подтверждение
        static byte[] NAK = { 0x15 };
        static byte[] SYN = { 0x16 };
        static byte[] ETB = { 0x17 };
        static byte[] LF = { 0x0A };
        static byte[] CR = { 0x0D };
        #endregion

        static void Exchange()
        {
            int i = 0;
            while (i < 300)
            //while (_continue)
            {
                Thread.Sleep(1000);
                _serialPort.Write(ENQ, 0, ENQ.Length);
                Console.WriteLine("C: <ENQ>;");

                string messageFrom = _serialPort.ReadExisting();

                if (messageFrom.Length == 0)
                {
                    //Thread.Sleep(5000);
                    //Console.WriteLine("Empty message");
                    // ExchangeLog("Empty message");
                }
                else
                {
                    Console.WriteLine($"{messageFrom}");

                    Encoding utf8 = Encoding.UTF8;
                    // Convert the string into a byte array.
                    byte[] encodedMessage = utf8.GetBytes(messageFrom);

                    bool EndOfMessage = false;

                    if (encodedMessage[0] == ACK[0])
                    {
                 
                        _serialPort.WriteLine(MessageForSend);
                        //_serialPort.Write(ACK, 0, ACK.Length);
                        Console.WriteLine($"S:{MessageForSend};");
                      

                    }
                    i = i + 1;
                    Console.WriteLine($"C: i = {i} !!!");
                    //i = i + 1;
                }

             }
                //_serialPort.Write(ENQ, 0, ENQ.Length);
        }
        static void COMPortSettings()
        {
            //Thread readThread = new Thread(ReadFromCOM);
            Thread ExchangeThread = new Thread(Exchange);
            try
            {
                // Create a new SerialPort object
                _serialPort = new SerialPort();

                // настройки СОМ порта
                _serialPort.PortName = COMPortName;
                _serialPort.BaudRate = 9600;
                _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), "None", true);
                _serialPort.DataBits = 8;
                _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One", true);
                _serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None", true);

                // Set the read/write timeouts
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;

                _serialPort.Open();
                _continue = true;

                // Запуск потока чтения порта
                //readThread.Start();
               ExchangeThread.Start();

                Console.WriteLine("Reading thread is started");
                //Exchange();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR. Невозможно открыть порт:" + ex.ToString());
                return;
            }

        }
        static void Main(string[] args)
        {
            COMPortSettings();
            Console.WriteLine("Service is working");

            Console.ReadLine();
        }
    }
}
