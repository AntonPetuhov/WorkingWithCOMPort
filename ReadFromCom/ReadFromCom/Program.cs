using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace ReadFromCom
{
    class Program
    {
        static SerialPort _serialPort;
        public static string COMPortName = "COM4"; // 
        static bool _continue;

        static byte[] ACK = { 0x06 };
        static byte[] ENQ = { 0x05 }; // запрос 

        static void ReadFromCOM()
        {
            //Thread.Sleep(10000);
            while (_continue)
            {
                // считываем все доступные байты в строку
                string messageFrom = _serialPort.ReadExisting();

                if (messageFrom.Length == 0)
                {
                    Thread.Sleep(5000);
                    Console.WriteLine("Empty message");
                    // ExchangeLog("Empty message");
                }
                else
                {
                    int i = 0;
                    Console.WriteLine($"{messageFrom}");

                    _serialPort.Write(ACK, 0, ACK.Length);
                    Console.WriteLine($"<ACK>;");

                    // UTF8 encoder
                    // UTF8Encoding utf8 = new UTF8Encoding();
                    Encoding utf8 = Encoding.UTF8;
                    // Convert the string into a byte array.
                    byte[] encodedMessage = utf8.GetBytes(messageFrom);

                    bool EndOfMessage = false;

                    if (encodedMessage[0] == ENQ[0])
                    {
                        // На запрос ENQ, драйвер должен ответить ACK
                        _serialPort.Write(ACK, 0, ACK.Length);
                        Console.WriteLine($"S:<ACK>;");

                        while (!EndOfMessage)
                        {
                            string TMPString = "";
                            try
                            {
                                string TMPMessage = _serialPort.ReadLine();

                                // ExchangeLog($"Client:{TMPMessage}"); // в логе будут кривые символы, если открывать через блокнот

                                // Преобразовываем строку в массив байтов
                                byte[] TMPencodedMessage = utf8.GetBytes(TMPMessage);

                                // Для удобства дальнейшего чтения логов, формируем строку из считанного массива байт, заменяя управляющие байты, на символы UTF8
                                // иначе будут нечитаемые символы

                                //TMPString = GetStringFromBytes(TMPencodedMessage);

                                // пишем сообщение от прибора в лог обмена
                                Console.WriteLine($"C:{TMPMessage};");
                                i = i + 1;
                                Console.WriteLine($"C:{i};");

                                _serialPort.Write(ACK, 0, ACK.Length);
                                Console.WriteLine("S:<ACK>;");
                            }
                            catch (Exception ex)
                            {
                               
                                //Console.WriteLine($"{ex}");
                                //_serialPort.Write(ACK, 0, ACK.Length);
                                //Console.WriteLine("S:<ACK>;");
                            }
                        }
                    }
                }
            }
        }

        static void COMPortSettings()
        {
            Thread readThread = new Thread(ReadFromCOM);
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
                readThread.Start();
                Console.WriteLine("Reading thread is started");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR. Невозможно открыть порт:" + ex.ToString());
                return;
            }

        }
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            COMPortSettings();

            Console.ReadLine();
        }
    }
}
