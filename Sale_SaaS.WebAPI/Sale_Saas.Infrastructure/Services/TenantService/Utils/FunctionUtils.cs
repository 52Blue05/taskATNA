using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace Sale_Saas.Infrastructure.Services.TenantService.Utils
{
    public static class FunctionUtils
    {
        public static string GetMd5HashSalt(string input)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(EncryptionKey);
            string sSalt = Convert.ToBase64String(plainTextBytes);
            byte[] salt = Convert.FromBase64String(sSalt);
            Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: input,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashed;
        }

		public static bool VerifyPassword(string input, string storedHash)
		{
			string EncryptionKey = "MAKV2SPBNI99212";
			string storedSalt = Convert.ToBase64String(Encoding.UTF8.GetBytes(EncryptionKey));
			byte[] salt = Convert.FromBase64String(storedSalt);
			string hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: input,
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA512,
				iterationCount: 100000,
				numBytesRequested: 256 / 8));

			return hashedInput == storedHash;
		}
        public static string GenString()
        {
            string newPass = "";
            Random rdn = new Random();
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            for (int i = 0; i < 8; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (newPass.IndexOf(character) != -1);
                newPass += character;
            }
            newPass = newPass.ToLower();
            return newPass;
        }

    }
}
