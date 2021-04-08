using System;
using Xamarin.Forms;

namespace XF.Material.Forms.UI.Internals
{
    /// <inheritdoc />
    /// <summary>
    /// Used for rendering the <see cref="T:Xamarin.Forms.Entry" /> control in <see cref="T:XF.Material.Forms.UI.MaterialTextField" />.
    /// </summary>
    public class MaterialTimePicker : TimePicker
    {
        public static readonly BindableProperty TintColorProperty = BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(MaterialDatePicker), Material.Color.Secondary);
        public static readonly BindableProperty NullableTimeProperty = BindableProperty.Create(nameof(NullableTime), typeof(TimeSpan?), typeof(MaterialDatePicker));
        public static readonly BindableProperty IgnoreCancelProperty = BindableProperty.Create(nameof(IgnoreCancel), typeof(bool), typeof(MaterialDatePicker), true);

        private Color? _color;
        public new EventHandler<NullableTimeChangedEventArgs> TimeSelected;
        
        /// <summary>
        /// Public constructor required for xamarin hot reload
        /// </summary>
        public MaterialTimePicker()
        {
            // No TimeChanged event on the TimePicker :(
            base.PropertyChanged += (sender, args) =>
            {
                if (!IgnoreCancel && args.PropertyName == TimeProperty.PropertyName)
                    TriggerTimeSelected(Time);
            };
        }
        
        public Color TintColor
        {
            get => (Color)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }

        public bool IgnoreCancel
        {
            get => (bool)GetValue(IgnoreCancelProperty);
            set => SetValue(IgnoreCancelProperty, value);
        }
        
        public TimeSpan? NullableTime
        {
            get => (TimeSpan?)GetValue(NullableTimeProperty);

            set
            {
                if (NullableTime != value)
                {
                    SetValue(NullableTimeProperty, value);
                    UpdateTime();
                }
            }
        }
        
        private void UpdateTime()
        {
            if (NullableTime.HasValue)
            {
                if (_color.HasValue)
                {
                    TextColor = _color.Value;
                    _color = null;
                }

                Time = NullableTime.Value;
            }
            else
            {
                _color = TextColor;
                TextColor = Color.Transparent;
            }
        }
        
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            UpdateTime();
        }
        
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(IsFocused) && !IsFocused && IgnoreCancel)
            {
                //Called even if cancel is selected. Except if IgnoreCancel is false.
                TriggerTimeSelected(Time);
            }
        }
        
        private void TriggerTimeSelected(TimeSpan time)
        {
            if (time != NullableTime)
            {
                var old = NullableTime;
                NullableTime = time;
                TimeSelected?.Invoke(this, new NullableTimeChangedEventArgs(old, time));
            }
        }
    }
}
