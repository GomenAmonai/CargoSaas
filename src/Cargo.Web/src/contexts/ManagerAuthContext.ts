import { createContext } from 'react';

export interface ManagerUser {
  id: string;
  firstName: string;
  username?: string;
  photoUrl?: string;
  role: string;
  tenantId: string;
}

export interface ManagerAuthContextType {
  user: ManagerUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  logout: () => void;
}

export const ManagerAuthContext = createContext<ManagerAuthContextType | undefined>(undefined);
