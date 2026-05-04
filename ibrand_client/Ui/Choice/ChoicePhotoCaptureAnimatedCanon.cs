namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class ChoicePhotoCaptureAnimatedCanon : ChoicePhotoCaptureCanon
    {


        protected override void SetCaptureTextBox()
        {
            _photo_field = new CaptureAnimatedTextBoxCanon();
            _photo_field.Initialize();
        }

        public override bool IsAnswered()
        {
            return ((CaptureAnimatedTextBoxCanon)_photo_field).IsAnswered;
        }
    }
}
