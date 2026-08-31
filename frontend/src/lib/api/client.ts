import { authState } from '../state/auth.svelte';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5246/api';

interface FetchOptions extends RequestInit {
  data?: any;
}

class ApiClient {
  private isRefreshing = false;
  private refreshPromise: Promise<boolean> | null = null;

  async request<T>(endpoint: string, options: FetchOptions = {}): Promise<T> {
    const url = `${API_BASE_URL}${endpoint}`;
    
    // Set up headers
    const headers = new Headers(options.headers);
    if (options.data && !(options.data instanceof FormData)) {
      headers.set('Content-Type', 'application/json');
    }

    if (authState.token) {
      headers.set('Authorization', `Bearer ${authState.token}`);
    }

    const config: RequestInit = {
      ...options,
      headers,
    };

    if (options.data) {
      config.body = options.data instanceof FormData ? options.data : JSON.stringify(options.data);
    }

    // Since we use HttpOnly cookie for refresh token, we need to include credentials
    config.credentials = 'include';

    try {
      let response = await fetch(url, config);

      // Handle 401 Unauthorized (Token expired)
      if (response.status === 401) {
        const refreshed = await this.refreshToken();
        if (refreshed) {
          // Retry the original request with the new token
          headers.set('Authorization', `Bearer ${authState.token}`);
          response = await fetch(url, config);
        } else {
          authState.logout();
          // Optional: redirect to login
          if (typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
            window.location.href = '/login';
          }
          throw new Error('Authentication required');
        }
      }

      // Handle other errors
      if (!response.ok) {
        let errorMsg = 'An error occurred';
        try {
          const errorData = await response.json();
          errorMsg = errorData.message || errorData.title || errorMsg;
        } catch {
           errorMsg = response.statusText;
        }
        throw new Error(errorMsg);
      }

      // Handle 204 No Content
      if (response.status === 204) {
        return {} as T;
      }

      return await response.json() as T;

    } catch (error) {
      console.error('API Request Failed:', error);
      throw error;
    }
  }

  private async refreshToken(): Promise<boolean> {
    if (this.isRefreshing) {
      return this.refreshPromise!;
    }

    this.isRefreshing = true;
    this.refreshPromise = new Promise(async (resolve) => {
      try {
        const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
          method: 'POST',
          credentials: 'include' // Crucial: send the HttpOnly cookie
        });

        if (response.ok) {
          const data = await response.json();
          authState.setToken(data.accessToken);
          resolve(true);
        } else {
          resolve(false);
        }
      } catch (e) {
        resolve(false);
      } finally {
        this.isRefreshing = false;
        this.refreshPromise = null;
      }
    });

    return this.refreshPromise;
  }

  // Convenience methods
  get<T>(endpoint: string, options?: FetchOptions) {
    return this.request<T>(endpoint, { ...options, method: 'GET' });
  }

  post<T>(endpoint: string, data?: any, options?: FetchOptions) {
    return this.request<T>(endpoint, { ...options, method: 'POST', data });
  }

  put<T>(endpoint: string, data?: any, options?: FetchOptions) {
    return this.request<T>(endpoint, { ...options, method: 'PUT', data });
  }
  
  patch<T>(endpoint: string, data?: any, options?: FetchOptions) {
    return this.request<T>(endpoint, { ...options, method: 'PATCH', data });
  }

  delete<T>(endpoint: string, options?: FetchOptions) {
    return this.request<T>(endpoint, { ...options, method: 'DELETE' });
  }
}

export const api = new ApiClient();
