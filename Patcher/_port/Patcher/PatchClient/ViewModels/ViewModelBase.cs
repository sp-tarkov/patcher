using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PatchClient.Models;

namespace PatchClient.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        /// <summary>
        /// Delay the return of the viewmodel
        /// </summary>
        /// <param name="Milliseconds">The amount of time in milliseconds to delay</param>
        /// <returns>The viewmodel after the delay time</returns>
        /// <remarks>Useful to delay the navigation to another view via the <see cref="ViewNavigator"/>. For instance, to allow an animation to complete.</remarks>
        public ViewModelBase WithDelay(int Milliseconds)
        {
            System.Threading.Thread.Sleep(Milliseconds);

            return this;
        }
    }

}
