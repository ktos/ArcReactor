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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ArcReactor.Models
{
    public class LedColor : INotifyPropertyChanged
    {
        public int Index { get; set; }

        private Color color;

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

        public void SetRgb(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        public string ToDeviceCommand()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("c");
            sb.Append((char)Index);
            sb.Append((char)R);
            sb.Append((char)G);
            sb.Append((char)B);

            return sb.ToString();
        }


        /// <summary>
        /// Even run when databound property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handles when property is changed raising <see cref="PropertyChanged"/>
        /// event.
        /// 
        /// Part of <see cref="INotifyPropertyChanged"/> implementation.
        /// </summary>
        /// <param name="name">Name of a changed property</param>
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

    }
}
