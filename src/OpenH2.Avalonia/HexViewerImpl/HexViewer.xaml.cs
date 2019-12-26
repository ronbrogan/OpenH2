using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Metadata;
using PropertyChanged;
using OpenH2.AvaloniaControls.HexViewerImpl;

//[assembly: XmlnsDefinition("https://github.com/ronbrogan/openh2/avaloniacontrols", "OpenH2.AvaloniaControls.HexViewer")]
namespace OpenH2.AvaloniaControls
{
    [DoNotNotify]
    public class HexViewer : UserControl
    {
        public static readonly DirectProperty<HexViewer, Memory<byte>> DataProperty =
            AvaloniaProperty.RegisterDirect<HexViewer, Memory<byte>>(nameof(Data), h => h.Data, (h,v) => { h.Data = v.Slice(0, Math.Min(40000, v.Length)); h.allData = v; });

        public static readonly DirectProperty<HexViewer, ObservableCollection<HexViewerFeature>> FeaturesProperty =
            AvaloniaProperty.RegisterDirect<HexViewer, ObservableCollection<HexViewerFeature>>(nameof(Features), h => h.Features, (h, v) => h.Features = v);

        public static readonly DirectProperty<HexViewer, HexViewerFeature> SelectedFeatureProperty =
            AvaloniaProperty.RegisterDirect<HexViewer, HexViewerFeature>(nameof(SelectedFeature), h => h.SelectedFeature, (h, v) => h.SelectedFeature = v);

        public static readonly DirectProperty<HexViewer, int> SelectedOffsetProperty =
            AvaloniaProperty.RegisterDirect<HexViewer, int>(nameof(SelectedOffset), h => h.SelectedOffset, (h, v) => h.SelectedOffset = v, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly DirectProperty<HexViewer, bool> IsDisabledProperty =
            AvaloniaProperty.RegisterDirect<HexViewer, bool>(nameof(IsDisabled), h => h.IsDisabled, (h,v) => h.IsDisabled = v);

        private bool _isDisabled;
        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                SetAndRaise(IsDisabledProperty, ref _isDisabled, value);
                UpdateData();
            }
        }

        private Memory<byte> allData;
        private Memory<byte> _data;
        private Memory<byte> Data
        {
            get => _data;
            set
            {
                SetAndRaise(DataProperty, ref _data, value);
                UpdateData();
            }
        }

        private ObservableCollection<HexViewerFeature> _features;
        private ObservableCollection<HexViewerFeature> Features
        {
            get => _features;
            set
            {
                SetAndRaise(FeaturesProperty, ref _features, value);
                UpdateData();
            }
        }

        private HexViewerFeature _selectedFeature;
        private HexViewerFeature SelectedFeature
        {
            get => _selectedFeature;
            set
            {
                SetAndRaise(SelectedFeatureProperty, ref _selectedFeature, value);
                UpdateHexBox();
            }
        }

        private int _selectedOffset;
        public int SelectedOffset
        {
            get => _selectedOffset;
            set
            {
                SetAndRaise(SelectedOffsetProperty, ref _selectedOffset, value);
                UpdateHexBox();
            }
        }

        private int LineSize { get; set; } = 12;

        private ScrollViewer Scroller { get; set; }
        private TextBlock AddressBox { get; set; }
        private TextBlock HexBox { get; set; }
        private TextBlock AsciiBox { get; set; }
        private Button ExportButton { get; set; }

        private TextBlock[] Boxes { get; set; }

        private HexViewerViewModel DataVM { get; set; }

        private int lastScrollRangeSize = -1;
        private double lastScrollFeatureUpdate = 0;


