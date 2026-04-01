using Core.Models.Enums;

namespace Core.Extentions;

public static class ExtraFlagExtensions
{
    public static ExtraFlag Add(this ExtraFlag flags, ExtraFlag flag)
    {
        return flags | flag;
    }
    public static ExtraFlag Remove(this ExtraFlag flags, ExtraFlag flag)
    {
        return flags & ~flag;
    }
}