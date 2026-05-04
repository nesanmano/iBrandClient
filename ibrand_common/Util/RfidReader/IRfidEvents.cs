using System;

namespace NomadLogic.Common.Util.RfidReader
{
    /// <summary>
    /// Ajout à l'interface de lecture RFID pour se connecter à des événements précis de lecture.
    /// </summary>
    public interface IRfidEvents : IRfidReader
    {
        event EventHandler<RfidReadEventArgs> CardRead;
        event EventHandler<EventArgs> HardWareDisconnected;
        event EventHandler<RfidReadEventArgs> InvalidCardRead;
    }
}