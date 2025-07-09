namespace Inputs.Inputs.Data
{
    public enum EToggleInput : byte
    {
        None = 0,
        
        LeftSwipe = 0b0000_0001,
        RightSwipe = 0b0000_1000,
        UpSwipe = 0b0000_0100,
        DownSwipe = 0b0000_0010,
        
        Sprint = 0b0100_0000,
        Normal = 0b0010_0000,
        Hide = 0b0001_0000,
    }
}