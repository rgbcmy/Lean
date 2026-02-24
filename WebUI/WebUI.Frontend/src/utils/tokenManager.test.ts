/**
 * Token Manager Utility Tests
 * Token 管理工具测试
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { decodeJWT, isTokenExpired } from '@/utils/tokenManager';

describe('Token Manager Utilities', () => {
  describe('decodeJWT', () => {
    it('should decode a valid JWT token', () => {
      // This is a sample JWT with payload: {"sub":"1234567890","name":"John Doe","exp":9999999999}
      const token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZXhwIjo5OTk5OTk5OTk5fQ.PGS_oP1bOT7GMGX-keFPVPxq_AFLXb7dLPZZ3qjj1jk';

      const decoded = decodeJWT(token);

      expect(decoded).toBeDefined();
      expect(decoded.sub).toBe('1234567890');
      expect(decoded.name).toBe('John Doe');
      expect(decoded.exp).toBe(9999999999);
    });

    it('should return null for invalid token', () => {
      const invalidToken = 'invalid.token.here';

      const decoded = decodeJWT(invalidToken);

      expect(decoded).toBeNull();
    });

    it('should return null for malformed token', () => {
      const malformedToken = 'not-a-jwt';

      const decoded = decodeJWT(malformedToken);

      expect(decoded).toBeNull();
    });

    it('should handle tokens with special characters', () => {
      // Token with URL-safe base64 characters (- and _)
      const token = 'eyJhbGciOiJIUzI1NiJ9.eyJ1c2VySWQiOiJ0ZXN0LXVzZXItMTIzIn0.xyz';

      const decoded = decodeJWT(token);

      expect(decoded).toBeDefined();
      expect(decoded.userId).toBe('test-user-123');
    });
  });

  describe('isTokenExpired', () => {
    beforeEach(() => {
      // Mock Date.now() to return a consistent value
      vi.spyOn(Date, 'now').mockReturnValue(1700000000000); // Nov 15, 2023
    });

    it('should return false for token that is not expired', () => {
      // Token expires in the future (timestamp: 2000000000 = May 18, 2033)
      const token = 'eyJhbGciOiJIUzI1NiJ9.eyJleHAiOjIwMDAwMDAwMDB9.xyz';

      const expired = isTokenExpired(token);

      expect(expired).toBe(false);
    });

    it('should return true for expired token', () => {
      // Token expired in the past (timestamp: 1600000000 = Sep 13, 2020)
      const token = 'eyJhbGciOiJIUzI1NiJ9.eyJleHAiOjE2MDAwMDAwMDB9.xyz';

      const expired = isTokenExpired(token);

      expect(expired).toBe(true);
    });

    it('should return true for token without exp claim', () => {
      const token = 'eyJhbGciOiJIUzI1NiJ9.eyJ1c2VySWQiOiIxMjMifQ.xyz';

      const expired = isTokenExpired(token);

      expect(expired).toBe(true);
    });

    it('should return true for invalid token', () => {
      const invalidToken = 'invalid.token';

      const expired = isTokenExpired(invalidToken);

      expect(expired).toBe(true);
    });

    it('should consider token expired 5 minutes before actual expiration', () => {
      // Current time: 1700000000000 (Nov 15, 2023)
      // Token expires at: 1700000300 seconds = 1700000300000 ms (5 minutes later)
      // With 5-minute buffer, should be considered expired
      const token = 'eyJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MDAwMDAzMDB9.xyz';

      const expired = isTokenExpired(token);

      expect(expired).toBe(true);
    });

    it('should not be expired if more than 5 minutes remain', () => {
      // Current time: 1700000000000 ms
      // Token expires at: 1700000400 seconds = 1700000400000 ms (6.67 minutes later)
      // With 5-minute buffer, should NOT be expired
      const token = 'eyJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MDAwMDA0MDB9.xyz';

      const expired = isTokenExpired(token);

      expect(expired).toBe(false);
    });
  });
});
