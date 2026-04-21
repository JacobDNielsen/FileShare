import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  vus: 1,
  duration: "10s",
  tlsAuth: [
    {
      cert: open("../../../.dev-certs/pem/fileshare-client.crt"),
      key: open("../../../.dev-certs/pem/fileshare-client.key"),
    },
  ],
};

export default function () {
  //const response = http.get("https://[::1]:5041/benchmark/ping");
  const response = http.get("https://localhost:5041/benchmark/ping");

  check(response, {
    "is status 200": (r) => r.status === 200,
  });

  sleep(1);
}
