import { test, expect } from '@playwright/test';

test.describe('Navigation & Layout', () => {
  test('la page login a un layout sans sidebar', async ({ page }) => {
    await page.goto('/login');
    // Auth pages use BlankLayout — no .app-sidebar
    const sidebar = page.locator('.app-sidebar, #main-sidebar');
    await expect(sidebar).toHaveCount(0);
  });

  test('la page register a un layout sans sidebar', async ({ page }) => {
    await page.goto('/register');
    const sidebar = page.locator('.app-sidebar, #main-sidebar');
    await expect(sidebar).toHaveCount(0);
  });
});

test.describe('Responsive - Mobile', () => {
  test.use({ viewport: { width: 375, height: 812 } });

  test('la page login est responsive sur mobile', async ({ page }) => {
    await page.goto('/login');
    // auth-card should be visible
    const card = page.locator('.auth-card').first();
    await expect(card).toBeVisible();
    const box = await card.boundingBox();
    expect(box).not.toBeNull();
    if (box) {
      expect(box.width).toBeLessThanOrEqual(375);
    }
  });
});

test.describe('Responsive - Desktop', () => {
  test.use({ viewport: { width: 1440, height: 900 } });

  test('la page login est bien centrée sur desktop', async ({ page }) => {
    await page.goto('/login');
    const card = page.locator('.auth-card').first();
    await expect(card).toBeVisible();
    const box = await card.boundingBox();
    expect(box).not.toBeNull();
    if (box) {
      // Card should be centered, not taking full width
      expect(box.width).toBeLessThan(600);
    }
  });
});
