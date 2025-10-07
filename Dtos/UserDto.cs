namespace IAMLegend.Dtos
{

    public class UserDto
    {
        public int UserProfileId { get; set; }
        public string Domain { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool IsAdmin { get; set; }
    }

}
