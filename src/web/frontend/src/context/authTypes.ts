import { createContext } from 'react';

export interface AuthContextType {
  isLoggedIn: boolean;
  username: string;
  login: (userName: string, password: string) => Promise<void>;
  signup: (userName: string, email: string, password: string) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | null>(null);
