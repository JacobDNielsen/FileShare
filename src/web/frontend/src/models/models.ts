export interface LoginResponse {
  userName: string;
  tokenType: string;
  accessToken: string;
}

export interface GetFileInfoResponse {
  baseFileName: string;
  size: number;
  ownerId: string;
  userId: string;
  version: string;
  userCanWrite: boolean;
}

export interface GetAllFilesMetadataResponse {
  id: number;
  fileId: string;
  baseFileName: string;
  size: number;
  createdAt: string;
  lastModifiedAt: string;
}
