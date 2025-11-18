import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

export default defineConfig(({ command, mode }) => {

  const isWebComponentBuild = process.env.BUILD_TARGET === 'webcomponent'

  return {
    plugins: [
      // Activer le mode CustomElement SEULEMENT quand on build le webcomponent
      vue({
        customElement: isWebComponentBuild
      })
    ],

    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src')
      }
    },

    server: {
      port: 8080
    },

    build: isWebComponentBuild
      ? {
          // 🟦 Build spécifique Web Component
          outDir: 'dist-wc',
          lib: {
            entry: path.resolve(__dirname, 'src/webcomponents/login-webcomponent.js'),
            name: 'ConfluencesLogin',
            fileName: () => 'login-webcomponent.js',
            formats: ['es']
          },
          rollupOptions: {
            external: [],
            output: {
              format: 'es'
            }
          }
        }
      : {
          // 🟩 Build normal de ton app Vue (inchangé)
          outDir: 'dist',
        }
  }
})
