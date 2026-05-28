import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import type { User, AuthResponse } from '@/types';
import { authApi } from '@/api';
import toast from 'react-hot-toast';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  googleLogin: (idToken: string) => Promise<void>;
  register: (email: string, password: string, firstName: string, lastName: string, phone?: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    const token = localStorage.getItem('accessToken');
    if (storedUser && token) {
      setUser(JSON.parse(storedUser));
    }
    setIsLoading(false);
  }, []);

  const handleAuthResponse = (data: AuthResponse) => {
    const userData: User = {
      userId: data.userId,
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName,
      phone: data.phone,
      role: data.role,
    };
    setUser(userData);
    localStorage.setItem('user', JSON.stringify(userData));
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
  };

  const login = async (email: string, password: string) => {
    const response = await authApi.login({ email, password });
    handleAuthResponse(response.data);
    toast.success('Welcome back!');
  };

  const googleLogin = async (idToken: string) => {
    const response = await authApi.googleLogin(idToken);
    handleAuthResponse(response.data);
    toast.success('Welcome!');
  };

  const register = async (email: string, password: string, firstName: string, lastName: string, phone?: string) => {
    const response = await authApi.register({ email, password, firstName, lastName, phone });
    handleAuthResponse(response.data);
    toast.success('Account created successfully!');
  };

  const logout = () => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      authApi.logout(refreshToken).catch(() => {});
    }
    setUser(null);
    localStorage.removeItem('user');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    toast.success('Logged out successfully');
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isAdmin: user?.role === 'Admin',
        isLoading,
        login,
        googleLogin,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
