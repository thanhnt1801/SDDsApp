namespace UserService.Interfaces
{
    public interface IUserServices
    {
        public string CreateRandomToken(string email, string memberRole);
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    }
}
