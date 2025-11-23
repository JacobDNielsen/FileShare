export interface LoginResponse {
  userName: string;
  tokenType: string;
  accessToken: string;
}

export interface CheckFileInfoResponse {
  baseFileName: string;
  size: number;
  ownerId: string;
  userId: string;
  version: string;
  userCanWrite: boolean;
}

export type JSONValue =
  | string
  | number
  | boolean
  | null
  | { [key: string]: JSONValue }
  | JSONValue[];
