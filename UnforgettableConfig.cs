using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Unforgettable
{
    public class UnforgettableConfig
    {
        [Category("HUD layout")]
        [DisplayName("Left margin")]
        [Description("Distance in pixels from the left edge of the screen.")]
        [DefaultValue(20f)]
        [Range(0f, 800f)]
        public float HudMarginLeft { get; set; } = 20f;

        [Category("HUD layout")]
        [DisplayName("Top margin")]
        [Description("Distance in pixels from the top edge of the screen.")]
        [DefaultValue(20f)]
        [Range(0f, 800f)]
        public float HudMarginTop { get; set; } = 20f;

        [Category("HUD layout")]
        [DisplayName("Icon size")]
        [Description("Width and height of each timer icon in pixels.")]
        [DefaultValue(80f)]
        [Range(32f, 256f)]
        public float HudIconSize { get; set; } = 80f;

        [Category("HUD layout")]
        [DisplayName("Gap between icons")]
        [Description("Horizontal spacing between icons from the same station.")]
        [DefaultValue(10f)]
        [Range(0f, 64f)]
        public float HudIconGap { get; set; } = 10f;

        [Category("HUD layout")]
        [DisplayName("Gap between stations")]
        [Description("Vertical spacing between oven, cooking pot, and crucible icon rows.")]
        [DefaultValue(10f)]
        [Range(0f, 64f)]
        public float HudBandGap { get; set; } = 10f;

        [Category("Clay oven")]
        [DisplayName("Repeat alarm sound")]
        [Description("When enabled, the oven alarm keeps playing until the item is removed.")]
        [DefaultValue(true)]
        public bool OvenRepeatAlarm { get; set; } = true;

        [Category("Clay oven")]
        [DisplayName("Show icon when done")]
        [Description("When enabled, the oven icon keeps blinking after baking finishes.")]
        [DefaultValue(true)]
        public bool OvenShowIconWhenDone { get; set; } = true;

        [Category("Clay oven")]
        [DisplayName("Alarm interval (seconds)")]
        [Description("Delay between repeated oven alarm sounds.")]
        [DefaultValue(5)]
        [Range(1, 60)]
        public int OvenAlarmIntervalSeconds { get; set; } = 5;

        [Category("Cooking pot")]
        [DisplayName("Repeat alarm sound")]
        [Description("When enabled, the cooking pot alarm keeps playing until the meal is removed.")]
        [DefaultValue(false)]
        public bool PotRepeatAlarm { get; set; } = false;

        [Category("Cooking pot")]
        [DisplayName("Show icon when done")]
        [Description("When enabled, the cooking pot icon keeps blinking after cooking finishes.")]
        [DefaultValue(false)]
        public bool PotShowIconWhenDone { get; set; } = false;

        [Category("Cooking pot")]
        [DisplayName("Alarm interval (seconds)")]
        [Description("Delay between repeated cooking pot alarm sounds.")]
        [DefaultValue(5)]
        [Range(1, 60)]
        public int PotAlarmIntervalSeconds { get; set; } = 5;

        [Category("Crucible")]
        [DisplayName("Repeat alarm sound")]
        [Description("When enabled, the crucible alarm keeps playing until the ingot is removed.")]
        [DefaultValue(true)]
        public bool CrucibleRepeatAlarm { get; set; } = true;

        [Category("Crucible")]
        [DisplayName("Show icon when done")]
        [Description("When enabled, the crucible icon keeps blinking after smelting finishes.")]
        [DefaultValue(true)]
        public bool CrucibleShowIconWhenDone { get; set; } = true;

        [Category("Crucible")]
        [DisplayName("Alarm interval (seconds)")]
        [Description("Delay between repeated crucible alarm sounds.")]
        [DefaultValue(5)]
        [Range(1, 60)]
        public int CrucibleAlarmIntervalSeconds { get; set; } = 5;
    }
}
