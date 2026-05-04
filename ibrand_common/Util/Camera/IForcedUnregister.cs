namespace NomadLogic.Common.Util.Camera
{
    /// <summary>
    /// Interface pour écoutant qui peut se faire dé-enregistrer de force.
    /// </summary>
    public interface IForcedUnregister
    {
        /// <summary>
        /// Indique que l'on a été dé-enregistré de force, probablement parce que l'objet à écouter à
        /// été détruit ou n'est plus valide.
        /// </summary>
        void OnForcedUnregister();
    }
}