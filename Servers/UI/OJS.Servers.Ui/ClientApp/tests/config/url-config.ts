type Env = 'local' | 'real';

interface IConfig {
    FRONTEND_URL: string;
    API_BASE_URL: string;
    ADMINISTRATION_URL: string;
}

const CONFIGS: Record<Env, IConfig> = {
    local: {
        FRONTEND_URL: 'http://localhost:5002',
        API_BASE_URL: 'http://localhost:5010/api',
        ADMINISTRATION_URL: 'http://localhost:5001',
    },
    real: {
        FRONTEND_URL: 'https://dev.alpha.judge.softuni.org',
        API_BASE_URL: 'https://dev.alpha.judge.softuni.org/api',
        ADMINISTRATION_URL: 'https://admin.dev.alpha.judge.softuni.org',
    },
};

export const URL_CONFIG: IConfig = (() => {
    const env = (process.env.TEST_ENV || 'local') as Env;

    return CONFIGS[env];
})();
