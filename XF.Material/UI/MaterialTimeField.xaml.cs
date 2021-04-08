using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Material.Forms.Resources;
using XF.Material.Forms.UI.Internals;
using TypeConverterAttribute = Xamarin.Forms.TypeConverterAttribute;

namespace XF.Material.Forms.UI
{
    /// <inheritdoc cref="ContentView" />
    /// <summary>
    /// A control that let users enter and edit text.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MaterialTimeField : ContentView, IMaterialElementConfiguration
    {
        public static readonly BindableProperty AlwaysShowUnderlineProperty = BindableProperty.Create(nameof(AlwaysShowUnderline), typeof(bool), typeof(MaterialTimeField), false);
        public static readonly BindableProperty ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), typeof(MaterialTimeField), Material.Color.Error);
        public static readonly BindableProperty ErrorTextProperty = BindableProperty.Create(nameof(ErrorText), typeof(string), typeof(MaterialTimeField));
        public static readonly BindableProperty FloatingPlaceholderColorProperty = BindableProperty.Create(nameof(FloatingPlaceholderColor), typeof(Color), typeof(MaterialTimeField), Color.FromHex("#99000000"));
        public static readonly BindableProperty FloatingPlaceholderEnabledProperty = BindableProperty.Create(nameof(FloatingPlaceholderEnabled), typeof(bool), typeof(MaterialTimeField), true);
        public static readonly BindableProperty FloatingPlaceholderFontSizeProperty = BindableProperty.Create(nameof(FloatingPlaceholderFontSize), typeof(double), typeof(MaterialTimeField), 0d);
        public static readonly BindableProperty FocusCommandProperty = BindableProperty.Create(nameof(FocusCommand), typeof(Command<bool>), typeof(MaterialTimeField));
        public static readonly BindableProperty HasErrorProperty = BindableProperty.Create(nameof(HasError), typeof(bool), typeof(MaterialTimeField), false);
        public static readonly BindableProperty HelperTextColorProperty = BindableProperty.Create(nameof(HelperTextColor), typeof(Color), typeof(MaterialTimeField), Color.FromHex("#99000000"));
        public static readonly BindableProperty HelperTextFontFamilyProperty = BindableProperty.Create(nameof(HelperTextFontFamily), typeof(string), typeof(MaterialTimeField));
        public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(nameof(HelperText), typeof(string), typeof(MaterialTimeField), string.Empty);
        public static readonly BindableProperty HorizontalPaddingProperty = BindableProperty.Create(nameof(HorizontalPadding), typeof(MaterialHorizontalThickness), typeof(MaterialTimeField), new MaterialHorizontalThickness(12d), defaultBindingMode: BindingMode.OneTime);
        public static readonly BindableProperty LeadingIconProperty = BindableProperty.Create(nameof(LeadingIcon), typeof(ImageSource), typeof(MaterialTimeField));
        public static readonly BindableProperty LeadingIconTintColorProperty = BindableProperty.Create(nameof(LeadingIconTintColor), typeof(Color), typeof(MaterialTimeField), Color.FromHex("#99000000"));
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(MaterialTimeField), Color.FromHex("#99000000"));
        public static readonly BindableProperty PlaceholderFontFamilyProperty = BindableProperty.Create(nameof(PlaceholderFontFamily), typeof(string), typeof(MaterialTimeField));
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(MaterialTimeField), string.Empty);
        public static readonly BindableProperty ShouldAnimateUnderlineProperty = BindableProperty.Create(nameof(ShouldAnimateUnderline), typeof(bool), typeof(MaterialTimeField), true);
        public static readonly BindableProperty TimeChangeCommandProperty = BindableProperty.Create(nameof(TimeChangeCommand), typeof(Command<TimeSpan?>), typeof(MaterialTimeField));
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(MaterialTimeField), Color.FromHex("#D0000000"));
        public static readonly BindableProperty TextFontFamilyProperty = BindableProperty.Create(nameof(TextFontFamily), typeof(string), typeof(MaterialTimeField));
        public static readonly BindableProperty TextFontSizeProperty = BindableProperty.Create(nameof(TextFontSize), typeof(double), typeof(MaterialTimeField), 16d);
        public static readonly BindableProperty TimeProperty = BindableProperty.Create(nameof(Time), typeof(TimeSpan?), typeof(MaterialTimeField), null, BindingMode.TwoWay);
        public static readonly BindableProperty TintColorProperty = BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(MaterialTimeField), Material.Color.Secondary);
        public static readonly BindableProperty ClearIconProperty = BindableProperty.Create(nameof(ClearIcon), typeof(ImageSource), typeof(MaterialTimeField), new FileImageSource { File = "xf_clear" });
        public static readonly BindableProperty ErrorIconProperty = BindableProperty.Create(nameof(ErrorIcon), typeof(ImageSource), typeof(MaterialTimeField), new FileImageSource { File = "xf_error" });
        public static readonly BindableProperty DropDrownArrowIconProperty = BindableProperty.Create(nameof(DropDrownArrowIcon), typeof(ImageSource), typeof(MaterialTimeField), new FileImageSource { File = "xf_arrow_dropdown" });
        public static readonly BindableProperty UnderlineColorProperty = BindableProperty.Create(nameof(UnderlineColor), typeof(Color), typeof(MaterialTimeField), Color.FromHex("#99000000"));
        public new static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(MaterialTimeField), Color.FromHex("#DCDCDC"));
        public static readonly BindableProperty FormatProperty = BindableProperty.Create(nameof(Format), typeof(string), typeof(MaterialTimeField), "t");

        private readonly Easing _animationCurve = Easing.SinOut;
        private readonly Dictionary<string, Action> _propertyChangeActions;
        private DisplayInfo _lastDeviceDisplay;
        const uint AnimDurationMs = 350; //250/2;
        const double AnimDurationS = AnimDurationMs / 1000.0;
        private bool? IsFloating = null;

        /// <summary>
        /// Initializes a new instance of <see cref="MaterialTimeField"/>.
        /// </summary>
        public MaterialTimeField()
        {
            InitializeComponent();

            _propertyChangeActions = new Dictionary<string, Action>
            {
                { nameof(HelperText), () => OnHelperTextChanged() },
                { nameof(ErrorText), () => OnErrorTextChanged() },

                { nameof(Time), () => OnTimeChanged(Time) },
                { nameof(IsEnabled), () => OnEnabledChanged(IsEnabled) },
                { nameof(HasError), () => UpdateErrorState() },
                { nameof(ErrorIcon), () => UpdateErrorState() },
                { nameof(DropDrownArrowIcon), () => UpdateErrorState() },

                { nameof(FloatingPlaceholderEnabled), () => OnFloatingPlaceholderEnabledChanged() },
            };

            timePicker.TimeSelected += TimePicker_TimeChanged;
            //timePicker.SizeChanged += Entry_SizeChanged;
            timePicker.Focused += TimePicker_Focused;
            timePicker.Unfocused += TimePicker_Unfocused;

            _lastDeviceDisplay = DeviceDisplay.MainDisplayInfo;
            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        }

        /// <summary>
        /// Raised when this text field receives focus.
        /// </summary>
        public new event EventHandler<FocusEventArgs> Focused;

        /// <summary>
        /// Raised when this text field loses focus.
        /// </summary>
        public new event EventHandler<FocusEventArgs> Unfocused;

        /// <summary>
        /// Raised when the time has changed.
        /// </summary>
        public event EventHandler<NullableTimeChangedEventArgs> TimeChanged;

        #region properties
        /// <summary>
        /// Gets or sets whether the underline accent of this text field should always show or not.
        /// </summary>
        public bool AlwaysShowUnderline
        {
            get => (bool)GetValue(AlwaysShowUnderlineProperty);
            set => SetValue(AlwaysShowUnderlineProperty, value);
        }

        /// <summary>
        /// Gets or sets the background color of this text field.
        /// </summary>
        public new Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        /// <summary>
        /// String format for the time
        /// </summary>
        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set => SetValue(FormatProperty, value);
        }

        [TypeConverter(typeof(ImageSourceConverter))]
        public ImageSource ClearIcon
        {
            get => (ImageSource)GetValue(ClearIconProperty);
            set => SetValue(ClearIconProperty, value);
        }

        [TypeConverter(typeof(ImageSourceConverter))]
        public ImageSource ErrorIcon
        {
            get => (ImageSource)GetValue(ErrorIconProperty);
            set => SetValue(ErrorIconProperty, value);
        }

        [TypeConverter(typeof(ImageSourceConverter))]
        public ImageSource DropDrownArrowIcon
        {
            get => (ImageSource)GetValue(DropDrownArrowIconProperty);
            set => SetValue(DropDrownArrowIconProperty, value);
        }

        /// <summary>
        /// Gets or sets the color to indicate an error in this text field.
        /// The default value is the color of <see cref="MaterialColorConfiguration.Error"/>.
        /// </summary>
        public Color ErrorColor
        {
            get => (Color)GetValue(ErrorColorProperty);
            set => SetValue(ErrorColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the error text of this text field.
        /// </summary>
        public string ErrorText
        {
            get => (string)GetValue(ErrorTextProperty);
            set => SetValue(ErrorTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the color of the floating placeholder.
        /// </summary>
        public Color FloatingPlaceholderColor
        {
            get => (Color)GetValue(FloatingPlaceholderColorProperty);
            set => SetValue(FloatingPlaceholderColorProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the placeholder label will float at top of the text field when focused or when it has text.
        /// </summary>
        public bool FloatingPlaceholderEnabled
        {
            get => (bool)GetValue(FloatingPlaceholderEnabledProperty);
            set => SetValue(FloatingPlaceholderEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size of the floating placeholder.
        /// </summary>
        public double FloatingPlaceholderFontSize
        {
            get => (double)GetValue(FloatingPlaceholderFontSizeProperty);
            set => SetValue(FloatingPlaceholderFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the command that will be executed when this text field receives or loses focus.
        /// </summary>
        public Command<bool> FocusCommand
        {
            get => (Command<bool>)GetValue(FocusCommandProperty);
            set => SetValue(FocusCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the boolean value whether this text field has an error, and if it will show the its error text.
        /// </summary>
        public bool HasError
        {
            get => (bool)GetValue(HasErrorProperty);
            set => SetValue(HasErrorProperty, value);
        }

        /// <summary>
        /// Gets or sets the helper text of this text field.
        /// </summary>
        public string HelperText
        {
            get => (string)GetValue(HelperTextProperty);
            set => SetValue(HelperTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the color of this text field's helper text.
        /// </summary>
        public Color HelperTextColor
        {
            get => (Color)GetValue(HelperTextColorProperty);
            set => SetValue(HelperTextColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family of this text field's helper text.
        /// </summary>
        public string HelperTextFontFamily
        {
            get => (string)GetValue(HelperTextFontFamilyProperty);
            set => SetValue(HelperTextFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the horizontal padding of the text field.
        /// </summary>
        public MaterialHorizontalThickness HorizontalPadding
        {
            get => (MaterialHorizontalThickness)GetValue(HorizontalPaddingProperty);
            set => SetValue(HorizontalPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the image source of the icon to be showed at the left side of this text field.
        /// </summary>
        [TypeConverter(typeof(ImageSourceConverter))]
        public ImageSource LeadingIcon
        {
            get => (ImageSource)GetValue(LeadingIconProperty);
            set => SetValue(LeadingIconProperty, value);
        }

        /// <summary>
        /// Gets or sets the tint color of the icon of this text field.
        /// </summary>
        public Color LeadingIconTintColor
        {
            get => (Color)GetValue(LeadingIconTintColorProperty);
            set => SetValue(LeadingIconTintColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the text of this text field's placeholder.
        /// </summary>
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);

            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException($"{nameof(Placeholder)} must not be null or empty");

                SetValue(PlaceholderProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the color of this text field's placeholder.
        /// </summary>
        public Color PlaceholderColor
        {
            get => (Color)GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family of this text field's placeholder
        /// </summary>
        public string PlaceholderFontFamily
        {
            get => (string)GetValue(PlaceholderFontFamilyProperty);
            set => SetValue(PlaceholderFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the underline indicator will be animated. If set to false, the underline will not be shown.
        /// </summary>
        public bool ShouldAnimateUnderline
        {
            get => (bool)GetValue(ShouldAnimateUnderlineProperty);
            set => SetValue(ShouldAnimateUnderlineProperty, value);
        }

        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        public TimeSpan? Time
        {
            get => (TimeSpan?)GetValue(TimeProperty);

            set
            {
                if (value.HasValue && !FloatingPlaceholderEnabled)
                    placeholder.IsVisible = false;
                else if (!value.HasValue && !FloatingPlaceholderEnabled)
                    placeholder.IsVisible = true;

                SetValue(TimeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the command that will execute if there is a change in this text field's input text.
        /// </summary>
        public Command<TimeSpan?> TimeChangeCommand
        {
            get => (Command<TimeSpan?>)GetValue(TimeChangeCommandProperty);
            set => SetValue(TimeChangeCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the color of this text field's input text.
        /// </summary>
        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family of this text field's input text.
        /// </summary>
        public string TextFontFamily
        {
            get => (string)GetValue(TextFontFamilyProperty);
            set => SetValue(TextFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the text's font size.
        /// </summary>
        public double TextFontSize
        {
            get => (double)GetValue(TextFontSizeProperty);
            set => SetValue(TextFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the tint color of the underline and the placeholder of this text field when focused.
        /// The default value is the color of <see cref="MaterialColorConfiguration.Secondary"/>.
        /// </summary>
        public Color TintColor
        {
            get => (Color)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the color of the underline when this text field is activated. <see cref="AlwaysShowUnderline"/> is set to true.
        /// </summary>
        public Color UnderlineColor
        {
            get => (Color)GetValue(UnderlineColorProperty);
            set => SetValue(UnderlineColorProperty, value);
        }
        #endregion

        public string SmallText => HasError ? ErrorText : HelperText;
        public bool IsHelperVisible => IsEnabled && !string.IsNullOrEmpty(HelperText);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IMaterialElementConfiguration.ElementChanged(bool created)
        {
        }

        /// <summary>
        /// Requests to set focus on this field.
        /// </summary>
        public new void Focus()
            => timePicker.Focus();

        /// <summary>
        /// Requests to unset the focus on this field.
        /// </summary>
        public new void Unfocus()
            => timePicker.Unfocus();

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext != null)
                Device.BeginInvokeOnMainThread(() => UpdateState(false));
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent != null)
                Device.BeginInvokeOnMainThread(() => UpdateState(false));
        }

        /// <inheritdoc />
        /// <summary>
        /// Method that is called when a bound property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName != null && _propertyChangeActions != null && _propertyChangeActions.TryGetValue(propertyName, out var handlePropertyChange))
                handlePropertyChange();
        }

        /// <summary>
        /// AnimateToInactiveOrFocusedState: !timePicker.NullableTime.HasValue
        /// </summary>
        private void UpdateState(bool animated = true)
        {
            var isFocused = timePicker.IsFocused;
            var isFloating = FloatingPlaceholderEnabled && (Time.HasValue || isFocused);
            var tintColor = isFocused ? (HasError ? ErrorColor : TintColor) : (isFloating ? FloatingPlaceholderColor : PlaceholderColor);

            //Update trailing icon on startup (detected by !animated)
            if (!animated)
                _ = UpdateErrorState(false); //It's fully sync

            UnderlineSetColorState();
            var anim = new Animation();

            if (isFloating != IsFloating)
            {
                IsFloating = isFloating;

                if (FloatingPlaceholderEnabled)
                {
                    var normalPlaceholderFontSize = timePicker.FontSize;
                    var floatingPlaceholderFontSize = FloatingPlaceholderFontSize == 0 ? normalPlaceholderFontSize * 0.75 : FloatingPlaceholderFontSize;
                    var startFont = isFloating ? normalPlaceholderFontSize : floatingPlaceholderFontSize;
                    var endFOnt = isFloating ? floatingPlaceholderFontSize : normalPlaceholderFontSize;
                    var startY = placeholder.TranslationY;
                    var endY = isFloating ? -(normalPlaceholderFontSize * 0.8) : 0;

                    anim.Add(0, AnimDurationS, new Animation(v => placeholder.FontSize = v, startFont, endFOnt, _animationCurve));
                    anim.Add(0, AnimDurationS, new Animation(v => placeholder.TranslationY = v, startY, endY, _animationCurve, () =>
                    {
                        placeholder.TextColor = tintColor;
                    }));
                }
                else
                {
                    placeholder.TextColor = tintColor;
                }
            }
            else
            {
                placeholder.TextColor = tintColor;
            }

            UnderlineSetState(anim);

            if (animated)
                anim.Commit(this, Guid.NewGuid().ToString(), rate: 2, length: AnimDurationMs, easing: _animationCurve);
            else
                anim.Commit(this, Guid.NewGuid().ToString(), 1, 1);
        }

        private async Task UpdateErrorState(bool animated = true)
        {
            var isFocused = timePicker.IsFocused;
            var isFloating = FloatingPlaceholderEnabled && (Time.HasValue || isFocused);
            var tintColor = isFocused ? (HasError ? ErrorColor : TintColor) : (isFloating ? FloatingPlaceholderColor : PlaceholderColor);

            if (HasError)
            {
                trailingIcon.Source = ErrorIcon;

                placeholder.TextColor = tintColor;
                counter.TextColor = ErrorColor;
                UnderlineSetColorState();

                if (string.IsNullOrEmpty(ErrorText) || !animated)
                {
                    helper.TextColor = ErrorColor;
                }
                else
                {
                    await helper.FadeTo(0, AnimDurationMs, _animationCurve);
                    helper.TextColor = ErrorColor;
                    OnPropertyChanged(nameof(SmallText));
                    helper.TranslationY = -4;
                    await Task.WhenAll(
                        helper.FadeTo(1, AnimDurationMs, _animationCurve),
                        helper.TranslateTo(0, 0, AnimDurationMs, _animationCurve));
                }
            }
            else
            {
                trailingIcon.Source = DropDrownArrowIcon;

                placeholder.TextColor = tintColor;
                counter.TextColor = HelperTextColor;
                UnderlineSetColorState();

                if (string.IsNullOrEmpty(ErrorText) || !animated)
                {
                    helper.TextColor = HelperTextColor;
                }
                else
                {
                    await helper.FadeTo(0, AnimDurationMs, _animationCurve);
                    helper.TextColor = HelperTextColor;
                    OnPropertyChanged(nameof(SmallText));
                    helper.TranslationY = -4;
                    await Task.WhenAll(
                        helper.FadeTo(1, AnimDurationMs, _animationCurve),
                        helper.TranslateTo(0, 0, AnimDurationMs, _animationCurve));
                }
            }
        }

        private void UnderlineSetState(Animation anim = null)
        {
            var isFocused = timePicker.IsFocused;
            var hasValue = Time.HasValue;

            var hasLine = hasValue || isFocused;
            var hasThickLine = isFocused;

            var isLineVisible = underline.WidthRequest != 0;

            if (anim == null || !ShouldAnimateUnderline)
            {
                if (hasLine)
                {
                    underline.WidthRequest = -1;
                    underline.HorizontalOptions = LayoutOptions.FillAndExpand;
                    if (hasThickLine)
                        underline.HeightRequest = 2;
                }
                else
                {
                    underline.WidthRequest = 0;
                    underline.HeightRequest = 0;
                    underline.HorizontalOptions = LayoutOptions.Center;
                }
            }
            else
            {
                if (hasLine)
                {
                    anim.Add(0, AnimDurationS, new Animation(v => underline.WidthRequest = v, 0, Width, _animationCurve, () =>
                    {
                        underline.WidthRequest = -1;
                        underline.HorizontalOptions = LayoutOptions.FillAndExpand;
                    }));
                    anim.Add(0, AnimDurationS, new Animation(v => underline.HeightRequest = v, underline.HeightRequest, hasThickLine ? 2 : 1, _animationCurve));
                }
                else if (isLineVisible)
                {
                    anim.Add(0, AnimDurationS, new Animation(v => underline.WidthRequest = v, Width, 0, _animationCurve, () =>
                    {
                        underline.WidthRequest = 0;
                        underline.HeightRequest = 0;
                        underline.HorizontalOptions = LayoutOptions.Center;
                    }));
                }
            }
        }

        private void UnderlineSetColorState()
        {
            var isFocused = timePicker.IsFocused;
            var hasValue = Time.HasValue;

            if (HasError)
                underline.Color = ErrorColor;
            else if (isFocused || hasValue)
                underline.Color = TintColor;
            else if (AlwaysShowUnderline)
                underline.Color = UnderlineColor;
            else
                underline.Color = Color.Transparent;
        }

        private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            if (e.DisplayInfo.Orientation != _lastDeviceDisplay.Orientation)
            {
                UnderlineSetState();
                _lastDeviceDisplay = e.DisplayInfo;
            }
        }

        private void TimePicker_Focused(object sender, FocusEventArgs e)
        {
            FocusCommand?.Execute(e.IsFocused);
            Focused?.Invoke(this, e);
            Device.BeginInvokeOnMainThread(() => UpdateState());
        }

        private void TimePicker_Unfocused(object sender, FocusEventArgs e)
        {
            FocusCommand?.Execute(e.IsFocused);
            Unfocused?.Invoke(this, e);
            Device.BeginInvokeOnMainThread(() => UpdateState());
        }

        //private void Entry_SizeChanged(object sender, EventArgs e)
        //{
        //var baseHeight = FloatingPlaceholderEnabled ? 56 : 40;
        //var diff = timePicker.Height - 20;
        //var rawRowHeight = baseHeight + diff;
        //_autoSizingRow.Height = new GridLength(rawRowHeight);

        //var iconVerticalMargin = (_autoSizingRow.Height.Value - 24) / 2;

        //if (leadingIcon.IsVisible)
        //{
        //    leadingIcon.Margin = new Thickness(HorizontalPadding.Left, iconVerticalMargin, 0, iconVerticalMargin);
        //    timePicker.Margin = new Thickness(12, timePicker.Margin.Top, HorizontalPadding.Right, timePicker.Margin.Bottom);
        //}
        //else
        //{
        //    timePicker.Margin = new Thickness(HorizontalPadding.Left, timePicker.Margin.Top, HorizontalPadding.Right, timePicker.Margin.Bottom);
        //}

        //if (trailingIcon.IsVisible)
        //{
        //    var entryPaddingLeft = leadingIcon.IsVisible ? 12 : HorizontalPadding;
        //    trailingIcon.Margin = new Thickness(12, iconVerticalMargin, HorizontalPadding.Right, iconVerticalMargin);
        //    timePicker.Margin = new Thickness(entryPaddingLeft.Left, timePicker.Margin.Top, 0, timePicker.Margin.Bottom);
        //}

        //helper.Margin = new Thickness(HorizontalPadding.Left, helper.Margin.Top, 12, 0);
        //counter.Margin = new Thickness(0, counter.Margin.Top, HorizontalPadding.Right, 0);

        //var placeholderLeftMargin = FloatingPlaceholderEnabled ? HorizontalPadding.Left : timePicker.Margin.Left;
        //placeholder.Margin = new Thickness(placeholderLeftMargin, 0, 0, 0);

        //if (HasError)
        //{
        //    underline.Color = ErrorColor;
        //}
        //}

        private void TimePicker_TimeChanged(object sender, NullableTimeChangedEventArgs e)
        {
            Time = timePicker.NullableTime;
            TimeChangeCommand?.Execute(timePicker.NullableTime);
            TimeChanged?.Invoke(this, e);
        }

        private void OnEnabledChanged(bool isEnabled)
        {
            Opacity = isEnabled ? 1 : 0.33;
            OnPropertyChanged(nameof(IsHelperVisible));
        }

        private void OnErrorTextChanged()
        {
            OnPropertyChanged(nameof(SmallText));
        }

        private void OnFloatingPlaceholderEnabledChanged()
        {
            _autoSizingRow.Height = FloatingPlaceholderEnabled ? new GridLength(54) : GridLength.Auto;
            UpdateState(false);

            //    double marginTopVariation = Device.RuntimePlatform == Device.iOS ? 18 : 20;
            //    timePicker.Margin = isEnabled ? new Thickness(timePicker.Margin.Left, 24, timePicker.Margin.Right, 0) : new Thickness(timePicker.Margin.Left, marginTopVariation - 9, timePicker.Margin.Right, 0);

            //    var iconMargin = leadingIcon.Margin;
            //    leadingIcon.Margin = isEnabled ? new Thickness(iconMargin.Left, 16, iconMargin.Right, 16) : new Thickness(iconMargin.Left, 8, iconMargin.Right, 8);

            //    var trailingIconMargin = trailingIcon.Margin;
            //    trailingIcon.Margin = isEnabled ? new Thickness(trailingIconMargin.Left, 16, trailingIconMargin.Right, 16) : new Thickness(trailingIconMargin.Left, 8, trailingIconMargin.Right, 8);
        }

        private void OnHelperTextChanged()
        {
            OnPropertyChanged(nameof(SmallText));
            OnPropertyChanged(nameof(IsHelperVisible));
        }

        private void OnTimeChanged(TimeSpan? time)
        {
            timePicker.NullableTime = time;
            clearIcon.IsVisible = time.HasValue;
            Device.BeginInvokeOnMainThread(() => UpdateState());
        }

        private void OnClear(object sender, EventArgs e)
        {
            if (Time != null)
                Time = null;
        }

        private void OnTap(object sender, EventArgs e)
        {
            if (!timePicker.IsFocused)
                timePicker.Focus();
        }
    }
}
