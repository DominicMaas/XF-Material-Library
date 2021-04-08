using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XF.Material.Droid.Renderers.Internals;
using XF.Material.Forms.UI.Internals;

[assembly: ExportRenderer(typeof(MaterialTimePicker), typeof(MaterialTimePickerRenderer))]
namespace XF.Material.Droid.Renderers.Internals
{
    internal class MaterialTimePickerRenderer : TimePickerRenderer
    {
        private MaterialTimePicker _materialTimePicker;

        public MaterialTimePickerRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.TimePicker> e)
        {
            base.OnElementChanged(e);

            if (e?.NewElement != null)
            {
                _materialTimePicker = Element as MaterialTimePicker;
                SetControl();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
        }

        private void SetControl()
        {
            if (Control == null)
            {
                return;
            }

            Control.Background = new ColorDrawable(Color.Transparent.ToAndroid());
            Control.SetPadding(0, 0, 0, 0);
            Control.SetIncludeFontPadding(false);
            Control.SetMinimumHeight((int)MaterialHelper.ConvertDpToPx(20));

            var layoutParams = new MarginLayoutParams(Control.LayoutParameters);
            layoutParams.SetMargins(0, 0, 0, 0);
            Control.LayoutParameters = layoutParams;
        }
    }
}
