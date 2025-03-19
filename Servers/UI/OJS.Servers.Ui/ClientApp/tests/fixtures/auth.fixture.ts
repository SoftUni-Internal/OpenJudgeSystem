import { test as base } from '@playwright/test';
import { URL_CONFIG } from '../config/url-config';

export const authTest = base.extend<{ auth: { login: (email: string, password: string) => Promise<void> } }>({
    auth: async ({ request, browser, context }, use) => {
        const login = async (username: string, password: string) => {
            const response = await request.post(`${URL_CONFIG.API_BASE_URL}/account/login`, {
                  data: { username , password }
            });

            if (!response.ok()) {
                 throw new Error(`Login failed: ${response.status()}`);
            }

            // Extract cookies from response
            const cookies = response.headersArray()
                .filter(header => header.name.toLowerCase() === 'set-cookie')
                .map(header => header.value)
                .map(cookie => {
                    const [name, value] = cookie.split(';')[0].split('=');
                    return {
                        name,
                        value,
                        domain: new URL(URL_CONFIG.API_BASE_URL).hostname,
                        path: '/',
                    };
                });

            await context.addCookies(cookies);
        };

        await use({ login });
    }
});

export { expect } from '@playwright/test';
