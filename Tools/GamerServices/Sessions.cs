using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endif

namespace TLSA.Engine.Tools.GamerServices
{
    /// <summary>
    /// Sesiones iniciadas de jugadores.
    /// </summary>
    /// <remarks>Encapsula las llamadas basicas al servicio de jugadores para acceder a informacion de usuarios con sesion iniciada y su informacion de perfil.</remarks>
    public class Sessions
    {
        public Sessions()
        {
            Manager.GameInstance.Components.Add(new GamerServicesComponent(Manager.GameInstance));
        }

        /// <summary>
        /// Devuelve la lista de usuarios que hayan iniciado sesion:
        /// </summary>
        public SignedInGamerCollection SignedInGamers
        {
            get 
            {
#if WINDOWS
                return null;
#else
                return Microsoft.Xna.Framework.GamerServices.Gamer.SignedInGamers;
#endif
            }
        }

        /// <summary>
        /// Obtiene la imagen de jugador de un usuario.
        /// </summary>
        /// <param name="gamer">Usuario conectado.</param>
        /// <returns>Devuelve una textura con la imagen de jugador.</returns>
        public Texture2D GetGamerPicture(SignedInGamer gamer)
        {
            return Texture2D.FromStream(Manager.GraphicsDevice, gamer.GetProfile().GetGamerPicture());
        }
    }
}
