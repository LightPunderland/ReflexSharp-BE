namespace Features.User.Exceptions
{
    public class InsufficientXPException : System.Exception
    {
        public Guid UserId { get; set; }
        public int CurrentXP { get; set; }
        public int RequiredXP { get; set; }

        public InsufficientXPException(Guid UserId, int CurrentXP, int RequiredXP)
                : base($"User {UserId} has {CurrentXP} XP, but needs {RequiredXP} XP to level up.")
        {
            this.UserId = UserId;
            this.CurrentXP = CurrentXP;
            this.RequiredXP = RequiredXP;
        }
    }

    public class UserNotFoundException : System.Exception
    {
        public Guid UserId { get; }

        public UserNotFoundException(Guid UserId) : base($"User with ID {UserId} is not a valid user in our database.")
        {
            this.UserId = UserId;
        }
    }
}
