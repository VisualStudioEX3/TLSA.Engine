namespace TLSA.Engine
{
    /// <summary>
    /// Define a drawable object.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Get or set if the object is draw.
        /// </summary>
        bool Visible { get; set; }
        /// <summary>
        /// Dibuja el objeto.
        /// </summary>
        /// <param name="gameTime"></param>
        void Draw(Microsoft.Xna.Framework.GameTime gameTime);
    }
}