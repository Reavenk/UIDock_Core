using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre.UIDock
{
    /// <summary>
    /// A message receiver to listen in on events happening in dock system, as well
    /// as giving outside code the ability to veto or react to certain events.
    /// </summary>
    public interface IDockListener
    {
        /// <summary>
        /// Called when a window has been requested for docking.
        /// </summary>
        /// <param name="r">The docking system.</param>
        /// <param name="win">The window attemping to be undocked.</param>
        /// <returns></returns>
        bool RequestUndock(Root r, Window win);

        /// <summary>
        /// Called when a window has been requested to be closed.
        /// </summary>
        /// <param name="r">The docking system.</param>
        /// <param name="win">The window attempting to be closed.</param>
        /// <returns></returns>
        bool RequestClose(Root r, Window win);

        /// <summary>
        /// Called when a window is about to be destroyed.
        /// </summary>
        /// <param name="r">The docking system.</param>
        /// <param name="win">The window about to be destroyed.</param>
        void OnClosing(Root r, Window win);
    }
}
