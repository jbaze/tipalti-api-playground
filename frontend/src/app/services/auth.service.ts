import { Injectable, signal } from '@angular/core';

/**
 * Authentication service with mock JWT token generation
 *
 * Purpose:
 * - Simulates user login flow without real backend authentication
 * - Generates realistic-looking JWT tokens for demonstration
 * - Manages authentication state using Angular signals
 * - Provides token to HTTP interceptor for Authorization headers
 *
 * Note: This is a MOCK implementation for playground purposes.
 * In production, replace with real JWT authentication (e.g., Auth0, Keycloak, custom JWT API)
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  /**
   * Hard-coded admin credentials for demo
   * In production: remove and use real authentication
   */
  private readonly ADMIN_EMAIL = 'administrator@localhost';
  private readonly ADMIN_PASSWORD = 'Administrator1!';

  /**
   * Current authentication token (signal for reactivity)
   * Null when user is not logged in
   */
  public token = signal<string | null>(null);

  /**
   * Current user email (signal for reactivity)
   * Null when user is not logged in
   */
  public userEmail = signal<string | null>(null);

  /**
   * Computed signal: whether user is currently logged in
   */
  public isLoggedIn = signal<boolean>(false);

  constructor() {
    // Check if token exists in sessionStorage on service initialization
    const savedToken = sessionStorage.getItem('authToken');
    const savedEmail = sessionStorage.getItem('userEmail');

    if (savedToken && savedEmail) {
      this.token.set(savedToken);
      this.userEmail.set(savedEmail);
      this.isLoggedIn.set(true);
    }
  }

  /**
   * Attempts to log in with provided credentials
   *
   * @param email - User email
   * @param password - User password
   * @returns Object with success status, optional token, and optional error message
   */
  login(email: string, password: string): { success: boolean; token?: string; error?: string } {
    // Validate credentials against hardcoded admin account
    if (email === this.ADMIN_EMAIL && password === this.ADMIN_PASSWORD) {
      // Generate a realistic-looking JWT token
      const token = this.generateMockJWT(email);

      // Update state
      this.token.set(token);
      this.userEmail.set(email);
      this.isLoggedIn.set(true);

      // Persist to sessionStorage (NOT localStorage for security)
      sessionStorage.setItem('authToken', token);
      sessionStorage.setItem('userEmail', email);

      return { success: true, token };
    }

    // Invalid credentials
    return {
      success: false,
      error: 'Invalid email or password. Try: administrator@localhost / Administrator1!'
    };
  }

  /**
   * Logs out the current user
   * Clears token and session storage
   */
  logout(): void {
    this.token.set(null);
    this.userEmail.set(null);
    this.isLoggedIn.set(false);

    sessionStorage.removeItem('authToken');
    sessionStorage.removeItem('userEmail');
  }

  /**
   * Generates a mock JWT token that looks realistic
   *
   * Structure: header.payload.signature (Base64-encoded JSON objects)
   *
   * Real JWT tokens have:
   * - Header: { "alg": "HS256", "typ": "JWT" }
   * - Payload: { "sub": "user@example.com", "name": "User", "exp": 1234567890 }
   * - Signature: HMACSHA256(base64(header) + "." + base64(payload), secret)
   *
   * This mock version:
   * - Generates valid Base64 header and payload
   * - Uses a fake signature (not cryptographically signed)
   * - Can be decoded at jwt.io for demo purposes
   *
   * @param email - User email to include in token payload
   * @returns Mock JWT token string
   */
  private generateMockJWT(email: string): string {
    // JWT Header
    const header = {
      alg: 'HS256',
      typ: 'JWT'
    };

    // JWT Payload
    const payload = {
      sub: email,
      name: 'Admin User',
      email: email,
      role: 'administrator',
      iat: Math.floor(Date.now() / 1000), // Issued at (current time)
      exp: Math.floor(Date.now() / 1000) + (60 * 60) // Expires in 1 hour
    };

    // Encode to Base64 (URL-safe)
    const encodedHeader = this.base64UrlEncode(JSON.stringify(header));
    const encodedPayload = this.base64UrlEncode(JSON.stringify(payload));

    // Generate a fake signature (in real JWT, this would be HMAC signature)
    // For demo purposes, we just use a random string
    const fakeSignature = this.generateRandomBase64(43); // JWT signatures are typically 43 chars

    // Combine: header.payload.signature
    return `${encodedHeader}.${encodedPayload}.${fakeSignature}`;
  }

  /**
   * Base64 URL-safe encoding
   * Converts JSON object to Base64, replacing + with -, / with _, and removing =
   */
  private base64UrlEncode(str: string): string {
    const base64 = btoa(str);
    return base64
      .replace(/\+/g, '-')
      .replace(/\//g, '_')
      .replace(/=/g, '');
  }

  /**
   * Generates a random Base64 URL-safe string
   * Used for the fake JWT signature
   */
  private generateRandomBase64(length: number): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_';
    let result = '';
    for (let i = 0; i < length; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
  }

  /**
   * Decodes the JWT payload for display purposes
   * (Does NOT validate the signature - this is mock auth!)
   */
  decodeToken(token: string): any {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) {
        return null;
      }

      // Decode the payload (middle part)
      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }
}
