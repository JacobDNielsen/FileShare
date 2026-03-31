import http from "k6/http";
import { check } from "k6";
import { getEnvVariable } from "../helpers/env.js";
import { scenarioOption } from "../helpers/scenarios.js";

const SCENARIO = getEnvVariable("SCENARIO", { fallback: "smoke" });

export const options = scenarioOption(SCENARIO);

const TARGET_URL = getEnvVariable("TARGET_URL", { required: true });

function randomUser() {
  const unique = `${Date.now()}_${Math.floor(Math.random() * 10000)}`;
  return {
    username: `user_${unique}`,
    password: "Test123!",
    email: `user_${unique}@test.com`,
  };
}

export default function () {
  const user = randomUser();

  const signupRes = http.post(
    `${TARGET_URL}/api/auth/signup`,
    JSON.stringify(user),
    {
      headers: { "Content-Type": "application/json" },
      tags: { 
        name: "auth_signup" ,
        scenario: SCENARIO,
      }
    }
  );

  console.log("SIGNUP STATUS:", signupRes.status);
  console.log("SIGNUP RESPONSE:", signupRes.body);

  check(signupRes, {
    "signup 201": (r) => r.status === 201,
  });

  if (signupRes.status !== 201) {
    throw new Error("Signup failed, cannot continue test");
  }

 const body = signupRes.json();

 const token = body.accessToken;
 const tokenType = body.tokenType || "Bearer";

 if (!token) {
   throw new Error("Token not found in signup response");
 }

 const storageRes = http.get(`${TARGET_URL}/api/storage`, {
    headers: {
      Authorization: `${tokenType} ${token}`,
     
    },
    tags: {
       name: "storage_get_files",
       scenario: SCENARIO,
    }
 });


  console.log("STORAGE STATUS:", storageRes.status);
  console.log("STORAGE RESPONSE:", storageRes.body);

  check(storageRes, {
    "storage 200": (r) => r.status === 200,
  });
}