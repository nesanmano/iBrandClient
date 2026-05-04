namespace NomadLogic.Common
{
    public delegate void OnUploadPictureFinishedDelegate(bool success);
    public delegate void ShowMessageBoxDelegate(string message, bool error);

    public delegate void AddProgressStepDelegate(int value);
    public delegate void SetProgressMinMaxDelegate(int min, int max);

    public delegate void ServerFound(string ipAddress);
    public delegate void SearchFinished();
}