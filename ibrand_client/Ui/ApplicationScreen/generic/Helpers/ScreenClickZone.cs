using System;
using System.Drawing;
using Newtonsoft.Json;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Instance visuelle qui génère une action spécifique quand on clique dessus.
    /// On peut personaliser ce qui arrive (avec des validation venant du père) en pasant une fonction plus mature à l'évaluation de ces zones.
    /// </summary>
    public abstract class ScreenClickZone : IOnLoad, IPaint, IChangeLanguage, IEvaluateScreenZones, IDisposable
    {
        [JsonIgnore]
        public Action OnClick;

        public Rectangle Zone { get; set; }

        public virtual bool IsVisible { get; set; }
        
        public abstract bool OnLoad(AbstractScreen screen);
        
        public void Paint(Graphics g, bool debug = false)
        {
            DoPaint(g);
            if (debug) DoDebugPaint(g);
        }

        protected virtual void DoDebugPaint(Graphics g)
        {
            if (IsVisible) ScreenUtil.DrawDebugRectangle(g, ScreenUtil.DebugBluePen, Zone);
            else ScreenUtil.DrawDebugInactiveRectangle(g, ScreenUtil.DebugBlackPen, Zone);
        }

        protected abstract void DoPaint(Graphics g);

        public virtual bool SkipValidation { get; set; }

        [JsonIgnore]
        public string CurrentLanguage { get; protected set; }

        public ScreenEvaluationResult EvaluateZones(AbstractScreen screen, int x, int y, Func<ScreenEvaluationResult> validationMethod)
        {
            if (!IsVisible) return new ScreenEvaluationResult();
            if (!MathUtil.InArea(x, y, Zone)) return new ScreenEvaluationResult();
            if (validationMethod != null && !SkipValidation)
            {
                return validationMethod.Invoke();
            }
            return new ScreenEvaluationResult(OnClick);
        }

        public virtual void Dispose()
        {
            // Rien par défaut.
        }

        public virtual void ChangeLanguage(AbstractScreen screen, string languageCode)
        {
            CurrentLanguage = languageCode;
        }

        public override string ToString()
        {
            return this.ToStringPublicProperties(1);
        }
    }
}