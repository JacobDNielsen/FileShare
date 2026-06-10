import { useState, type ReactNode } from 'react';
import Cookies from 'js-cookie';
import apiClient from '../api/apiClient';
import type { AuthResponse } from '../models/types';
import { AuthContext } from './authTypes';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoggedIn, setIsLoggedIn] = useState(() => !!Cookies.get('jwt'));
  const [username, setUsername] = useState(() => Cookies.get('username') ?? '');

  const handleAuthSuccess = (data: AuthResponse) => {
    Cookies.set('jwt', data.accessToken, { path: '/' });
    Cookies.set('username', data.userName, { path: '/' });
    setIsLoggedIn(true);
    setUsername(data.userName);
  };

  const login = async (userName: string, password: string) => {
    const { data } = await apiClient.post<AuthResponse>('/auth/login', {
      userName,
      password,
    });
    handleAuthSuccess(data);
  };

  const signup = async (userName: string, email: string, password: string) => {
    const { data } = await apiClient.post<AuthResponse>('/auth/signup', {
      userName,
      email,
      password,
    });
    handleAuthSuccess(data);
  };

  const logout = () => {
    Cookies.remove('jwt', { path: '/' });
    Cookies.remove('username', { path: '/' });
    setIsLoggedIn(false);
    setUsername('');
  };

  return (
    <AuthContext.Provider value={{ isLoggedIn, username, login, signup, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
