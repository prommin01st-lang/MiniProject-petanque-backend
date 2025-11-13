using System;

namespace JWTdemo.Models
{
    // นี่คือข้อมูล User ที่ปลอดภัย ที่จะส่งไปให้ Form (Autocomplete)
    public class UserOptionDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
    }
}