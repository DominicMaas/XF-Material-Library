using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XF.Material.Forms.UI.Internals;
using XF.Material.iOS.Renderers;

[assembly: ExportRenderer(typeof(MaterialTimePicker), typeof(MaterialTimePickerRenderer))]

namespace XF.Material.iOS.Renderers
{
    /// <summary>
    /// Remove border
    /// </summary>
    public class MaterialTimePickerRenderer : TimePickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                Control.Layer.BorderWidth = 0;
                Control.BorderStyle = UITextBorderStyle.None;
            }
        }

        protected override UITextField CreateNativeControl()
        {
            var control = base.CreateNativeControl();
            control.BorderStyle = UITextBorderStyle.None;
            return control;
        }
    }
}
