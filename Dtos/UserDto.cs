using System;

namespace WebApi.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public string MobilePhone { get; set; }
        public string Status { get; set; }
        public DateTime Registered { get; set; }
        public string Password { get; set; }
    }
}