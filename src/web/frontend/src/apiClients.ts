import axios from "axios";
import type { AxiosInstance, InternalAxiosRequestConfig } from "axios";
import Cookies from "js-cookie";

const AUTH_URL = import.meta.env.VITE_AUTH_URL as string;
const STORAGE_URL = import.meta.env.VITE_STORAGE_URL as string;
const WOPIHOST_URL = import.meta.env.VITE_WOPIHOST_URL as string;

const createApiClient = (baseURL: string): AxiosInstance => {
  const client = axios.create({ baseURL });

  client.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
      const token = Cookies.get("jwt");
      if (token) {
        config.headers = config.headers ?? {};
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error) => Promise.reject(error)
  );

  return client;
};

export const authApiClient = createApiClient(AUTH_URL);
export const storageApiClient = createApiClient(STORAGE_URL);
export const wopiHostApiClient = createApiClient(WOPIHOST_URL);
