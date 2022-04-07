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
using PropertyChanged;
using OpenH2.AvaloniaControls.HexViewerImpl;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;

//[assembly: XmlnsDefinition("https://github.com/ronbrogan/openh2/avaloniacontrols", "OpenH2.AvaloniaControls.HexViewer")]
namespace OpenH2.AvaloniaControls
{
    [DoNotNotify]
    public class HexViewer : UserControl
    {
        public static readonly DirectProperty<HexViewer, Memory<byte>> DataProperty =
            AvaloniaProperty.RegisterDirect<HexViewer, Memory<byte>>(nameof(Data), h => h.Data, (h,v) => { h.Data = v; h.allData = v; });

        public static readonly DirectProperty<HexViewer, ObservableCollection<HexViewerFeature>> FeaturesProperty =
            AvaloniaProperty.RegisterDirect<HexViewer, ObservableCollection<HexViewerFeature>>(nameof(Features), h => h.Features, (h, v) => h.Features = v);

        public static readonly DirectProperty<HexViewer, HexViewerFeature> SelectedFeatureProperty =
            AvaloniaProperty.RegisterDirect<HexViewer, HexViewerFeature>(nameof(SelectedFeature), h => h.SelectedFeature, (h, v) => h.SelectedFeature = v);

        public static readonly DirectProperty<HexViewer, int> SelectedOffsetProperty =
            AvaloniaProperty.RegisterDirect<HexViewer, int>(nameof(SelectedOffset), h => h.SelectedOffset, (h, v) => { h.SelectedOffset = v; h.BringIntoView(v); }, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

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

        private int LineSize = 12;
        private int totalLineCount = 0;
        private double lineHeight = 0;
        private int visibleOffset = 0;
        private int visibleLength = 12;

        private Grid HexGrid { get; set; }
        private ScrollBar Scroller { get; set; }
        private TextPresenter AddressBox { get; set; }
        private TextPresenter HexBox { get; set; }
        private TextPresenter AsciiBox { get; set; }
        private Button ExportButton { get; set; }
        private TextBox GotoBox { get; }
        private TextBox BasisBox { get; }
        private TextPresenter[] Boxes { get; set; }

        private HexViewerViewModel DataVM { get; set; }

        


        public HexViewer()
        {
            this.InitializeComponent();

            this.DataVM = new HexViewerViewModel();
            this.Get<Grid>("mainGrid").DataContext = this.DataVM;

            this.Features = new ObservableCollection<HexViewerFeature>();

            this.HexGrid = this.FindControl<Grid>("hexGrid");
            this.Scroller = this.FindControl<ScrollBar>("scroller");
            this.AddressBox = this.FindControl<TextPresenter>("addressBox");
            this.HexBox = this.FindControl<TextPresenter>("hexBox");
            this.AsciiBox = this.FindControl<TextPresenter>("asciiBox");
            this.ExportButton = this.FindControl<Button>("exportButton");
            this.GotoBox = this.FindControl<TextBox>("gotoBox");
            this.BasisBox = this.FindControl<TextBox>("basisBox");

            Boxes = new[] { AddressBox, HexBox, AsciiBox };

            foreach(var box in Boxes)
            {
                box.FontFamily = "Consolas";
                box.FontSize = 14;
            }

            var layout = new TextLayout("0", new Typeface("Consolas"), 14, Brushes.Black);
            this.lineHeight = layout.Size.Height;

            this.GotoBox.KeyDown += this.OffsetBox_KeyDown;
            this.BasisBox.KeyDown += this.OffsetBox_KeyDown;
            this.ExportButton.Command = ReactiveCommand.CreateFromTask(ExportButton_Click);

            this.HexGrid.PointerWheelChanged += (s,e) => this.Scroller.SetValue(ScrollBar.ValueProperty, this.Scroller.Value - e.Delta.Y);
            this.Scroller.GetSubject(ScrollBar.ValueProperty).Subscribe(this.Scroller_Scroll);
        }

        private void Scroller_Scroll(double offset)
        {
            UpdateHexBox();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void UpdateData()
        {
            if (_data.IsEmpty)
                return;

            this.visibleOffset = 0;
            this.totalLineCount = (int)Math.Ceiling(this.Data.Length / (float)LineSize);

            this.Scroller.IsEnabled = true;
            this.Scroller.Minimum = 0;
            this.Scroller.Maximum = totalLineCount;
            this.Scroller.Value = 0;
            this.Scroller.Visibility = ScrollBarVisibility.Visible;

            this.HexBox.PointerPressed -= this.HexBox_PointerPressed;
            this.HexBox.PointerPressed += this.HexBox_PointerPressed;

            Dispatcher.UIThread.Post(() =>
            {
                this.UpdateHexBox();
            }, DispatcherPriority.Render);
        }

        private void UpdateHexBox()
        {
            var hexBuilder = new StringBuilder();
            var asciiBuilder = new StringBuilder();
            var addressBuilder = new StringBuilder();
            
            var visibleLines = (int)(this.HexBox.Bounds.Height / this.lineHeight);
            this.visibleOffset = this.LineSize * (int)this.Scroller.Value;
            this.visibleLength = visibleLines * this.LineSize;
            this.Scroller.Maximum = this.totalLineCount - visibleLines;
            this.Scroller.ViewportSize = visibleLines;

            for (var i = 0; i < visibleLines; i++)
            {
                var start = this.visibleOffset + LineSize * i;
                var length = Math.Min(LineSize, this.Data.Length - start);

                if (start > this.Data.Length || start + length > this.Data.Length)
                    break;

                var chunk = this.Data.Span.Slice(start, length);

                ToHexString(chunk, hexBuilder);
                ToAsciiString(chunk, asciiBuilder);
                addressBuilder.AppendLine((this.visibleOffset + (i * LineSize)).ToString().PadLeft(7, '0'));
            }

            this.HexBox.Text = hexBuilder.ToString();
            this.AsciiBox.Text = asciiBuilder.ToString();
            this.AddressBox.Text = addressBuilder.ToString();

            // Setting Text on HexBox causes the current FormattedText to be thrown away
            // Need to set spans after that happens, so Posting to UI Thread here
            Dispatcher.UIThread.Post(() =>
            {
                this.HexBox.FormattedText.Spans = GetVisibleSpans();
                this.HexBox.InvalidateVisual();
            }, DispatcherPriority.Render);
        }

        private List<FormattedTextStyleSpan> GetVisibleSpans()
        {
            var hexSpans = new List<FormattedTextStyleSpan>();

            if (this.Features != null)
            {
                hexSpans.AddRange(this.Features
                    .Where(f => f.Start < this.visibleOffset + this.visibleLength
                                && this.visibleOffset <= f.Start + f.Length)
                    .Select(GetFormattedSpan)
                    .ToList());
            }

            var start = OffsetToCursor(this.SelectedOffset);
            var length = OffsetToCursor(this.SelectedOffset + this.DataVM.HighlightSize / 8) - start;

            hexSpans.Add(new FormattedTextStyleSpan(start, length, this.DataVM.HighlightColor.Brush));

            return hexSpans;
        }

        private void HexBox_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var clientRelative = e.GetPosition(this.HexBox);

            var hit = this.HexBox.FormattedText.HitTestPoint(clientRelative);

            var pos = hit.TextPosition;

            this.SelectedOffset = CursorToOffset(pos);

            if(e.ClickCount == 2)
            {
                this.BasisBox.Text = this.SelectedOffset.ToString();
                this.GotoBox.Text = "0";
            }
            else
            {
                this.SelectedFeature = null;

                for (var i = _features.Count - 1; i > 0; i--)
                {
                    var feature = _features[i];

                    var end = feature.Start + feature.Length;

                    if (this.SelectedOffset >= feature.Start && this.SelectedOffset < end)
                    {
                        this.SelectedFeature = feature;
                        return;
                    }
                }
            }
        }

        private string LastSaveLocation = "D:\\";

        private async Task ExportButton_Click()
        {
            var dialog = new SaveFileDialog
            {
                InitialDirectory = LastSaveLocation,
                Title = "Save Data"
            };

            if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                try
                {
                    var path = await dialog.ShowAsync(desktop.MainWindow);

                    if (string.IsNullOrWhiteSpace(path))
                        return;

                    File.WriteAllBytes(path, this.allData.ToArray());
                    LastSaveLocation = Path.GetDirectoryName(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void OffsetBox_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                UpdateSelectedOffset();
            }
        }

        private void UpdateSelectedOffset()
        { 
            if (int.TryParse(this.BasisBox.Text, out var basis) == false)
            {
                this.BasisBox.Text = "0";
                basis = 0;
            }

            if(int.TryParse(this.GotoBox.Text, out var offset) == false)
            {
                offset = this.SelectedOffset;
                this.GotoBox.Text = offset.ToString();
            }

            if(this.SelectedOffset != basis + offset)
            {
                this.SelectedOffset = basis + offset;
            }
        }

        private void BringIntoView(int address)
        {
            int.TryParse(this.BasisBox.Text, out var basis);

            this.GotoBox.Text = (address - basis).ToString();

            var addressLine = address / this.LineSize;

            if(addressLine < this.Scroller.Value || addressLine > this.Scroller.Value + (this.visibleLength / this.LineSize))
            {
                this.Scroller.Value = Math.Min(addressLine, this.Scroller.Maximum);
            }
        }

        private FormattedTextStyleSpan GetFormattedSpan(HexViewerFeature feature)
        {
            var start = OffsetToCursor(feature.Start);
            var length = OffsetToCursor(feature.Start + feature.Length) - start;


            if(start < 0)
            {
                length += start;

                start = 0;
            }

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
        private int OffsetToCursor(int input)
        {
            input -= this.visibleOffset;

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
        private int CursorToOffset(int input)
        {
            var gaps = input / 13;

            var bytes = (input - gaps) / 3;

            return bytes + this.visibleOffset;
        }
    }
}
