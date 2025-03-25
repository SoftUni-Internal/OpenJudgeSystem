import { defineConfig } from "eslint/config";
import globals from "globals";
import js from "@eslint/js";
import tseslint from "typescript-eslint";
import reactPlugin from 'eslint-plugin-react';
import reactHooks from 'eslint-plugin-react-hooks';
import importPlugin from 'eslint-plugin-import';
import promisePlugin from 'eslint-plugin-promise';
import jsxA11yPlugin from 'eslint-plugin-jsx-a11y';
import simpleImportSortPlugin from 'eslint-plugin-simple-import-sort';
import reduxConfig from './src/redux/eslint.redux.config.js';
import playwrightConfig from './tests/eslint.playwright.config.js';

export default defineConfig([
  tseslint.configs.recommendedTypeChecked,
  reactPlugin.configs.flat.recommended,
  importPlugin.flatConfigs.recommended,
  reactHooks.configs['recommended-latest'],
  promisePlugin.configs['flat/recommended'],
  {
    files: ["**/*.{js,mjs,cjs,ts,jsx,tsx}"],
    settings: {
      'import/resolver': {
        typescript: {},
      },
      'react': {
        version: 'detect',
      },
    },
    languageOptions: {
        globals: globals.browser,
        parser: tseslint.parser,
        parserOptions: {
            ecmaFeatures: { jsx: true },
            sourceType: 'module',
            projectService: true,
            tsconfigRootDir: import.meta.dirname,
        },
    },
    plugins: {
        'js': js,
        'jsx-a11y': jsxA11yPlugin,
        'simple-import-sort': simpleImportSortPlugin,
        '@typescript-eslint': tseslint.plugin,
    },
    extends: [
        "js/recommended",
    ],
    rules: {
        'no-unused-vars': 'off',
        'no-undefined': 'off',
        'no-use-before-define': 'off',
        'prefer-destructuring': 'off',
        'react/no-array-index-key': 'off',
        'react/react-in-jsx-scope': 'off',

        'node/no-missing-import': 0,
        'sort-imports': 0,
        'no-confusing-arrow': 0,
        'node/no-unsupported-features/es-syntax': 0,

        '@typescript-eslint/no-unused-vars': 'error',
        '@typescript-eslint/no-explicit-any': 'warn',
        '@typescript-eslint/no-unsafe-assignment': 'warn',
        '@typescript-eslint/no-unsafe-member-access': 'warn',
        '@typescript-eslint/no-unsafe-call': 'warn',
        '@typescript-eslint/no-unsafe-return': 'warn',
        '@typescript-eslint/no-unsafe-function-type': 'warn',
        '@typescript-eslint/no-unsafe-argument': 'warn',
        '@typescript-eslint/only-throw-error': 'warn',
        '@typescript-eslint/no-misused-promises': 'warn',
        '@typescript-eslint/no-floating-promises': 'warn',
        '@typescript-eslint/no-use-before-define': [ 'error' ],
        '@typescript-eslint/naming-convention': [
            'error',
            {
                selector: 'interface',
                format: [ 'PascalCase' ],
                custom: {
                    regex: '^I[A-Z]',
                    match: true,
                },
            },
        ],

        'react/display-name': 'warn',
        'react/jsx-filename-extension': [ 'warn', { extensions: [ '.tsx' ] } ],
        'react/jsx-props-no-spreading': ['warn', { custom: 'ignore' }],
        'import/extensions': [
            'error',
            'ignorePackages',
            {
                ts: 'never',
                tsx: 'never',
            },
        ],
        'react-hooks/rules-of-hooks': 'error',
        'react-hooks/exhaustive-deps': 'error',

        'react/function-component-definition': [ 'error',
            {
                namedComponents: 'arrow-function',
                unnamedComponents: 'arrow-function',
            },
        ],

        'array-bracket-spacing': [ 'error', 'always', { singleValue: true } ],
        // enforces curly brackets for arrow functions only when needed
        'arrow-body-style': [ 'error', 'as-needed' ],
        // treat var statements as if they were block scoped (off by default)
        'block-scoped-var': [ 'error' ],
        // comma is required at the end of each line, when in object or arrays
        'comma-dangle': [ 'error', 'always-multiline' ],
        // require return statements to either always or never specify values
        'consistent-return': 'error',
        // specify curly brace conventions for all control statements
        curly: [ 'error', 'all' ],
        // require default case in switch statements (off by default)
        'default-case': [ 'error' ],
        // encourages use of dot notation whenever possible
        'dot-notation': [ 'error' ],
        // enfores files to end with a new line
        'eol-last': 'error',
        // require the use of === and !==
        eqeqeq: 'error',
        // anonymous are not obliged to have a name
        'func-names': 'off',
        // enforces the use of function expressions for more predictable code
        'func-style': [ 'error', 'expression' ],
        // function params are either on the same line or each is on a new line
        'function-paren-newline': [ 'error', 'multiline' ],
        // allows require(__PACKAGE_NAME__) everywhere in the file
        'global-require': 'off',
        // make sure for-in loops have an if statement (off by default)
        'guard-for-in': 'error',
        // indent is 4 spaces
        'indent': [
            'error',
            4,
            {
                ignoredNodes:
                    [
                        'JSXElement',
                        'JSXElement > *',
                        'JSXAttribute',
                        'JSXIdentifier',
                        'JSXNamespacedName',
                        'JSXMemberExpression',
                        'JSXSpreadAttribute',
                        'JSXExpressionContainer',
                        'JSXOpeningElement',
                        'JSXClosingElement',
                        'JSXText',
                        'JSXEmptyExpression',
                        'JSXSpreadChild' ],
            } ],
        // enforces consistent spacing between keys and values in object literal properties
        'key-spacing': [
            'error',
            { mode: 'strict' } ],
        // linebreak unix (LF) is better than Windows (CRLF)
        'linebreak-style': [ 'error', 'unix' ],
        // max 140 characters for code, and 100 for comments
        'max-len': [ 'warn', {
            tabWidth: 4,
            code: 140,
            comments: 100,
            ignoreUrls: true,
            ignorePattern: '^import\\s.+\\sfrom\\s.+;$',
        } ],
        // enforces ternary on multiple lines
        'multiline-ternary': [ 'error', 'always' ],
        // disallow using an async function as a Promise executor
        'no-async-promise-executor': 'error',
        // disallow use of arguments.caller or arguments.callee
        'no-caller': 'error',
        // disallow the catch clause parameter name being
        // the same as a variable in the outer scope (off by default in the node environment)
        'no-catch-shadow': 'error',
        // disallow assignment in conditional expressions
        'no-cond-assign': 'error',
        // console.log() is allowed
        'no-console': 'warn',
        // disallow use of constant expressions in conditions
        'no-constant-condition': 'error',
        // disallow use of debugger
        'no-debugger': 'error',
        // disallow deletion of variables
        'no-delete-var': 'error',
        // disallow duplicate keys when creating object literals
        'no-dupe-keys': 'error',
        // disallow else after a return in an if (off by default)
        'no-else-return': 'error',
        // disallow empty statements
        'no-empty': 'error',
        // disallow assigning to the exception in a catch block
        'no-ex-assign': 'error',
        // disallow adding to native types
        'no-extend-native': 'error',
        // disallow unnecessary function binding
        'no-extra-bind': 'error',
        // disallow double-negation boolean casts in a boolean context
        'no-extra-boolean-cast': 'error',
        // disallow unnecessary parentheses (off by default)
        'no-extra-parens': 'warn',
        // disallow unnecessary semicolons
        'no-extra-semi': 'error',
        // disallow fallthrough of case statements
        'no-fallthrough': 'error',
        // disallow the use of leading or trailing
        // decimal points in numeric literals (off by default)
        'no-floating-decimal': 'error',
        // disallow overwriting functions written as function declarations
        'no-func-assign': 'error',
        // disallow function or variable declarations in nested blocks
        'no-inner-declarations': 'error',
        // disallow irregular whitespace outside of strings and comments
        'no-irregular-whitespace': 'error',
        // disallow creation of functions within loops
        'no-loop-func': 'error',
        // disallow use of multiple spaces
        'no-multi-spaces': 'error',
        // disallow use of multiline strings
        'no-multi-str': 'error',
        // disallow reassignments of native objects
        'no-native-reassign': 'error',
        // disallow negation of the left operand of an in expression
        'no-negated-in-lhs': 'error',
        // nested ternary is not bad
        'no-nested-ternary': 'off',
        // disable 'new ClassType()' without assigning the object
        'no-new': 'error',
        // disallow use of new operator for Function object
        'no-new-func': 'error',
        // disallows creating new instances of String, Number, and Boolean
        'no-new-wrappers': 'error',
        // disallow the use of object properties of the global object (Math and JSON) as functions
        'no-obj-calls': 'error',
        // disallow use of octal literals
        'no-octal': 'error',
        // disallow use of octal escape sequences in string literals,
        // such as var foo = "Copyright \251";
        'no-octal-escape': 'error',
        // allows calling built-ins
        'no-prototype-builtins': 'off',
        // disallow declaring the same variable more then once
        'no-redeclare': 'error',
        // disallows unnecessary return await
        'no-return-await': 'error',
        // disallow use of assignment in return statement
        'no-return-assign': 'error',
        // disallow comparisons where both sides are exactly the same (off by default)
        'no-self-compare': 'error',
        // disallow use of comma operator
        'no-sequences': 'error',
        // disallow declaration of variables already declared in the outer scope
        'no-shadow': 'off',
        '@typescript-eslint/no-shadow': [ 'error' ],
        // disallow shadowing of names such as arguments
        'no-shadow-restricted-names': 'error',
        // disallow sparse arrays
        'no-sparse-arrays': 'error',
        // disallow undeclared variables
        'no-undef': 'error',
        // disallow use of undefined when initializing variables
        'no-undef-init': 'error',
        // disallow the usage of _ af both ends of an object name,
        // except with class/object functions
        'no-underscore-dangle': [
            'error', {
                allowAfterThis: true,
                allowAfterSuper: true,
            },
        ],
        // disallow unreachable statements after
        // a return, throw, continue, or break statement
        'no-unreachable': 'error',
        // disallow usage of expressions in statement position
        'no-unused-expressions': 'error',
        // enforce consistent line breaks inside braces
        'object-curly-newline': [
            'error', {
                ObjectExpression: { multiline: true },
                ObjectPattern: { multiline: true },
            } ],
        // enforce consistent spacing inside braces
        'object-curly-spacing': [ 'error', 'always' ],
        // enforce consistent linebreak style for operators
        'operator-linebreak': [ 'error', 'after', { overrides: { '?': 'before', ':': 'before' } } ],
        // Suggest using const
        'prefer-const': 'error',

        // Prefer using concise optional chain expressions instead of chained logical ands
        '@typescript-eslint/prefer-optional-chain': 'error',

        // require quotes around object literal property names only when needed
        'quote-props': [ 'error', 'as-needed' ],
        // Use single quotes
        quotes: [ 'error', 'single' ],
        // semicolons at the end of each line
        semi: 'error',
        // space-before-function-paren
        'space-before-function-paren': [ 'error', {
            anonymous: 'always',
            named: 'never',
            asyncArrow: 'always',
        } ],
        // Disallow spaces inside of parentheses
        'space-in-parens': [ 'error', 'never' ],

        /* import */
        // 'import/first': 'error',
        // 'import/prefer-default-export': 'error',
        // 'import/no-unused-modules': ['error', {
        //     unusedExports: true,
        //     missingExports: true,
        // }],

        // /* react rules */
        'react/jsx-indent': [ 'error', 4 ],
        // No sense in TypeScript context
        'react/prop-types': 'off',
        // No sense in TypeScript context
        'react/require-default-props': 'off',

        /* promise rules */
        'promise/always-return': 'error',
        'promise/no-return-wrap': 'error',
        'promise/param-names': 'error',
        'promise/catch-or-return': 'error',
        'promise/no-native': 'off',
        'promise/no-nesting': 'error',
        'promise/no-promise-in-callback': 'error',
        'promise/no-callback-in-promise': 'warn',
        'promise/avoid-new': 'off',
        'promise/no-new-statics': 'error',
        'promise/no-return-in-finally': 'warn',
        'promise/valid-params': 'warn',
        'promise/prefer-await-to-then': 'error',
        'promise/prefer-await-to-callbacks': 'error',

        /* html elements */
        'jsx-a11y/label-has-associated-control': [
            'error',
            { required: { some: [ 'nesting', 'id' ] } } ],
        'jsx-a11y/label-has-for': [ 'error', { required: { some: [ 'nesting', 'id' ] } } ],
        'simple-import-sort/imports': [
            'error',
            {
                groups: [
                    // Packages `react` related packages come first.
                    [ '^react', '^@?\\w' ],
                    // Internal packages.
                    [ '^(@|components)(/.*|$)' ],
                    // Side effect imports.
                    [ '^\\u0000' ],
                    // Parent imports. Put `..` last.
                    [ '^\\.\\.(?!/?$)', '^\\.\\./?$' ],
                    // Other relative imports. Put same-folder imports and `.` last.
                    [ '^\\./(?=.*/)(?!/?$)', '^\\.(?!/?$)', '^\\./?$' ],
                    // Style imports.
                    [ '^.+\\.?(css)$' ],
                ],
            },
        ],
        'no-restricted-imports': [
            2,
            {
                paths: [
                    {
                        name: 'lodash',
                        message:
                            'Do not import from `lodash` directly, as we don\'t support tree-shaking for it.' +
                            ' Instead, import the function you\'re trying to use, e.g. `import debounce from \'lodash/debounce\'`',
                    },
                    {
                        name: 'react-icons/all',
                        message:
                            'Do not import from `react-icons/all` directly, but use concrete file:' +
                            ' i.e. instead of `import {...} from \'react-icons/all\'` use `import { ... } from \'react-icons/md\'`',
                    },
                    // { name: '@mui/material' },
                ],
            },
        ],
        '@typescript-eslint/explicit-module-boundary-types': 0,
    },
  },
  playwrightConfig,
  reduxConfig,
]);