        public HexViewer()
        {
            this.InitializeComponent();

            this.DataVM = new HexViewerViewModel();
            this.Get<Grid>("mainGrid").DataContext = this.DataVM;

            this.Features = new ObservableCollection<HexViewerFeature>();

            this.Scroller = this.FindControl<ScrollViewer>("scroller");
            this.AddressBox = this.FindControl<TextBlock>("addressBox");
            this.HexBox = this.FindControl<TextBlock>("hexBox");
            this.AsciiBox = this.FindControl<TextBlock>("asciiBox");
            this.ExportButton = this.FindControl<Button>("exportButton");

            this.Scroller.ObservableForProperty(s => s.Offset).Subscribe(o =>
            {
                //a.start < b.end && b.start < a.end
                if (o.Value.Y < lastScrollFeatureUpdate-(lastScrollRangeSize / 4d)
                    || o.Value.Y > lastScrollFeatureUpdate+(lastScrollRangeSize / 2d))
                {
                    UpdateHexBox();
                }
            });

            Boxes = new[] { AddressBox, HexBox, AsciiBox };

            foreach(var box in Boxes)
            {
                box.FontFamily = "Consolas";
                box.FontSize = 14;
            }

            this.FindControl<TextBox>("gotoBox").KeyDown += this.GotoBox_KeyDown;
            this.ExportButton.Command = ReactiveCommand.CreateFromTask(ExportButton_Click);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void UpdateData()
        {
            if (_data.IsEmpty)
                return;

            if(this.IsDisabled)
            {
                this.HexBox.Text = string.Empty;
                this.AsciiBox.Text = string.Empty;
                this.AddressBox.Text = string.Empty;

                this.HexBox.PointerPressed -= this.HexBox_PointerPressed;

                this.UpdateHexBox();
                return;
            }

            var span = this._data.Span;

            var hexBuilder = new StringBuilder();
            var asciiBuilder = new StringBuilder();
            var addressBuilder = new StringBuilder();

            var hexLines = (int)Math.Ceiling(span.Length / (float)LineSize);

            for (var i = 0; i < hexLines; i++)
            {
                var start = LineSize * i;
                var length = Math.Min(LineSize, span.Length - start);

                var chunk = span.Slice(start, length);

                ToHexString(chunk, hexBuilder);
                ToAsciiString(chunk, asciiBuilder);
                addressBuilder.AppendLine((i * LineSize).ToString().PadLeft(7, '0'));
            }

            this.HexBox.Text = hexBuilder.ToString();
            this.AsciiBox.Text = asciiBuilder.ToString();
            this.AddressBox.Text = addressBuilder.ToString();

            this.HexBox.PointerPressed -= this.HexBox_PointerPressed;
            this.HexBox.PointerPressed += this.HexBox_PointerPressed;

            this.UpdateHexBox();
        }

        private void UpdateHexBox()
        {
            HexBox.FormattedText.Spans = GetNearbySpans();

            HexBox.InvalidateVisual();
        }

        private List<FormattedTextStyleSpan> GetNearbySpans()
        {
            var (rangeStart, rangeEnd) = GetApproximateScrollerVisibleChars();

            var hexSpans = new List<FormattedTextStyleSpan>();

            if (this.Features != null)
            {
                //a.start < b.end && b.start < a.end

                hexSpans.AddRange(this.Features
                    .Where(f => OffsetToCursor(f.Start) < rangeEnd 
                                && rangeStart <= OffsetToCursor(f.Start + f.Length))
                    .Select(GetFormattedSpan)
                    .ToList());
            }

            var start = OffsetToCursor(this.SelectedOffset);
            var length = OffsetToCursor(this.SelectedOffset + this.DataVM.HighlightSize / 8) - start;

            hexSpans.Add(new FormattedTextStyleSpan(start, length, this.DataVM.HighlightColor.Brush));

            return hexSpans;
        }

        private (int, int) GetApproximateScrollerVisibleChars()
        {
            var offset = this.Scroller.Offset;

            var topLeft = this.HexBox.FormattedText.HitTestPoint(new Point(0, offset.Y));
            var bottomRight = this.HexBox.FormattedText.HitTestPoint(
                new Point(this.HexBox.Bounds.Right, offset.Y + this.Scroller.Bounds.Bottom));

            var size = bottomRight.TextPosition - topLeft.TextPosition;

            lastScrollFeatureUpdate = offset.Y;
            lastScrollRangeSize = size ;


            return (topLeft.TextPosition - size, bottomRight.TextPosition + size);
        }

        private void HexBox_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var clientRelative = e.GetPosition(this.HexBox);

            var hit = this.HexBox.FormattedText.HitTestPoint(clientRelative);

            var pos = hit.TextPosition;

            this.SelectedOffset = CursorToOffset(pos);
            this.SelectedFeature = null;

            for(var i = _features.Count - 1; i > 0; i--)
            {
                var feature = _features[i];

                var start = OffsetToCursor(feature.Start);
                var end = OffsetToCursor(feature.Start + feature.Length);

                if (pos >= start && pos < end)
                {
                    this.SelectedFeature = feature;
                    return;
                }
            }
        }

        private async Task ExportButton_Click()
        {
            var dialog = new SaveFileDialog
            {
                InitialDirectory = "D:\\",
                Title = "Save Data"
            };

            var path = await dialog.ShowAsync(AvaloniaLocator.Current.GetService<Window>());

            File.WriteAllBytes(path, this.allData.ToArray());
        }

        private void GotoBox_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            var box = (TextBox)sender;

            if (e.Key == Avalonia.Input.Key.Enter)
            {
                if (int.TryParse(box.Text, out var address))
                {
                    this.SelectedOffset = address;
                    var cursor = OffsetToCursor(address);
                    var hit = this.HexBox.FormattedText.HitTestTextPosition(cursor);

                    this.Scroller.Offset = new Vector(this.Scroller.Offset.X, hit.Y);
                }
            }
        }

        private FormattedTextStyleSpan GetFormattedSpan(HexViewerFeature feature)
        {
            var start = OffsetToCursor(feature.Start);
            var length = OffsetToCursor(feature.Start + feature.Length) - start;

            return new FormattedTextStyleSpan(start, length, this.SelectedFeature == feature ? feature.ActiveBrush : feature.Brush);
        }

        private static void ToHexString(Span<byte> data, StringBuilder builder)
        {
            for (var i = 0; i < data.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                {
                    builder.Append(" ");
                }

                builder.Append(data[i].ToString("X2"));
                builder.Append(" ");
            }

            builder.Append("\n");
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

        /// <summary>
        /// Convert a byte offset into the text cursor position
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int OffsetToCursor(int input)
        {
            // Each byte is two hex chars and one space
            var basis = input * 3;

            var gaps = input / 4;

            return basis + gaps;
        }

        /// <summary>
        /// Take text cursor position and return byte offset of selection
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CursorToOffset(int input)
        {
            var gaps = input / 13;

            var bytes = (input - gaps) / 3;

            return bytes;
        }
    }
}
