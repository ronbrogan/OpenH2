using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Brush = Avalonia.Media.Brush;

namespace OpenH2.Avalonia
{
    public class HexViewer : UserControl
    {
        public static readonly DirectProperty<HexViewer, byte[]> DataProperty =
            AvaloniaProperty.RegisterDirect<HexViewer, byte[]>(nameof(Data), h => h.Data, (h,v) => h.Data = v);

        private byte[] _data;
        private byte[] Data { get => _data; set => UpdateData(value); }

        private TextBlock AddressBox { get; set; }
        private TextBlock HexBox { get; set; }
        private TextBlock AsciiBox { get; set; }

        private TextBlock[] Boxes { get; set; }

        public HexViewer()
        {
            this.InitializeComponent();

            AddressBox = this.FindControl<TextBlock>("addressBox");
            HexBox = this.FindControl<TextBlock>("hexBox");
            AsciiBox = this.FindControl<TextBlock>("asciiBox");

            Boxes = new[] { AddressBox, HexBox, AsciiBox };

            foreach(var box in Boxes)
            {
                box.FontFamily = "Consolas";
                box.FontSize = 14;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void UpdateData(byte[] value)
        {
            SetAndRaise(DataProperty, ref _data, value);

            if (_data == null)
                return;

            Memory<byte> data = value;
            var span = data.Span;

            var chunkSize = 12;

            var hexBuilder = new StringBuilder();
            var asciiBuilder = new StringBuilder();
            var addressBuilder = new StringBuilder();

            for(var i = 0; i < value.Length / chunkSize; i++)
            {
                var chunk = span.Slice(chunkSize * i, chunkSize);

                ToHexString(chunk, hexBuilder);
                ToAsciiString(chunk, asciiBuilder);
                addressBuilder.AppendLine((i * chunkSize).ToString().PadLeft(7, '0'));
            }

            HexBox.Text = hexBuilder.ToString();
            AsciiBox.Text = asciiBuilder.ToString();
            AddressBox.Text = addressBuilder.ToString();

            var a = new FormattedTextStyleSpan(0, 12, Brushes.Blue);

            HexBox.FormattedText.Spans = new List<FormattedTextStyleSpan>()
            {
                a
            };
        }

        private static void ToHexString(Span<byte> data, StringBuilder builder)
        {
            for(var i = 0; i < data.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                {
                    builder.Append(" ");
                }

                builder.Append(data[i].ToString("X2"));
                builder.Append(" ");
            }

            builder.AppendLine();
        }

        private static void ToAsciiString(Span<byte> data, StringBuilder builder)
        {
            for (var i = 0; i < data.Length; i++)
            {
                var val = data[i];

                if (val > 31 && val < 127)
                {
                    builder.Append(asciiChars[val]);
                }
                else
                {
                    builder.Append('.');
                }
            }

            builder.AppendLine();
        }

        private static Dictionary<byte, char> asciiChars = new Dictionary<byte, char>
        {
            {32, ' ' },
            {33, '!' },
            {34, '"' },
            {35, '#' },
            {36, '$' },
            {37, '%' },
            {38, '&' },
            {39, '\'' },
            {40, '(' },
            {41, ')' },
            {42, '*' },
            {43, '+' },
            {44, ',' },
            {45, '-' },
            {46, '.' },
            {47, '/' },
            {48, '0' },
            {49, '1' },
            {50, '2' },
            {51, '3' },
            {52, '4' },
            {53, '5' },
            {54, '6' },
            {55, '7' },
            {56, '8' },
            {57, '9' },
            {58, ':' },
            {59, ';' },
            {60, '<' },
            {61, '=' },
            {62, '>' },
            {63, '?' },
            {64, '@' },
            {65, 'A' },
            {66, 'B' },
            {67, 'C' },
            {68, 'D' },
            {69, 'E' },
            {70, 'F' },
            {71, 'G' },
            {72, 'H' },
            {73, 'I' },
            {74, 'J' },
            {75, 'K' },
            {76, 'L' },
            {77, 'M' },
            {78, 'N' },
            {79, 'O' },
            {80, 'P' },
            {81, 'Q' },
            {82, 'R' },
            {83, 'S' },
            {84, 'T' },
            {85, 'U' },
            {86, 'V' },
            {87, 'W' },
            {88, 'X' },
            {89, 'Y' },
            {90, 'Z' },
            {91, '[' },
            {92, '\\'},
            {93, ']' },
            {94, '^' },
            {95, '_' },
            {96, '\'' },
            {97, 'a' },
            {98, 'b' },
            {99, 'c' },
            {100,'d' },
            {101,'e' },
            {102,'f' },
            {103,'g' },
            {104,'h' },
            {105,'i' },
            {106,'j' },
            {107,'k' },
            {108,'l' },
            {109,'m' },
            {110,'n' },
            {111,'o' },
            {112,'p' },
            {113,'q' },
            {114,'r' },
            {115,'s' },
            {116,'t' },
            {117,'u' },
            {118,'v' },
            {119,'w' },
            {120,'x' },
            {121,'y' },
            {122,'z' },
            {123,'{' },
            {124,'|' },
            {125,'}' },
            {126,'~' },
        };
    }
}
