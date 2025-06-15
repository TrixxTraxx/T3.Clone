using LLMLab.Dtos.User;
using LLMLab.Server.Data;
using Riok.Mapperly.Abstractions;

namespace LLMLab.Server.Mappers;

[Mapper]
public partial class UserMapper
{
    public static partial UserDto Map(ApplicationUser user);
}