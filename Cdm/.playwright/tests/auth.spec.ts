import { test, expect, Page } from '@playwright/test';

// Unique test user to avoid conflicts
const timestamp = Date.now();
const TEST_USER = {
  email: `test_pw_${timestamp}@cdm-test.com`,
  password: 'Test@1234!',
  nickname: `Testeur_${timestamp}`,
};

test.describe('Authentification', () => {
  test('la page de login est accessible', async ({ page }) => {
    await page.goto('/login');
    await expect(page.locator('text=Chronique')).toBeVisible();
    await expect(page.locator('input[type="email"], input#email, input[name*="Email"]').first()).toBeVisible();
    await expect(page.locator('input[type="password"]').first()).toBeVisible();
  });

  test('la page de register est accessible', async ({ page }) => {
    await page.goto('/register');
    await expect(page.locator('text=Chronique')).toBeVisible();
    await expect(page.locator('input[type="email"], input#email, input[name*="Email"]').first()).toBeVisible();
  });

  test('les routes protégées redirigent vers login', async ({ page }) => {
    await page.goto('/worlds');
    // Blazor Server uses client-side redirect via forceLoad — wait for login form or URL change
    await Promise.race([
      page.waitForURL('**/login**', { timeout: 20000 }).catch(() => {}),
      page.locator('input#email, input[placeholder*="Email"], form input[type="email"]').first().waitFor({ timeout: 20000 }).catch(() => {}),
    ]);
    // After redirect, either URL changed or login form is visible
    const hasLoginForm = await page.locator('input#email, form').first().isVisible().catch(() => false);
    const isOnLogin = page.url().includes('/login');
    expect(hasLoginForm || isOnLogin).toBeTruthy();
  });

  test("la page d'accueil redirige vers login si non authentifié", async ({ page }) => {
    await page.goto('/');
    await Promise.race([
      page.waitForURL('**/login**', { timeout: 20000 }).catch(() => {}),
      page.locator('input#email, input[placeholder*="Email"], form input[type="email"]').first().waitFor({ timeout: 20000 }).catch(() => {}),
    ]);
    const hasLoginForm = await page.locator('input#email, form').first().isVisible().catch(() => false);
    const isOnLogin = page.url().includes('/login');
    expect(hasLoginForm || isOnLogin).toBeTruthy();
  });

  test('lien vers inscription depuis login', async ({ page }) => {
    await page.goto('/login');
    const registerLink = page.locator('a[href*="register"]');
    await expect(registerLink).toBeVisible();
    await registerLink.click();
    await page.waitForURL('**/register**');
    expect(page.url()).toContain('/register');
  });

  test('lien vers connexion depuis register', async ({ page }) => {
    await page.goto('/register');
    const loginLink = page.locator('a[href*="login"]');
    await expect(loginLink).toBeVisible();
    await loginLink.click();
    await page.waitForURL('**/login**');
    expect(page.url()).toContain('/login');
  });
});
