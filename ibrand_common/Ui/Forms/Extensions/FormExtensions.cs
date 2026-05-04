using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui.Forms.Extensions
{
    /// <summary>
    /// Extensions génériques sur les formes Windows.
    /// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/how-to-implement-and-call-a-custom-extension-method
    /// </summary>
    public static class FormExtensions
    {
        /// <summary>
        /// Retourne les contrôles enfants d'un ou plusieurs types spécifiques d'une instance de contrôle.
        /// </summary>
        /// <param name="control">Le contrôle sur lequel exécuter la méthode.</param>
        /// <param name="type">Les types des enfants à rechercher.</param>
        /// <returns></returns>
        public static IEnumerable<Control> GetAll(this Control control, params Type[] type)
        {
            var controls = control.Controls.Cast<Control>();

            var enumerable = controls as Control[] ?? controls.ToArray();
            return enumerable.SelectMany(ctrl => GetAll(ctrl, type))
                .Concat(enumerable)
                .Where(c => type?.Contains(c.GetType()) ?? false);
        }
    }
}
