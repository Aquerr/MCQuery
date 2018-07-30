using System;
using System.Collections.Generic;
using System.Text;

namespace MCQuery
{
	public class Rcon : Connection
	{
        private readonly byte[] _requestId = { 0x01, 0x01, 0x01, 0x01 };

        private readonly byte[] _loginType = { 0x03 };
        private readonly byte[] _commandType = { 0x02 };
        private readonly byte[] _multiType = { 0x00 };
        private readonly byte[] _twoBytePad = { 0x00, 0x00 };

        private string _password;

        public Rcon(string address, int port, string password) : base(address, port)
		{
            _password = password;
		}

        public bool Login()
        {
            //1. Reminder Lenght = int
            //2. Request Id = int
            //3. Type = int
            //4. Payload = byte[]
            //5. 2-byte pad = byte, byte

//            List<byte> message = new List<byte>();
//            message.AddRange(new byte[] { 0x08 });
//            message.AddRange(new byte[] { 0x01 });
//            message.AddRange(_loginType);
//            message.AddRange(Encoding.ASCII.GetBytes(_password));
//            message.AddRange(_twoBytePad);
//            byte[] loginMessage = message.ToArray();

            var command = BitConverter.GetBytes(10 + Encoding.UTF8.GetByteCount(_password));
            List<byte> message = new List<byte>();
            message.AddRange(command);
            message.AddRange(BitConverter.GetBytes(1));
            message.AddRange(BitConverter.GetBytes(3));
            message.AddRange(Encoding.UTF8.GetBytes(_password));
            message.AddRange(_twoBytePad);

            byte[] response = SendByTcp(_address, _port, message.ToArray());

            foreach (byte item in response)
            {
                //if (item == _requestId)
                //{
                //    return true;
                //}
            }

            return false;
        }
	}
}