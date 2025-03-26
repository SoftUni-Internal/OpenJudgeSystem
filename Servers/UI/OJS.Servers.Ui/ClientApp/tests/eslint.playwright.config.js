// eslint.playwright.config.js
import playwright from 'eslint-plugin-playwright';
import globals from 'globals';

export default {
    ...playwright.configs['flat/recommended'],
    files: [ 'tests/**/*.ts', 'tests/**/*.tsx', '**/*.spec.ts', '**/*.spec.tsx' ],
    languageOptions: { globals: { ...globals.node } },
    plugins: { playwright },
    rules: {
        ...playwright.configs['flat/recommended'].rules,
        'no-extra-parens': 'off',
        'react-hooks/rules-of-hooks': 'off',
        '@typescript-eslint/no-unused-vars': 'off',
        // Add your custom rules here
    },
};
