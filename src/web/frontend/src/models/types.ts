// Auth
export interface SignupRequest {
  userName: string;
  email: string;
  password: string;
  locale?: string;
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface AuthResponse {
  userName: string;
  tokenType: string;
  accessToken: string;
}

// Storage - GetAllFilesMetadata returns FileMetadata[]
export interface FileMetadata {
  id: number;
  fileId: string;
  baseFileName: string;
  size: number;
  createdAt: string;
  lastModifiedAt: string;
}

// Storage - Paged endpoint returns PagedResult<FileListItem>
export interface FileListItem {
  fileId: string;
  baseFileName: string;
  size: number;
  lastModifiedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

// Storage - CheckFileInfo
export interface CheckFileInfoResponse {
  baseFileName: string;
  size: number;
  ownerId: string;
  userId: string;
  version: string;
  userCanWrite: boolean;
}

// Storage - Upload response
export interface UploadResponse {
  fileId: string;
  baseFileName: string;
  size: number;
  createdAt: string;
  lastModifiedAt: string;
}

// Storage - Rename
export interface RenameRequest {
  baseFileName: string;
}

export interface RenameResponse {
  message: string;
  fileId: string;
  newName: string;
}

// Storage - Delete
export interface DeleteResponse {
  message: string;
  fileId: string;
}

export interface DeleteAllResponse {
  message: string;
  count: number;
  deletedNames: string[];
}
