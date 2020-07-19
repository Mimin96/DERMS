namespace DERMSCommon.SCADACommon
{
	public interface IStateUpdater
	{
		void UpdateConnectionState(ConnectionState currentConnectionState);

		void LogMessage(string message);
	}
}