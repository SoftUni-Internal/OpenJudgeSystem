import { test } from '@playwright/test';
import { authTest, expect } from './fixtures/auth.fixture';

test('has title', async ({ page }) => {
  await page.goto('/') ;

  // Expect a title "to contain" a substring.
  await expect(page).toHaveTitle(/SoftUni Judge Platform/);
});

authTest('can log in', async ({ page, auth }) => {
    await auth.login('judge_test', 'judge_test');
    const response = await page.goto('/profile');
    expect(response?.status()).toBe(200);
    await expect(page).toHaveURL('/profile');
});
