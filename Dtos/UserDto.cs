using System;

namespace WebApi.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string MobilePhone { get; set; }
        public string UserStatus { get; set; }
        public string Photo { get; set; }
        public DateTime Registered { get; set; }
        public string Password { get; set; }
    }
}