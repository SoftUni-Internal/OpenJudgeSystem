import { test as base } from '@playwright/test';

import { URL_CONFIG } from '../config/url-config';

export const test = base.extend<{
    auth: {
        login: (email: string, password: string) => Promise<void>;
        loginDefaultTestUser: () => Promise<void>;
            }
            }>({
                auth: async ({ request, browser, context }, use) => {
                    const login = async (username: string, password: string) => {
                        const response = await request.post(`${URL_CONFIG.API_BASE_URL}/account/login`, { data: { username , password } });

                        if (!response.ok()) {
                            throw new Error(`Login failed: ${response.status()} - ${response.statusText()}`);
                        }

                        const cookies = response.headersArray()
                            .filter(header => header.name.toLowerCase() === 'set-cookie')
                            .map(header => header.value)
                            .map(cookie => {
                                const [ name, value ] = cookie.split(';')[0].split('=');
                                return {
                                    name,
                                    value,
                                    domain: new URL(URL_CONFIG.API_BASE_URL).hostname,
                                    path: '/',
                                };
                            });

                        await context.addCookies(cookies);
                    };

                    const loginDefaultTestUser = async () => {
                        const username = process.env.TEST_USER_USERNAME;
                        const password = process.env.TEST_USER_PASSWORD;

                        if (!username || !password) {
                            throw new Error('TEST_USER_USERNAME and TEST_USER_PASSWORD must be set in the environment.');
                        }

                        await login(username, password);
                    };

                    await use({ login, loginDefaultTestUser });
                },
            });

export { expect } from '@playwright/test';
