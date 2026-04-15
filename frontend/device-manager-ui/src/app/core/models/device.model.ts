export interface Device {
  id: number;
  name: string;
  manufacturer: string;
  type: string;
  operatingSystem: string;
  osVersion: string;
  processor: string;
  ramAmount: number;
  description: string;
  assignedUserName?: string;
}

export interface CreateDevice {
  name: string;
  manufacturer: string;
  type: string;
  operatingSystem: string;
  osVersion: string;
  processor: string;
  ramAmount: number;
  description: string;
  assignedUserId?: number | undefined | null;
}

export interface UpdateDevice extends CreateDevice {}
