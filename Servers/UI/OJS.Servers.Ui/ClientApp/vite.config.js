import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
import svgr from 'vite-plugin-svgr';
import { visualizer } from "rollup-plugin-visualizer";
import { resolve } from 'path';
/// <reference types="vite-plugin-svgr/client" />

const isUnminified = process.env.UNMINIFIED === 'true';

// For development server, we want to fall back all requests to /index.html, so links can be opened in new tabs
const fallbackHtmlRoutes  = () => {
    return {
        name: 'fallback-html-routes',
        apply: 'serve',
        enforce: 'post',
        configureServer(server) {
            server.middlewares.use((req, res, next) => {
                const isHtmlRequest =
                    req.headers.accept?.includes('text/html') &&
                    !req.url.includes('.'); // ignore requests with file extensions

                if (isHtmlRequest) {
                    if (req.url.startsWith('/administration')) {
                        req.url = '/admin.html';
                    } else if (!req.url.startsWith('/api')) {
                        req.url = '/index.html';
                    }
                }

                next();
            });
        },
    };
};

export default defineConfig(({ mode }) =>
    ({
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
            fallbackHtmlRoutes(),
            visualizer({open: true, filename: 'bundle-analysis.html'}),
        ],
        server: {port: 5002},
        resolve: {
            alias: {
                'src': resolve(__dirname, 'src'),
            },
        },
    }));
