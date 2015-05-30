using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using XNAColor = Microsoft.Xna.Framework.Color;

namespace TLSA.Engine.Graphics
{
    /// <summary>
    /// Enumeracion de filtros de mezclado.
    /// </summary>
    public enum BlendFilters
    {
        /// <summary>
        /// Estado por defecto. Modula la opacidad del sprite a traves de la componente Alfa del color usado para dibujarlo.
        /// </summary>
        AlphaBlending,
        /// <summary>
        /// Modula la opacidad del sprite en base al tono del color de los pixeles mas cercanos al blanco.
        /// </summary>
        Aditive,
        /// <summary>
        /// Invierte el efecto de la opacidad aditiva.
        /// </summary>
        Inverse,
        /// <summary>
        /// Realiza una exclusion logica entre el color de origen del mapa y el color destino del backbuffer.
        /// </summary>
        /// <remarks>La exclusion de color no se aplica sobre pixeles de color Negro {R:0 G:0 B:0 A:255}.</remarks>
        XOR,
        /// <summary>
        /// Especifico para renderizar fuentes de texto de forma suave.
        /// </summary>
        TextSmothRendering,
        /// <summary>
        /// Aplica un efecto personalizado.
        /// </summary>
        /// <remarks>Establezca los estados del filtro mediante el metodo SetCustomParams().</remarks>
        Custom
    }

    /// <summary>
    /// Estados de mezcla por componente Alfa del color.
    /// </summary>
    /// <remarks>Define efectos basicos basados en estados de colores y mezclas a traves del canal alfa del color de los pixeles de escena y la textura a dibujar.</remarks>
    public static class BlendFactory
    {
        private static BlendState alpha, aditive, inverse, xor, text, custom;        

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <remarks>Inicializa los estados de blending.</remarks>
        internal static void Initialize()
        {
            // Color:
            alpha = BlendState.NonPremultiplied;

            // Aditivo:
            aditive = BlendState.Additive;

            // Sustrativo:
            inverse = SetBlendingState(BlendFunction.ReverseSubtract, Blend.InverseDestinationColor, Blend.One);

            // XOR:            
            xor = SetBlendingState(BlendFunction.Add, Blend.InverseDestinationColor, Blend.InverseSourceAlpha);

            // Renderizado suave para textos:
            text = BlendState.AlphaBlend;

            // Personalizado:
            custom = BlendState.Opaque;
        }

        /// <summary>
        /// Traduce el valor de la enumeracion al filtro indicado.
        /// </summary>
        /// <param name="filter">Filtro de mezclado.</param>
        /// <returns>Instancia de BlendState con los parametros requeridos.</returns>
        internal static BlendState GetBlendState(BlendFilters filter)
        {
            switch (filter)
            {
                case BlendFilters.AlphaBlending: default: return alpha;
                case BlendFilters.Aditive: return aditive;
                case BlendFilters.Inverse: return inverse;
                case BlendFilters.XOR: return xor;
                case BlendFilters.TextSmothRendering: return text;
                case BlendFilters.Custom: return custom;                
            }
        }

        /// <summary>
        /// Permite configurar el estado personalizado de mezclado de colores.
        /// </summary>
        /// <param name="BlendFunction">Funcion de mezclado de colores.</param>
        /// <param name="SourceBlend">Comportamiento de la mezcla en base a los colores de la textura a dibujar.</param>
        /// <param name="DestinationBlend">Comportamiento de la mezcla en base a los colores de la escena.</param>
        /// <remarks>Para aplicar estos valores se debe utilizar el filtro 'Custom' de la lista.</remarks>
        public static void SetCustomParams(BlendFunction BlendFunction, Blend SourceBlend, Blend DestinationBlend)
        {
            custom = SetBlendingState(BlendFunction, SourceBlend, DestinationBlend);
        }

        private static BlendState SetBlendingState(BlendFunction blendFunction, Blend sourceBlend, Blend destBlend)
        {
            BlendState ret = new BlendState();

            ret.AlphaBlendFunction = blendFunction;
            ret.AlphaDestinationBlend = destBlend;
            ret.AlphaSourceBlend = sourceBlend;
            ret.BlendFactor = XNAColor.White;
            ret.ColorBlendFunction = blendFunction;
            ret.ColorDestinationBlend = destBlend;
            ret.ColorSourceBlend = sourceBlend;
            ret.ColorWriteChannels = ColorWriteChannels.All;
            ret.ColorWriteChannels1 = ColorWriteChannels.Red;
            ret.ColorWriteChannels2 = ColorWriteChannels.Green;
            ret.ColorWriteChannels3 = ColorWriteChannels.Blue;
            ret.MultiSampleMask = -1;

            return ret;
        }
    }
}