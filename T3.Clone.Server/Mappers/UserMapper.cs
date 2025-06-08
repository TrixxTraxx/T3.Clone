using Riok.Mapperly.Abstractions;
using T3.Clone.Dtos.User;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Mappers;

[Mapper]
public partial class UserMapper
{
    public static partial UserDto Map(ApplicationUser user);
}