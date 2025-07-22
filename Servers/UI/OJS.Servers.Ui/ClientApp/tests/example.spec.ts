import { expect, test } from './fixtures/auth.fixture';

test('Has title', async ({ page }) => {
    await page.goto('/') ;

    // Expect a title "to contain" a substring.
    await expect(page).toHaveTitle(/SoftUni Judge Platform/);
});

test('Can log in', async ({ page, auth }) => {
    await auth.loginDefaultTestUser();
    const response = await page.goto('/profile');
    expect(response?.status()).toBe(200);
    await expect(page).toHaveURL('/profile');
});

test('Can log out', async ({ page, auth }) => {
    await auth.loginDefaultTestUser();
    await page.goto('/profile');
    await expect(page).toHaveURL('/profile');

    await page.goto('/logout');

    await expect(page.getByText('You are now successfully logged out')).toBeVisible();
    await expect(page).toHaveURL('/');

    const response = await page.goto('/profile');
    await expect(page).toHaveURL('/login');
});

test('Should contain the text "How to use SoftUni Judge Platform"', async ({ page }) => {
    await page.goto('/');
  
    // Check if the text is present on the page
    const text = await page.locator('text=How to use SoftUni Judge Platform');
    await expect(text).toBeVisible();
});

test('Should Check if the video is visible', async ({ page }) => {
    await page.goto('/');
  
    const videoElement = await page.locator('[data-title="Guidelines for working with the SoftUni Judge platform"]');
    
    // Check if the video is visible
    await expect(videoElement).toBeVisible();
});

test('Verify Programming Basics dropdown contents', async ({ page }) => {
    // Navigate to the SoftUni Judge platform
    await page.goto('/');
    
    // Wait for the dropdown to be visible and click it
    const dropdown = await page.getByText('Programming Basics', { exact: true }).click();
    
    // Define the expected menu items in order
    const expectedItems = [
      'C# Basics',
      'Java Basics',
      'JS Basics',
      'Python Basics',
      'PB - More Exercises',
      'PB - Exams',
      'C++ Basics',
      'Go Basics'
    ];
  
    // Verify each menu item exists and is visible
    for (const itemText of expectedItems) {
      const item = await page.getByRole('link', { name: itemText });
      await expect(item).toBeVisible();
    }
  });

  test('Verify Software Technologies dropdown contents', async ({ page }) => {
    // Navigate to the SoftUni Judge platform
    await page.goto('/');
    
    // Wait for the dropdown to be visible and click it
    const dropdownQaulityAssurance = await page.getByText('Quality Assurance', { exact: true }).click();
    const dropdownQaFundamentals = await page.getByText('QA Fundamentals', { exact: true }).click();
    const dropdownSoftwareTechnologies = await page.getByText('Software Technologies', { exact: true }).click();
    // Define the expected menu items in order
    const expectedItems = [
      'Software Technologies - Exercises',
      'Software Technologies - Exams'
    ];
  
    // Verify each menu item exists and is visible
    for (const itemText of expectedItems) {
      const item = await page.getByRole('link', { name: itemText });
      await expect(item).toBeVisible();
    }
  });

  test('Verify QA Fundamentals and Manual Testing dropdown contents', async ({ page }) => {
    // Navigate to the SoftUni Judge platform
    await page.goto('/');
    
    // Wait for the dropdown to be visible and click it
    const dropdownQaulityAssurance = await page.getByText('Quality Assurance', { exact: true }).click();
    const dropdownQaFundamentals = await page.getByText('QA Fundamentals', { exact: true }).click();
    const dropdownSoftwareTechnologies = await page.getByText('QA Fundamentals and Manual Testing', { exact: true }).click();
    // Define the expected menu items in order
    const expectedItems = [
      'QA Fundamentals and Manual Testing - Exams'
    ];
  
    // Verify each menu item exists and is visible
    for (const itemText of expectedItems) {
      const item = await page.getByRole('link', { name: itemText });
      await expect(item).toBeVisible();
    }
  });

  test('Verify Programming Fundamentals and Unit Testing dropdown contents', async ({ page }) => {
    // Navigate to the SoftUni Judge platform
    await page.goto('/');
    
    // Wait for the dropdown to be visible and click it
    const dropdownQaulityAssurance = await page.getByText('Quality Assurance', { exact: true }).click();
    const dropdownQaFundamentals = await page.getByText('Programming for QA', { exact: true }).click();
    const dropdownSoftwareTechnologies = await page.getByText('Programming Fundamentals and Unit Testing', { exact: true }).click();
    // Define the expected menu items in order
    const expectedItems = [
      'Programming Fundamentals and Unit Testing - Exercises',
      'Programming Fundamentals and Unit Testing - Exams'
    ];
  
    // Verify each menu item exists and is visible
    for (const itemText of expectedItems) {
      const item = await page.getByRole('link', { name: itemText });
      await expect(item).toBeVisible();
    }
  });

  test('Verify Programming Advanced for QA dropdown contents', async ({ page }) => {
    // Navigate to the SoftUni Judge platform
    await page.goto('/');
    
    // Wait for the dropdown to be visible and click it
    const dropdownQaulityAssurance = await page.getByText('Quality Assurance', { exact: true }).click();
    const dropdownQaFundamentals = await page.getByText('Programming for QA', { exact: true }).click();
    const dropdownSoftwareTechnologies = await page.getByText('Programming Advanced for QA', { exact: true }).click();
    // Define the expected menu items in order
    const expectedItems = [
      'Programming Advanced for QA - Exercises',
      'Programming Advanced for QA - Exams'
    ];
  
    // Verify each menu item exists and is visible
    for (const itemText of expectedItems) {
      const item = await page.getByRole('link', { name: itemText });
      await expect(item).toBeVisible();
    }
  });

  test('Verify Back-End Test Automation dropdown contents', async ({ page }) => {
    // Navigate to the SoftUni Judge platform
    await page.goto('/');
    
    // Wait for the dropdown to be visible and click it
    const dropdownQaulityAssurance = await page.getByText('Quality Assurance', { exact: true }).click();
    const dropdownQaFundamentals = await page.getByText('Back-End Test Automation', { exact: true }).click();
    // Define the expected menu items in order
    const expectedItems = [
      'Back-End Technologies Basics - Exercises',
      'Back-End Technologies Basics - Exams'
    ];
  
    // Verify each menu item exists and is visible
    for (const itemText of expectedItems) {
      const item = await page.getByRole('link', { name: itemText });
      await expect(item).toBeVisible();
    }
  });

  test('Contests page loads successfully', async ({ page }) => {
    await page.goto('/contests');
    await expect(page).toHaveTitle(/Contests/i);
  });

  test('Contests list is visible', async ({ page }) => {
    await page.goto('/contests');
    const contestItems = await page.locator('._contestsListContainer_15pps_57');
    await expect(contestItems.first()).toBeVisible();
  });

  test('Page works without JavaScript', async ({ browser }) => {
    const context = await browser.newContext({ javaScriptEnabled: false });
    const page = await context.newPage();
    await page.goto('/contests');
    const bodyText = await page.textContent('body');
    expect(bodyText).toContain('You need to enable JavaScript to run this app.');
    await context.close();
  });
