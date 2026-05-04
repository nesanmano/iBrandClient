namespace NomadLogic.Common.Serveur
{
    /// <summary>
    /// Cette classe est un "wrapper" autour de la classe "TerminalInfo" pour fin de présentation dans des contrôles Windows.
    /// </summary>
    public class TerminalControlItem
    {
        public TerminalControlItem(TerminalInfo term)
        {
            Terminal = term;
        }

        public TerminalInfo Terminal { get; }

        public override string ToString()
        {
            return $"({Terminal.TerminalId}) - {Terminal.TerminalName}";
        }
    }
}