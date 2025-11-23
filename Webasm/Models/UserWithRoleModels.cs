namespace UsersApp.ViewModels
{
    public class UserWithRoleModels
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string ProfilePictureUrl { get; set; }

        public bool IsBlocked { get; set; }
    }
}
