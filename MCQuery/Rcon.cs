namespace MCQuery
{
	public class Rcon : Connection
	{
		public Rcon(string address, int port, string password) : base(address, port)
		{

		}

        public override void Close()
        {
            base.Close();
        }
    }
}