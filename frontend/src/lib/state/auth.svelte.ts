// JWT decoding helper
function parseJwt(token: string) {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      window.atob(base64)
        .split('')
        .map(function(c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        })
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch (e) {
    return null;
  }
}

export type UserRole = 'Candidate' | 'Recruiter' | 'Administrator';

export interface User {
  id: string;
  email: string;
  username: string;
  roles: UserRole[];
}

class AuthState {
  token = $state<string | null>(null);
  user = $state<User | null>(null);

  // Derived state for easy role checking
  isCandidate = $derived(this.user?.roles.includes('Candidate') ?? false);
  isRecruiter = $derived(this.user?.roles.includes('Recruiter') ?? false);
  isAdmin = $derived(this.user?.roles.includes('Administrator') ?? false);
  isAuthenticated = $derived(this.token !== null && this.user !== null);

  constructor() {
    // Only initialize from localStorage in the browser
    if (typeof window !== 'undefined') {
      const storedToken = localStorage.getItem('jwt_token');
      if (storedToken) {
        this.setToken(storedToken);
      }
    }
  }

  setToken(newToken: string) {
    this.token = newToken;
    if (typeof window !== 'undefined') {
      localStorage.setItem('jwt_token', newToken);
    }
    
    // Parse JWT to extract user info
    const payload = parseJwt(newToken);
    if (payload) {
      // Handle the fact that roles can be a single string or an array in JWT
      const rolesClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      let roles: UserRole[] = [];
      if (Array.isArray(rolesClaim)) {
        roles = rolesClaim as UserRole[];
      } else if (typeof rolesClaim === 'string') {
        roles = [rolesClaim as UserRole];
      }

      this.user = {
        id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
        username: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
        roles: roles
      };
    } else {
      this.user = null;
    }
  }

  logout() {
    this.token = null;
    this.user = null;
    if (typeof window !== 'undefined') {
      localStorage.removeItem('jwt_token');
      // In a real app we would also call the backend /api/auth/logout endpoint 
      // to invalidate the refresh token cookie.
    }
  }
}

// Export a singleton instance
export const authState = new AuthState();
