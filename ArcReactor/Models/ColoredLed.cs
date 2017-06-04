#region License

/*
 * ArcReactor
 *
 * Copyright (C) Marcin Badurowicz <m at badurowicz dot net> 2017
 *
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files
 * (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
 * BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
 * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#endregion License

using System.ComponentModel;
using Windows.UI;

namespace ArcReactor.Models
{
    /// <summary>
    /// Represents a single RGB led on Arc Reactor device
    /// </summary>
    public class ColoredLed : INotifyPropertyChanged
    {
        /// <summary>
        /// Index of the LED, starting from 0, where 0 is a central
        /// ("core") and the next are either "core" or "ring"
        /// </summary>
        public int Index { get; set; }

        private Color color;

        /// <summary>
        /// The color of the LED
        /// </summary>
        public Color Color
        {
            get
            {
                return color;
            }

            private set
            {
                if (this.color != value)
                {
                    color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        private int r;

        /// <summary>
        /// Intensity value for red color
        /// </summary>
        public int R
        {
            get
            {
                return r;
            }

            set
            {
                if (this.r != value)
                {
                    r = value;
                    OnPropertyChanged(nameof(R));

                    Color = Color.FromArgb(255, (byte)R, (byte)G, (byte)B);
                }
            }
        }

        private int g;

        /// <summary>
        /// Intensity value for green color
        /// </summary>
        public int G
        {
            get
            {
                return g;
            }

            set
            {
                if (this.g != value)
                {
                    g = value;
                    OnPropertyChanged(nameof(G));

                    Color = Color.FromArgb(255, (byte)R, (byte)G, (byte)B);
                }
            }
        }

        private int b;

        /// <summary>
        /// Intensity value for blue color
        /// </summary>
        public int B
        {
            get
            {
                return b;
            }

            set
            {
                if (this.b != value)
                {
                    b = value;
                    OnPropertyChanged(nameof(B));

                    Color = Color.FromArgb(255, (byte)R, (byte)G, (byte)B);
                }
            }
        }

        /// <summary>
        /// Sets the value for red, green and blue properties all in one
        /// </summary>
        /// <param name="r">New value for red</param>
        /// <param name="g">New value for green</param>
        /// <param name="b">New value for blue</param>
        public void SetRgb(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Converts the current LED data into a Arc Reactor device
        /// command for setting the desired LED
        /// </summary>
        /// <returns>Device command to be sent to Arc Reactor device</returns>
        public byte[] ToDeviceCommand()
        {
            byte[] sb = new byte[5];

            sb[0] = (byte)'c';
            sb[1] = (byte)Index;

            sb[2] = (byte)R;
            sb[3] = (byte)G;
            sb[4] = (byte)B;

            return sb;
        }

        /// <summary>
        /// Even run when databound property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handles when property is changed raising <see
        /// cref="PropertyChanged"/> event.
        ///
        /// Part of <see cref="INotifyPropertyChanged"/> implementation.
        /// </summary>
        /// <param name="name">Name of a changed property</param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}