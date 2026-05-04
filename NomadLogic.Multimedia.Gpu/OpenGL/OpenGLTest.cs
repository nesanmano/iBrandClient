using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Services.LocalCapabilities;
using SharpGL;

namespace NomadLogic.Multimedia.Gpu.OpenGL
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Fenêtre de test pour saisir les extensions et la version OpenGL de la carte graphique.
    /// Si créé avec des arguments nulls, l'affichage est continu comme une fenêtre normale.
    /// Si on lui donne un rapport à remplir, cette fenêtre se ferme automatiquement dès que 
    /// l'information est acquise.
    /// </summary>
    [PublicAPI]
    public partial class OpenGLCapabilitiesTestForm : Form
    {
        private float _rtri;
        private float _rquad;
        private readonly OpenGLSubCapabilities _caps;
        private readonly CancellationTokenSource _cts;
        private readonly Action _followAction;

        public OpenGLCapabilitiesTestForm(OpenGLSubCapabilities reportOpenGlSubCapabilities, CancellationTokenSource cts, Action nextStepOfTesting)
        {
            InitializeComponent();

            _cts = cts;
            _caps = reportOpenGlSubCapabilities;
            _followAction = nextStepOfTesting;
        }

        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs e)
        {
            //  Get the OpenGL object, just to clean up the code.
            var gl = openGLControl1.OpenGL;

            #region Show beautiful 3D shapes!
            gl.Clear(SharpGL.OpenGL.GL_COLOR_BUFFER_BIT | SharpGL.OpenGL.GL_DEPTH_BUFFER_BIT);	// Clear The Screen And The Depth Buffer
            gl.LoadIdentity();                  // Reset The View

            // gl.Color(1.0f, 1.0f, 1.0f);
            // gl.FontBitmaps.DrawText(gl, 0, 0, "Arial", "Argh");
            
            gl.Translate(-1.5f, 0.0f, -6.0f);				// Move Left And Into The Screen

            gl.Rotate(_rtri, 0.0f, 1.0f, 0.0f);				// Rotate The Pyramid On It's Y Axis

            gl.Begin(SharpGL.OpenGL.GL_TRIANGLES);					// Start Drawing The Pyramid

            gl.Color(1.0f, 0.0f, 0.0f);			// Red
            gl.Vertex(0.0f, 1.0f, 0.0f);			// Top Of Triangle (Front)
            gl.Color(0.0f, 1.0f, 0.0f);			// Green
            gl.Vertex(-1.0f, -1.0f, 1.0f);			// Left Of Triangle (Front)
            gl.Color(0.0f, 0.0f, 1.0f);			// Blue
            gl.Vertex(1.0f, -1.0f, 1.0f);			// Right Of Triangle (Front)

            gl.Color(1.0f, 0.0f, 0.0f);			// Red
            gl.Vertex(0.0f, 1.0f, 0.0f);			// Top Of Triangle (Right)
            gl.Color(0.0f, 0.0f, 1.0f);			// Blue
            gl.Vertex(1.0f, -1.0f, 1.0f);			// Left Of Triangle (Right)
            gl.Color(0.0f, 1.0f, 0.0f);			// Green
            gl.Vertex(1.0f, -1.0f, -1.0f);			// Right Of Triangle (Right)

            gl.Color(1.0f, 0.0f, 0.0f);			// Red
            gl.Vertex(0.0f, 1.0f, 0.0f);			// Top Of Triangle (Back)
            gl.Color(0.0f, 1.0f, 0.0f);			// Green
            gl.Vertex(1.0f, -1.0f, -1.0f);			// Left Of Triangle (Back)
            gl.Color(0.0f, 0.0f, 1.0f);			// Blue
            gl.Vertex(-1.0f, -1.0f, -1.0f);			// Right Of Triangle (Back)

            gl.Color(1.0f, 0.0f, 0.0f);			// Red
            gl.Vertex(0.0f, 1.0f, 0.0f);			// Top Of Triangle (Left)
            gl.Color(0.0f, 0.0f, 1.0f);			// Blue
            gl.Vertex(-1.0f, -1.0f, -1.0f);			// Left Of Triangle (Left)
            gl.Color(0.0f, 1.0f, 0.0f);			// Green
            gl.Vertex(-1.0f, -1.0f, 1.0f);			// Right Of Triangle (Left)
            gl.End();						// Done Drawing The Pyramid

            gl.LoadIdentity();
            gl.Translate(1.5f, 0.0f, -7.0f);				// Move Right And Into The Screen

            gl.Rotate(_rquad, 1.0f, 1.0f, 1.0f);			// Rotate The Cube On X, Y & Z

            gl.Begin(SharpGL.OpenGL.GL_QUADS);					// Start Drawing The Cube

            gl.Color(0.0f, 1.0f, 0.0f);			// Set The Color To Green
            gl.Vertex(1.0f, 1.0f, -1.0f);			// Top Right Of The Quad (Top)
            gl.Vertex(-1.0f, 1.0f, -1.0f);			// Top Left Of The Quad (Top)
            gl.Vertex(-1.0f, 1.0f, 1.0f);			// Bottom Left Of The Quad (Top)
            gl.Vertex(1.0f, 1.0f, 1.0f);			// Bottom Right Of The Quad (Top)


            gl.Color(1.0f, 0.5f, 0.0f);			// Set The Color To Orange
            gl.Vertex(1.0f, -1.0f, 1.0f);			// Top Right Of The Quad (Bottom)
            gl.Vertex(-1.0f, -1.0f, 1.0f);			// Top Left Of The Quad (Bottom)
            gl.Vertex(-1.0f, -1.0f, -1.0f);			// Bottom Left Of The Quad (Bottom)
            gl.Vertex(1.0f, -1.0f, -1.0f);			// Bottom Right Of The Quad (Bottom)

            gl.Color(1.0f, 0.0f, 0.0f);			// Set The Color To Red
            gl.Vertex(1.0f, 1.0f, 1.0f);			// Top Right Of The Quad (Front)
            gl.Vertex(-1.0f, 1.0f, 1.0f);			// Top Left Of The Quad (Front)
            gl.Vertex(-1.0f, -1.0f, 1.0f);			// Bottom Left Of The Quad (Front)
            gl.Vertex(1.0f, -1.0f, 1.0f);			// Bottom Right Of The Quad (Front)

            gl.Color(1.0f, 1.0f, 0.0f);			// Set The Color To Yellow
            gl.Vertex(1.0f, -1.0f, -1.0f);			// Bottom Left Of The Quad (Back)
            gl.Vertex(-1.0f, -1.0f, -1.0f);			// Bottom Right Of The Quad (Back)
            gl.Vertex(-1.0f, 1.0f, -1.0f);			// Top Right Of The Quad (Back)
            gl.Vertex(1.0f, 1.0f, -1.0f);			// Top Left Of The Quad (Back)

            gl.Color(0.0f, 0.0f, 1.0f);			// Set The Color To Blue
            gl.Vertex(-1.0f, 1.0f, 1.0f);			// Top Right Of The Quad (Left)
            gl.Vertex(-1.0f, 1.0f, -1.0f);			// Top Left Of The Quad (Left)
            gl.Vertex(-1.0f, -1.0f, -1.0f);			// Bottom Left Of The Quad (Left)
            gl.Vertex(-1.0f, -1.0f, 1.0f);			// Bottom Right Of The Quad (Left)

            gl.Color(1.0f, 0.0f, 1.0f);			// Set The Color To Violet
            gl.Vertex(1.0f, 1.0f, -1.0f);			// Top Right Of The Quad (Right)
            gl.Vertex(1.0f, 1.0f, 1.0f);			// Top Left Of The Quad (Right)
            gl.Vertex(1.0f, -1.0f, 1.0f);			// Bottom Left Of The Quad (Right)
            gl.Vertex(1.0f, -1.0f, -1.0f);			// Bottom Right Of The Quad (Right)
            gl.End();                       // Done Drawing The Q

            gl.Flush();

            _rtri += 3.0f;// 0.2f;						// Increase The Rotation Variable For The Triangle 
            _rquad -= 3.0f;// 0.15f;	
            #endregion

            if (_caps == null) return;

            _caps.Vendor = gl.Vendor;
            _caps.Version = gl.Version;
            _caps.Extensions.AddRange(gl.Extensions.Split(' '));
            _caps.Status = StatusEnum.Valid;

            if (!string.IsNullOrEmpty(_caps.Vendor)) Invoke(new Action(Close));
        }

        /// <inheritdoc />
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _cts?.Cancel();
            _followAction?.Invoke();
        }

        
    }
}
