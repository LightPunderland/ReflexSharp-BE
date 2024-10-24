namespace Features.User.DTOs;

// enum
public enum Rank{
    Noob,
    Pro,
    Master,
    God,
    Admin,
    None
}

//record
public record UserDTO{
    public Guid Id {get; init;}
    public string Email {get; init;} = null;
    public string DisplayName {get; init; } = null!;
    public Rank PublicRank { get; init; } = Rank.None;
}
