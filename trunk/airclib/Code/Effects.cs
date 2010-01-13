using System;
using System.Collections.Generic;
using System.Text;
using airclib.Constants;

namespace airclib
{
    public class TextEffects
    {
        //Constants 
        public const string BoldFont = "\u0002";
        public const string ColoredFont = "\u0003";
        public const string UnderlineFont = "\u001F";
        public const string FontEnd = "\u000E";

        /// <summary>
        /// Changes color of wanted text.
        /// </summary>
        /// <param name="Text">Wanted text.</param>
        /// <param name="Color">Wanted color.</param>
        /// <returns>Returns text with changed color.</returns>
        public string ColorText(string Text, ColorMessages Color)
        {
            return ColoredFont + (int)Color + Text + FontEnd;
        }
        /// <summary>
        /// Makes wanted text bold.
        /// </summary>
        /// <param name="Text">Wanted text.</param>
        /// <returns>Text with bold effect.</returns>
        public string BoldText(string Text)
        {
            return BoldFont + Text + FontEnd;
        }
        /// <summary>
        /// Underlines wanted text.
        /// </summary>
        /// <param name="Text">Wanted text.</param>
        /// <returns>Underlined text.</returns>
        public string UnderlineText(string Text)
        {
            return UnderlineFont + Text + FontEnd;
        }
    }
}
