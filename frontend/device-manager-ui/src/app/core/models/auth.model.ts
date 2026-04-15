export interface AuthUser {
  id: number;
  fullName: string;
  email: string;
  role: string;
  location: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  location: string;
}
