import Oidc from 'oidc-client';

const settings = {
  authority: "http://localhost:5000",           // ton IDP
  client_id: "confluences-client",
  redirect_uri: "http://localhost:8080/callback", // callback géré par Vue
  response_type: "code",
  scope: "openid profile api1",
  post_logout_redirect_uri: "http://localhost:8080/login",
  userStore: new Oidc.WebStorageStateStore({ store: window.localStorage })
};

const userManager = new Oidc.UserManager(settings);

export async function login() {
  await userManager.signinRedirect();
}

export async function completeLogin() {
  const user = await userManager.signinRedirectCallback();
  return user;
}

export async function logout() {
  await userManager.signoutRedirect();
}

export async function getUser() {
  return await userManager.getUser();
}

export default userManager;
