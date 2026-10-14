export interface User {
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

export interface UserState {
  token: string | null;
  user: User | null;
  isLoadingSession: boolean;

  setSession: (token: string, user: User) => Promise<void>;
  clearSession: () => Promise<void>;
  loadSession: () => Promise<void>;
}
