using System;

namespace XF.Material.Forms.UI.Internals
{
    public class NullableTimeChangedEventArgs : EventArgs
    {
        public NullableTimeChangedEventArgs(TimeSpan? oldTime, TimeSpan? newTime)
        {
            OldTime = oldTime;
            NewTime = newTime;
        }

        public TimeSpan? OldTime { get; set; }

        public TimeSpan? NewTime { get; set; }
    }
}
