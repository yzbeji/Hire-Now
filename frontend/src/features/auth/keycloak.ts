import Keycloak from 'keycloak-js'

export const keycloak = new Keycloak({
  url: import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8081',
  realm: 'hirenow',
  clientId: 'hirenow-frontend',
})

// Runs at module load because keycloak-js throws if initialized twice, which StrictMode effects would do.
// Nothing waits for it before rendering, so public pages never depend on Keycloak.
export const keycloakReady = keycloak
  .init({
    onLoad: 'check-sso',
    silentCheckSsoRedirectUri: `${window.location.origin}/silent-check-sso.html`,
    pkceMethod: 'S256',
    checkLoginIframe: false,
  })
  .catch((error: unknown) => console.error('Keycloak init failed', error))
