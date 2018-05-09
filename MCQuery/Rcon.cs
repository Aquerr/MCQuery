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
        private readonly byte[] _twoBytePad = { 0, 0 };

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

            List<byte> message = new List<byte>();
            message.AddRange(new byte[] { 0x06 });
            message.AddRange(_requestId);
            message.AddRange(_loginType);
            message.AddRange(Encoding.ASCII.GetBytes(_password));
            message.AddRange(_twoBytePad);
            byte[] fullStatMessage = message.ToArray();

            byte[] response = SendByUdp(_address, _port, fullStatMessage);

            foreach (byte item in response)
            {
                //if (item == _requestId)
                //{
                //    return true;
                //}
            }

            return false;
        }

        public override void Close()
        {
            base.Close();
        }
    }
}