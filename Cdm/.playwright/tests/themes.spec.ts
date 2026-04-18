import { test, expect } from '@playwright/test';

test.describe('Thèmes', () => {
  test('la page login utilise le thème sombre par défaut', async ({ page }) => {
    await page.goto('/login');
    const html = page.locator('html');
    const theme = await html.getAttribute('data-theme');
    // App.razor uses data-theme="dark" by default
    expect(theme).toBe('dark');
  });

  test('les styles CSS sont chargés', async ({ page }) => {
    await page.goto('/login');
    // Check that our CSS is loaded by verifying the auth card is styled
    const authCard = page.locator('.auth-card').first();
    // Wait for Blazor to render (InteractiveServer with prerender:false)
    await expect(authCard).toBeVisible();
    const box = await authCard.boundingBox();
    // Card must be constrained by max-width (not full viewport)
    expect(box).not.toBeNull();
    if (box) {
      expect(box.width).toBeLessThan(600);
    }
  });

  test('les fonts Google sont référencées', async ({ page }) => {
    await page.goto('/login');
    const fontLinks = await page.locator('link[href*="fonts.googleapis.com"]').count();
    expect(fontLinks).toBeGreaterThan(0);
  });

  test('le fichier themes.css est chargé', async ({ page }) => {
    await page.goto('/login');
    const themeLink = await page.locator('link[href*="themes"]').count();
    expect(themeLink).toBeGreaterThan(0);
  });
});
