using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

/*
The MIT License (MIT)
Copyright (c) 2019 GitHub
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


namespace GitHubActionsMessageDecrypt;

class Program
{
    private static RSAParameters LoadParameters(String _keyFile)
    {   
        string encryptedBytes = File.ReadAllText(_keyFile);
        return JsonConvert.DeserializeObject<RSAParameters>(encryptedBytes);
    }

    static void Main(string[] args)
    {
        if(args.Length < 4){
            Console.Error.WriteLine("Please run with: dotnet run <rsaparams path> <encryption key> <iv> <file with base64ed encrypted blob>");
            return;
        }

        byte[] _encryptionKey = Convert.FromBase64String(args[1]);
        byte[] _iv = Convert.FromBase64String(args[2]);
          
        Console.Error.WriteLine("Loading RSA key parameters from file {0}", args[0]);
	var rsa = RSA.Create();
        rsa.ImportParameters(LoadParameters(args[0]));
        Console.Error.WriteLine("[+] RSA loaded");

        Aes aes = Aes.Create();
        ICryptoTransform decryptor = aes.CreateDecryptor(rsa.Decrypt(_encryptionKey, RSAEncryptionPadding.OaepSHA256), _iv);
        Console.Error.WriteLine("[+] Key/IV loaded");

        Console.Error.WriteLine("[+] Reading ciphertext from {0}", args[3]);
        string encryptedBytes = File.ReadAllText(args[3]);

        using MemoryStream body = new MemoryStream(Convert.FromBase64String(encryptedBytes));
        using CryptoStream cryptoStream = new CryptoStream(body, decryptor, CryptoStreamMode.Read);
        using StreamReader bodyReader = new StreamReader(cryptoStream, Encoding.UTF8);
        Console.WriteLine(bodyReader.ReadToEnd());

        Console.Error.WriteLine("[+] Done");
    }
}
