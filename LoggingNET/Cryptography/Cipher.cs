using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LoggingNET.Cryptography
{
  public class Cipher
  {
    public static void Encrypt(string content, string outputFile)
    {
      if(string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
      {
        throw new ArgumentNullException(nameof(content));
      }

      if(string.IsNullOrEmpty(outputFile) || string.IsNullOrWhiteSpace(outputFile))
      {
        throw new ArgumentNullException(nameof(outputFile));
      }

      using(var stream = new FileStream(outputFile, FileMode.OpenOrCreate))
      {
        var encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(content), null, DataProtectionScope.CurrentUser);

        stream.Write(encryptedBytes, 0, encryptedBytes.Length);
      }
    }

    public static string Decrypt(string inputFile)
    {
      try
      {
        if(string.IsNullOrEmpty(inputFile) || string.IsNullOrWhiteSpace(inputFile))
        {
          throw new ArgumentNullException(nameof(inputFile));
        }

        if(!File.Exists(inputFile))
        {
          return string.Empty;
        }

        byte[] outputBytes = ProtectedData.Unprotect(File.ReadAllBytes(inputFile), null, DataProtectionScope.CurrentUser);

        return Encoding.UTF8.GetString(outputBytes);
      } 
      catch(Exception ex)
      {
#if DEBUG
        Console.WriteLine(ex.Message);
#endif
        return File.ReadAllText(inputFile);
      }                                            
    }
  }
}
