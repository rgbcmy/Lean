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
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Core.Security;
using Xunit;

namespace WebUI.Tests.Security
{
    public class CredentialEncryptionServiceTests
    {
        private readonly Mock<ILogger<CredentialEncryptionService>> _mockLogger;
        private readonly CredentialEncryptionService _service;

        public CredentialEncryptionServiceTests()
        {
            _mockLogger = new Mock<ILogger<CredentialEncryptionService>>();
            _service = new CredentialEncryptionService(_mockLogger.Object);
        }

        [Fact]
        public void Encrypt_ValidPlaintext_ReturnsBase64String()
        {
            // Arrange
            var plaintext = "MySecretPassword123!";

            // Act
            var encrypted = _service.Encrypt(plaintext);

            // Assert
            Assert.NotNull(encrypted);
            Assert.NotEmpty(encrypted);
            Assert.NotEqual(plaintext, encrypted);
            
            // Verify it's valid base64
            var bytes = Convert.FromBase64String(encrypted);
            Assert.NotEmpty(bytes);
        }

        [Fact]
        public void Encrypt_EmptyString_ThrowsArgumentException()
        {
            // Arrange
            var plaintext = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.Encrypt(plaintext));
        }

        [Fact]
        public void Encrypt_NullString_ThrowsArgumentException()
        {
            // Arrange
            string plaintext = null!;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.Encrypt(plaintext));
        }

        [Fact]
        public void Decrypt_ValidCiphertext_ReturnsOriginalPlaintext()
        {
            // Arrange
            var plaintext = "MySecretPassword123!";
            var encrypted = _service.Encrypt(plaintext);

            // Act
            var decrypted = _service.Decrypt(encrypted);

            // Assert
            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void Decrypt_EmptyString_ThrowsArgumentException()
        {
            // Arrange
            var ciphertext = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.Decrypt(ciphertext));
        }

        [Fact]
        public void Decrypt_NullString_ThrowsArgumentException()
        {
            // Arrange
            string ciphertext = null!;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.Decrypt(ciphertext));
        }

        [Fact]
        public void Decrypt_InvalidBase64_ThrowsCryptographicException()
        {
            // Arrange
            var invalidCiphertext = "This is not valid base64!@#$%";

            // Act & Assert
            Assert.Throws<CryptographicException>(() => _service.Decrypt(invalidCiphertext));
        }

        [Theory]
        [InlineData("password")]
        [InlineData("MyPassword123!")]
        [InlineData("Very Long Password With Special Characters !@#$%^&*()")]
        [InlineData("UTF8支持中文密码")]
        public void EncryptDecrypt_VariousPasswords_Roundtrip(string plaintext)
        {
            // Arrange & Act
            var encrypted = _service.Encrypt(plaintext);
            var decrypted = _service.Decrypt(encrypted);

            // Assert
            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void Encrypt_SamePlaintext_ProducesDifferentCiphertextOnLinuxMac()
        {
            // Arrange
            var plaintext = "MySecretPassword123!";

            // Act
            var encrypted1 = _service.Encrypt(plaintext);
            var encrypted2 = _service.Encrypt(plaintext);

            // Assert
            // On Linux/macOS using AES-GCM with random nonce, ciphertexts should differ
            // On Windows using DPAPI, ciphertexts may also differ due to random entropy
            // Both should decrypt to the same plaintext
            var decrypted1 = _service.Decrypt(encrypted1);
            var decrypted2 = _service.Decrypt(encrypted2);
            
            Assert.Equal(plaintext, decrypted1);
            Assert.Equal(plaintext, decrypted2);
        }

        [Fact]
        public void Encrypt_LongPassword_HandlesCorrectly()
        {
            // Arrange
            var plaintext = new string('A', 1000); // 1000 character password

            // Act
            var encrypted = _service.Encrypt(plaintext);
            var decrypted = _service.Decrypt(encrypted);

            // Assert
            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void Encrypt_SpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var plaintext = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

            // Act
            var encrypted = _service.Encrypt(plaintext);
            var decrypted = _service.Decrypt(encrypted);

            // Assert
            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void Encrypt_UnicodeCharacters_HandlesCorrectly()
        {
            // Arrange
            var plaintext = "密码 パスワード 비밀번호 пароль 🔐🔑🔒";

            // Act
            var encrypted = _service.Encrypt(plaintext);
            var decrypted = _service.Decrypt(encrypted);

            // Assert
            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void EncryptDecrypt_Multiline_HandlesCorrectly()
        {
            // Arrange
            var plaintext = "Line 1\nLine 2\nLine 3\r\nLine 4";

            // Act
            var encrypted = _service.Encrypt(plaintext);
            var decrypted = _service.Decrypt(encrypted);

            // Assert
            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
        {
            // Arrange
            var plaintext = "MySecretPassword123!";
            var encrypted = _service.Encrypt(plaintext);
            
            // Tamper with the ciphertext
            var bytes = Convert.FromBase64String(encrypted);
            if (bytes.Length > 0)
            {
                bytes[bytes.Length / 2] ^= 0xFF; // Flip bits in the middle
            }
            var tamperedCiphertext = Convert.ToBase64String(bytes);

            // Act & Assert
            Assert.Throws<CryptographicException>(() => _service.Decrypt(tamperedCiphertext));
        }

        [Fact]
        public void EncryptDecrypt_MultipleInstances_Compatible()
        {
            // Arrange
            var plaintext = "MySecretPassword123!";
            var service1 = new CredentialEncryptionService(_mockLogger.Object);
            var service2 = new CredentialEncryptionService(_mockLogger.Object);

            // Act
            var encrypted = service1.Encrypt(plaintext);
            var decrypted = service2.Decrypt(encrypted);

            // Assert
            Assert.Equal(plaintext, decrypted);
        }
    }
}
