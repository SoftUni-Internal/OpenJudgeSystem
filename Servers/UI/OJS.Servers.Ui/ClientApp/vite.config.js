import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
import svgr from 'vite-plugin-svgr';
import { visualizer } from "rollup-plugin-visualizer";
import { resolve } from 'path';
/// <reference types="vite-plugin-svgr/client" />

// For development server, we want to forward all requests to /administration to /admin.html
const forwardToAdmin = () => {
    return {
        name: 'forward-to-admin-html',
        apply: 'serve',
        enforce: 'post',
        configureServer(server) {
            server.middlewares.use('/', (req, _, next) => {
                if (req.url.startsWith('/administration')) {
                    req.url = '/admin.html';
                }
                next()
            })
        },
    }
}

export default defineConfig(({ mode }) => {
    const isUnminified = process.argv.includes('--unminified');

    return ({
        appType: 'mpa', // Multi Page Application
        build: {
            minify: isUnminified ? false : 'esbuild',
            sourcemap: isUnminified,
            rollupOptions: {
                input: {
                    main: resolve(__dirname, 'index.html'),
                    admin: resolve(__dirname, 'admin.html')
                },
                onwarn(warning, warn) {
                    if (warning.code === 'MODULE_LEVEL_DIRECTIVE') {
                        return
                    }
                    warn(warning)
                }
            },
        },
        plugins: [
            react(),
            svgr(),
            forwardToAdmin(),
            visualizer({open: true, filename: 'bundle-analysis.html'}),
        ],
        server: {port: 5002},
        resolve: {
            alias: {
                'src': resolve(__dirname, 'src'),
            },
        },
    });
});
