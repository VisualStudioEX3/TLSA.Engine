using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TLSA.Engine.Tools;
using TLSA.Engine.Tools.XML;

namespace TLSA.Engine.Graphics
{
    /// <summary>
    /// Define una secuencia de animacion en una imagen que tenga varios fotogramas en serie pero defina distintas secuencias de animacion.
    /// </summary>
    public class AnimationSecuence 
    {
        public string Name;             // Nombre de la animacion.
        public Rectangle FirstFrame;    // Primer fotograma de la animacion. Contiene la informacion de la ubicacion del primer fotograma y el tamaño de todos los fotogramas de la secuencia.
        public int Frames;              // Numero de fotogramas de la animacion.
        public int Delay;               // Tiempo de espera entre fotogramas.
        public bool Loop;               // Indica si la animacion se ejecuta indefinidamente.

        #region Constructores
        public AnimationSecuence() 
        {
            this.Name = ""; this.FirstFrame = new Rectangle(); this.Frames = 0; this.Delay = 0; this.Loop = false;
        }
        public AnimationSecuence(string Name, Rectangle FirstFrame, int Frames, int Delay, bool Loop)
        {
            this.Name = Name; this.FirstFrame = FirstFrame; this.Frames = Frames; this.Delay = Delay; this.Loop = Loop;
        }
        #endregion
    }

    /// <summary>
    /// Define un objeto para gestionar los parametros de las animaciones de una imagen.
    /// </summary>
    public class AnimationManager : TLSA.Engine.IUpdateable
    {
        /// <summary>
        /// Informacion que se exportara e importara desde XML en el content pipeline:
        /// </summary>
        /// <example>// Ejemplo de importacion. El recurso data hace referencia a un archivo XML del Content:
        /// AnimationManager varAnimation = Content.Load<AnimationManager>("data");
        /// </example>
        public SerializableDictionary<string, AnimationSecuence> Secuences { get; set; } // Usamos el tipo SerializableDictionary para poder exportarlo e importarlo a XML.
        /// <summary>
        /// Devuelve el estado de la propiedad IsPaused.
        /// </summary>
        /// <remarks>Se implementa para respetar la interfaz IUpdateable.</remarks>
        public bool Enabled { get { return this.isPaused; } set { } }
        /// <summary>
        /// Devuelve el area del fotograma a dibujar.
        /// </summary>
        public Rectangle Frame { get { return drawFrame; } }

        [System.Xml.Serialization.XmlIgnore]
        public string CurrentSecuence { get; internal set; }


        /// <summary>
        /// Indica o devuelve el indice del frame de la animacion actual.
        /// </summary>
        public int CurrentFrameIndex
        {
            get { return currentFrame; }
            set
            {
                if (value < 0) currentFrame = 0; else if (value >= Current.Frames) currentFrame = Current.Frames - 1; else currentFrame = value;
            }
        }

        /// <summary>
        /// Devuelve si la animacion esta en pausa o no.
        /// </summary>
        public bool IsPaused { get { return isPaused; } }

        /// <summary>
        /// Indica si la animacion ha llegado a su final (solo si no es ciclica).
        /// </summary>
        public bool IsEnded { get { return isEnded; } }

        private AnimationSecuence Current;
        private int currentFrame; bool isPaused; bool isEnded;
        private Rectangle drawFrame;
        private double deltaTime;

        #region Constructores
        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        public AnimationManager()
        {
            Secuences = new SerializableDictionary<string, AnimationSecuence>();
            Current = new AnimationSecuence();
            currentFrame = 0;
            isPaused = false;
            isEnded = true;
            drawFrame = new Rectangle();
            this.Enabled = true;
        }
        #endregion
        #region Funciones de reproduccion
        /// <summary>
        /// Selecciona una secuencia de animacion y la activa.
        /// </summary>
        /// <param name="SecuenceName">Nombre o clave de la secuencia en la lista.</param>
        public void Play(string SecuenceName)
        {
            // Si es la misma secuencia + animacion finalizada = si
            // Si 
            if ((this.Secuences[SecuenceName].Loop && this.CurrentSecuence != SecuenceName) ||
                (!this.Secuences[SecuenceName].Loop))
            {
                this.CurrentSecuence = SecuenceName;
                Current = Secuences[SecuenceName];
                drawFrame = Current.FirstFrame;
                currentFrame = 0;
                isPaused = false;
                isEnded = false;
            }
        }

        // Pausa o continua la secuencia de animacion actual.
        public void Pause()
        {
            isPaused = !isPaused;
        }

        /// <summary>
        ///  Detiene la secuencia de animacion actual.
        /// </summary>
        public void Stop()
        {
            drawFrame = Current.FirstFrame;
            currentFrame = 0;
            isPaused = true;
            isEnded = true;
        }
        #endregion

        /// <summary>
        /// Permite definir una secuencia de animacion.
        /// </summary>
        /// <param name="Key">Nombre de la secuencia.</param>
        /// <param name="Frame">Dimensiones que definen el fotograma de la secuencia de animacion.</param>
        /// <param name="Frames">Numero de fotogramas que tiene la secuencia.</param>
        /// <param name="Delay">Tiempo de actualizacion entre fotogramas.</param>
        /// <param name="Loop">Indica si la secuencia se ejecuta en bucle.</param>
        /// <remarks>La secuencia debe estar compuesta de fotogramas todos del mismo tamaño y en fila, no en columna.</remarks>
        public void AddSecuence(string Key, Rectangle Frame, int Frames, int Delay, bool Loop)
        {
            this.Secuences.Add(Key, new AnimationSecuence(Key, Frame, Frames, Delay, Loop));
        }

        /// <summary>
        /// Actualiza la animacion.
        /// </summary>
        /// <param name="gameTime">Tiempo calculado por el bucle del juego utilizado para calcular la actualizacion entre fotogramas de la animacion.</param>
        public void Update(GameTime gameTime)
        {
            if (!isEnded && !isPaused && gameTime.TotalGameTime.TotalMilliseconds - deltaTime >= Current.Delay)
            {                
                deltaTime = gameTime.TotalGameTime.TotalMilliseconds;
                currentFrame++;                         // Pasamos al siguiente fotograma.
                if (currentFrame == Current.Frames)     // Si llegamos al ultimo fotograma:
                {
                    if (Current.Loop)                   // Si la animacion es ciclica:
                    {
                        currentFrame = 0;               // Pasamos al primer fotograma.
                    }
                    else if (!Current.Loop)             // Si la animacion no es ciclica:
                    {
                        currentFrame--;                 // Retrocedemos al ultimo fotograma.
                        isEnded = true;                 // Indicamos que la animacion ha terminado.
                    }       
                }

                // Devolvemos la posicion del fotograma actual:
                drawFrame.X = currentFrame * Current.FirstFrame.Width + Current.FirstFrame.X;  // Actualizamos la coordenada X de la estructura.
            }
        }        
    }
}