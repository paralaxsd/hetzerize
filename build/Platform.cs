using Nuke.Common.Tooling;
using System.ComponentModel;

[TypeConverter(typeof(TypeConverter<Platform>))]
public class Platform : Enumeration
{
    public static Platform Windows = new() { Value = "win" };
    public static Platform Linux = new() { Value = "linux" };

    public static implicit operator string(Platform platform) => platform.Value;
}