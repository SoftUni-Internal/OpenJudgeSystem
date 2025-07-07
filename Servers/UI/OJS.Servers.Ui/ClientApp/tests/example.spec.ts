import { expect, test } from './fixtures/auth.fixture';

test('has title', async ({ page }) => {
    await page.goto('/') ;

    // Expect a title "to contain" a substring.
    await expect(page).toHaveTitle(/SoftUni Judge Platform/);
});

test('can log in', async ({ page, auth }) => {
    await auth.loginDefaultTestUser();
    const response = await page.goto('/profile');
    expect(response?.status()).toBe(200);
    await expect(page).toHaveURL('/profile');
});

test('can log out', async ({ page, auth }) => {
    await auth.loginDefaultTestUser();
    await page.goto('/profile');
    await expect(page).toHaveURL('/profile');

    await page.goto('/logout');

    await expect(page.getByText('You are now successfully logged out')).toBeVisible();
    await page.waitForURL('/');
    await expect(page).toHaveURL('/');

    const response = await page.goto('/profile');
    expect(response?.status()).not.toBe(200);
});
