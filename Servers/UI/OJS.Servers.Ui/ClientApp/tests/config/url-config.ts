export const URL_CONFIG = (() => {
    const env = process.env.TEST_ENV || 'local';

    const configs = {
        local: {
            FRONTEND_URL: 'http://localhost:5002',
            API_BASE_URL: 'http://localhost:5010/api',
            ADMINISTRATION_URL: 'http://localhost:5001'
        },
        e2e: {
            FRONTEND_URL: 'https://e2e.alpha.judge.softuni.org',
            API_BASE_URL: 'https://e2e.alpha.judge.softuni.org/api',
            ADMINISTRATION_URL: 'https://admin.e2e.alpha.judge.softuni.org'
        },
        dev: {
            FRONTEND_URL: 'https://dev.alpha.judge.softuni.org',
            API_BASE_URL: 'https://dev.alpha.judge.softuni.org/api',
            ADMINISTRATION_URL: 'https://admin.dev.alpha.judge.softuni.org'
        }
    };

    return configs[env];
})();
