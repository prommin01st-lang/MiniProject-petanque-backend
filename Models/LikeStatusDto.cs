namespace JWTdemo.Models
{
    // DTO นี้จะบอก Frontend ว่าบทความนี้มีกี่ Like
    // และ User คนปัจจุบัน Like หรือยัง
    public class LikeStatusDto
    {
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }
}