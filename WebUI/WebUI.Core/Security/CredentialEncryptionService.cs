/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace WebUI.Core.Security
{
    /// <summary>
    /// Interface for credential encryption service
    /// </summary>
    public interface ICredentialEncryptionService
    {
        /// <summary>
        /// Encrypts sensitive data
        /// </summary>
        /// <param name="plaintext">Plain text to encrypt</param>
        /// <returns>Encrypted data as base64 string</returns>
        string Encrypt(string plaintext);

        /// <summary>
        /// Decrypts encrypted data
        /// </summary>
        /// <param name="ciphertext">Base64 encrypted string</param>
        /// <returns>Decrypted plain text</returns>
        string Decrypt(string ciphertext);
    }

    /// <summary>
    /// Credential encryption service using DPAPI on Windows and AES-GCM on Linux/macOS
    /// </summary>
    public class CredentialEncryptionService : ICredentialEncryptionService
    {
        private readonly ILogger<CredentialEncryptionService> _logger;
        private readonly byte[]? _encryptionKey;

        public CredentialEncryptionService(ILogger<CredentialEncryptionService> logger)
        {
            _logger = logger;
            
            // For Linux/macOS, derive a machine-specific key
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _encryptionKey = DeriveEncryptionKey();
            }
        }

        /// <summary>
        /// Encrypts plaintext using platform-specific encryption
        /// </summary>
        public string Encrypt(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext))
            {
                throw new ArgumentException("Plaintext cannot be null or empty", nameof(plaintext));
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return EncryptWithDPAPI(plaintext);
                }
                else
                {
                    return EncryptWithAesGcm(plaintext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt credentials");
                throw new CryptographicException("Failed to encrypt credentials", ex);
            }
        }

        /// <summary>
        /// Decrypts ciphertext using platform-specific decryption
        /// </summary>
        public string Decrypt(string ciphertext)
        {
            if (string.IsNullOrEmpty(ciphertext))
            {
                throw new ArgumentException("Ciphertext cannot be null or empty", nameof(ciphertext));
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return DecryptWithDPAPI(ciphertext);
                }
                else
                {
                    return DecryptWithAesGcm(ciphertext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt credentials");
                throw new CryptographicException("Failed to decrypt credentials", ex);
            }
        }

        /// <summary>
        /// Encrypts using Windows DPAPI (Data Protection API)
        /// </summary>
        private string EncryptWithDPAPI(string plaintext)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("DPAPI is only supported on Windows");
            }

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            
            // Use ProtectedData class (Windows-only)
            // Note: This requires System.Security.Cryptography.ProtectedData NuGet package
            var encryptedBytes = System.Security.Cryptography.ProtectedData.Protect(
                plaintextBytes,
                optionalEntropy: null,
                scope: System.Security.Cryptography.DataProtectionScope.CurrentUser
            );
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Decrypts using Windows DPAPI
        /// </summary>
        private string DecryptWithDPAPI(string ciphertext)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("DPAPI is only supported on Windows");
            }

            var encryptedBytes = Convert.FromBase64String(ciphertext);
            var plaintextBytes = System.Security.Cryptography.ProtectedData.Unprotect(
                encryptedBytes,
                optionalEntropy: null,
                scope: System.Security.Cryptography.DataProtectionScope.CurrentUser
            );
            return Encoding.UTF8.GetString(plaintextBytes);
        }

        /// <summary>
        /// Encrypts using AES-256-GCM for Linux/macOS
        /// </summary>
        private string EncryptWithAesGcm(string plaintext)
        {
            if (_encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key not initialized");
            }

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            
            // Generate random nonce (12 bytes recommended for GCM)
            var nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce);
            
            // Generate authentication tag
            var tag = new byte[16];
            var ciphertext = new byte[plaintextBytes.Length];
            
            using (var aesGcm = new AesGcm(_encryptionKey, 16))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            }
            
            // Combine nonce + tag + ciphertext
            var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);
            
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypts using AES-256-GCM for Linux/macOS
        /// </summary>
        private string DecryptWithAesGcm(string ciphertext)
        {
            if (_encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key not initialized");
            }

            var encryptedData = Convert.FromBase64String(ciphertext);
            
            // Extract nonce, tag, and ciphertext
            var nonce = new byte[12];
            var tag = new byte[16];
            var encrypted = new byte[encryptedData.Length - nonce.Length - tag.Length];
            
            Buffer.BlockCopy(encryptedData, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(encryptedData, nonce.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(encryptedData, nonce.Length + tag.Length, encrypted, 0, encrypted.Length);
            
            var plaintext = new byte[encrypted.Length];
            
            using (var aesGcm = new AesGcm(_encryptionKey, 16))
            {
                aesGcm.Decrypt(nonce, encrypted, tag, plaintext);
            }
            
            return Encoding.UTF8.GetString(plaintext);
        }

        /// <summary>
        /// Derives a machine-specific encryption key for Linux/macOS
        /// </summary>
        private byte[] DeriveEncryptionKey()
        {
            // Combine machine-specific identifiers
            var machineName = Environment.MachineName;
            var userName = Environment.UserName;
            var osVersion = Environment.OSVersion.ToString();
            
            var combinedSeed = $"{machineName}|{userName}|{osVersion}|WebUI-IBKR-Encryption-Salt-v1";
            var seedBytes = Encoding.UTF8.GetBytes(combinedSeed);
            var saltBytes = Encoding.UTF8.GetBytes("WebUI-IBKR-Salt-2024");
            
            // Use PBKDF2 to derive a 256-bit key
            return Rfc2898DeriveBytes.Pbkdf2(
                seedBytes,
                saltBytes,
                iterations: 100000,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: 32); // 256 bits = 32 bytes
        }
    }
}
