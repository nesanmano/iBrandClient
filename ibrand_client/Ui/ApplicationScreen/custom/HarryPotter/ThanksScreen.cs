namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HarryPotter
{
    public class ThanksScreen : AbstractScreen
    {
        public ThanksScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            IsSaveOnThisPage = true;
        }

        public new void SetActivated()
        {
            _activated = true;
        }
    }
}
