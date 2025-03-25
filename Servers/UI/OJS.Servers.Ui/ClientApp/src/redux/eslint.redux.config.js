export default [
    {
        files: [ 'src/redux/features/**/*.ts' ],
        rules: {
            'no-param-reassign': 'off',
            'import/group-exports': 'off',
            'import/exports-last': 'off',
            'no-unused-expressions': 'off',
            '@typescript-eslint/ban-ts-comment': 'off',
            '@typescript-eslint/no-unnecessary-type-assertions': 'off',
        },
    },
];
